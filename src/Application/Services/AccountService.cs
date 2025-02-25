using Application.Contracts;
using Application.Extensions;
using Domain.Aggregates;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public sealed class AccountService(IAccountRepository accountRepository, IAccountTypeRepository accountTypeRepository) : IAccountService
{
    public async Task<IResult> CreateAsync(CreateAccountRequest request)
    {
        Account? parent = null;
        AccountType? accountType = null;

        if (request.ParentId is not null)
        {
            parent = await accountRepository.GetAsync(request.ParentId);

            if (parent is null)
            {
                return Results.BadRequest("Parent account not found");
            }

            if (request.AccountTypeId is not null && !request.AccountTypeId.Equals(parent.AccountTypeId))
            {
                return Results.BadRequest("Child account can't have type different from parent account");
            }
        }

        if (request.AccountTypeId is not null)
        {
            accountType = await accountTypeRepository.GetAsync(request.AccountTypeId);

            if (accountType is null)
            {
                return Results.BadRequest("Accoutn type not found");
            }
        }

        var codeTaken = accountRepository.ListAsNoTracking().Any(a => a.Code == request.Code);
        if (codeTaken)
        {
            return Results.BadRequest("Code already in use");
        }

        var accountTypeId = parent is not null
            ? parent.AccountTypeId
            : accountType!.Id;

        var account = new Account(
            request.Name, request.Code, request.Description,
            request.CanHaveEntries, request.ParentId, accountTypeId);

        await accountRepository.CreateAsync(account);
        await accountRepository.SaveChangesAsync();

        return Results.Created("api/v1/accounts", new CreateAccountResponse(
            account.Id, account.Name, account.Code, account.Description,
            account.CanHaveEntries, account.ParentId, account.AccountTypeId));
    }

    public async Task<IResult> DeleteAsync(DeleteAccountRequest request)
    {
        var account = accountRepository.List()
            .FirstOrDefault(a => a.Id == request.Id);

        if (account is null)
        {
            return Results.BadRequest("Entity not found");
        }

        if (account.CanHaveEntries && account.ChildAccounts.Count > 0)
        {
            return Results.BadRequest("Can't remove an account with child accounts");
        }

        await accountRepository.DeleteAsync(account);
        await accountRepository.SaveChangesAsync();

        return Results.NoContent();
    }

    public async Task<IResult> UpdateAsync(int id, UpdateAccountRequest request)
    {
        var account = accountRepository.List()
                .Include(x => x.Parent)
                .Include(x => x.AccountType)
            .FirstOrDefault(a => a.Id == id);

        if (account is null)
        {
            return Results.BadRequest("Entity not foud");
        }

        if (!string.IsNullOrEmpty(request?.Code) && !request.Code.Equals(account.Code))
        {
            if (account.Parent is not null)
            {
                return Results.BadRequest("Can't change code on a account with a parent account");
            }

            if (account.CanHaveEntries && account.ChildAccounts.Count > 0)
            {
                return Results.BadRequest("Can't change code of a account with child accounts");
            }

            if (accountRepository.List().Any(e => e.Code.Equals(request.Code)))
            {
                return Results.BadRequest("Code already in use");
            }
        }

        if (request?.AccountTypeId is not null && !request.AccountTypeId.Equals(account.AccountTypeId))
        {
            if (account.Parent is not null)
            {
                return Results.BadRequest("Can't change type on a account with a parent account");
            }

            if ((account.CanHaveEntries && account.ChildAccounts.Count > 0))
            {
                return Results.BadRequest("Can't change type on a account with child accounts");
            }

            var typeExists = accountTypeRepository.ListAsNoTracking().Any(t => t.Id == request.AccountTypeId);

            if (!typeExists)
            {
                return Results.BadRequest("Invalid account type");
            }
        }

        if (request.ParentId is not null)
        {
            if (account.CanHaveEntries && account.ChildAccounts.Count > 0)
            {
                return Results.BadRequest("Can't change a parent of an account with entries");
            }

            var newParent = await accountRepository.GetAsync(request.ParentId);

            if (newParent is null)
            {
                return Results.BadRequest("Invalid parent account id");
            }
        }

        account.Update(request.Code, request.Name, request.Description, request.CanHaveEntries, request.AccountTypeId, request.ParentId);

        await accountRepository.SaveChangesAsync();

        return Results.Ok(new UpdateAccountResponse(
            account.Id, account.Name, account.Code, account.Description, account.CanHaveEntries, account.ParentId, account.AccountTypeId));
    }

    public async Task<IResult> ListAsync(ListAccountsRequest request)
    {
        var results = accountRepository.ListAsNoTracking()
                .Include(account => account.Parent)
                .Include(account => account.AccountType)
            .WhereIf(request.Id is not null, accountType => accountType.Id == request.Id)
            .WhereIf(request.Code is not null, accountType => accountType.Code == request.Code)
            .WhereIf(request.Name is not null, accountType => accountType.Name == request.Name)
            .WhereIf(request.ParentId is not null, accountType => accountType.ParentId == request.ParentId)
            .WhereIf(request.Description is not null, accountType => accountType.Description == request.Description)
            .WhereIf(request.AccountTypeId is not null, accountType => accountType.AccountTypeId == request.AccountTypeId)
            .WhereIf(request.CanHaveEntries is not null, accountType => accountType.CanHaveEntries == request.CanHaveEntries)
                .Skip(request.Page == 1 ? 0 : request.Page * request.PageSize)
                .Take(request.PageSize)
            .Select(account => new ListAccountsResponseItem(
                account.Id, account.Name, account.Code.ToString(), account.Description, account.CanHaveEntries,
                account.ParentId, account.AccountTypeId))
            .ToList();

        return Results.Ok(new PaginatedResponse<ListAccountsResponseItem>(
            results, request.Page, request.PageSize));
    }

    public async Task<IResult> GetNextCodeAsync(GetNextCodeRequest request)
    {
        if (request.AccountId is null)
        {
            if (!accountRepository.ListAsNoTracking().Any())
            {
                return Results.Ok(new GetNextCodeResponse("1"));
            }

            var allCodes = accountRepository.ListAsNoTracking()
                .ToList()
                .ToDictionary(x => x.Code, x => Convert.ToInt32(x.Code.Code.Replace(".", "")));

            var smallestMissing = FindSmallestMissingNumber(allCodes.Values.ToList());

            return Results.Ok(new GetNextCodeResponse(smallestMissing.ToString()));
        }

        var account = accountRepository.ListAsNoTracking()
                .Include(a => a.ChildAccounts)
           .FirstOrDefault(a => a.Id == request.AccountId);

        if (account is null)
        {
            return Results.NotFound();
        }

        var existingCodes = account.ChildAccounts
            .Select(a => a.Code.ToString())
            .ToList();

        var nextCode = GetNextCode(existingCodes, account.Code);

        return Results.Ok(new GetNextCodeResponse(nextCode));
    }

    public async Task<IResult> GetAsync(GetAccountRequest request)
    {
        var result = await accountRepository.GetAsync(request.Id);

        return result is not null
            ? Results.Ok(new GetAccountResponse(result.Id, result.Name, result.Code, result.Description, result.CanHaveEntries, result.ParentId, result.AccountTypeId))
            : Results.NotFound();
    }

    private string GetNextCode(List<string> existingCodes, string parentCode)
    {
        const int MAX_VALUE = 999;

        if (existingCodes.Count <= 0)
        {
            return $"{parentCode}.1";
        }

        var lastSteps = existingCodes
            .Select(x => Convert.ToInt32(x.Split('.').Last()))
            .ToList();

        var nextNumber = FindSmallestMissingNumber(lastSteps);
        var previousLastStep = nextNumber - 1;

        if (nextNumber < MAX_VALUE)
        {
            var suggestedCode =
                existingCodes.First(c =>
                    c.EndsWith($".{previousLastStep}")).Replace($".{previousLastStep}", $".{nextNumber}");

            return suggestedCode;
        }

        var parentLevelCode = string.Join('.',
            parentCode.Split('.').ToList().Take(parentCode.Split('.').Count() - 1));

        var parent = accountRepository.ListAsNoTracking()
                .Include(a => a.ChildAccounts)
            .FirstOrDefault(a => a.Code == parentLevelCode);

        if (parent is null)
        {
            return GetNextCode([], parentCode);
        }

        existingCodes = parent.ChildAccounts
           .Select(a => a.Code.ToString())
           .ToList();

        return GetNextCode(existingCodes, parent.Code);
    }

    private int FindSmallestMissingNumber(List<int> numbers)
    {
        numbers.Order();

        for (int i = 0; i < numbers.Count - 1; i++)
        {
            if (numbers[i + 1] != numbers[i] + 1)
            {
                return numbers[i] + 1;
            }
        }

        return numbers.Last() + 1;
    }
}
