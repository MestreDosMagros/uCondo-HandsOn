using Application.Contracts;
using Application.Extensions;
using Domain.Aggregates;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;

namespace Application.Services;

public sealed class AccountTypeService(IAccountTypeRepository accountTypeRepository, IAccountRepository accountRepository) : IAccountTypeService
{
    public async Task<IResult> CreateAsync(CreateAccountTypeRequest request)
    {
        var nameExists = accountTypeRepository.ListAsNoTracking()
            .Any(t => t.Name.ToLower().Equals(request.Name.ToLower()));

        if (nameExists)
        {
            return Results.BadRequest("Name already in use");
        }

        var accountType = new AccountType(request.Name, request.Description);

        await accountTypeRepository.CreateAsync(accountType);
        await accountTypeRepository.SaveChangesAsync();

        return Results.Created("api/v1/account-types", new CreateAccountTypeResponse(
            accountType.Id, accountType.Name, accountType.Description));
    }

    public async Task<IResult> UpdateAsync(int id, UpdateAccountTypeRequest request)
    {
        var accountType = await accountTypeRepository.GetAsync(id);

        if (accountType is null)
        {
            return Results.BadRequest("Entity not found");
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            var nameExists = accountTypeRepository.ListAsNoTracking()
                .Any(t => t.Name.ToLower().Equals(request.Name.ToLower()));

            if (nameExists)
            {
                return Results.BadRequest("Name already in use");
            }
        }

        accountType.UpdatedAt = DateTime.UtcNow;
        accountType.Name = request.Name ?? accountType.Name;
        accountType.Description = request.Description ?? accountType.Description;

        await accountTypeRepository.UpdateAsync(accountType);
        await accountTypeRepository.SaveChangesAsync();

        return Results.Ok(new UpdateAccountTypeResponse(
            accountType.Id, accountType.Name, accountType.Description));
    }

    public async Task<IResult> DeleteAsync(DeleteAccountTypeRequest request)
    {
        var accountType = await accountTypeRepository.GetAsync(request.Id);

        if (accountType is null)
        {
            return Results.BadRequest("Entity not found");
        }

        if (accountRepository.ListAsNoTracking().Any(a => a.AccountTypeId == request.Id))
        {
            return Results.BadRequest("Can't delete an account type that has active accounts");
        }

        await accountTypeRepository.DeleteAsync(accountType);
        await accountTypeRepository.SaveChangesAsync();

        return Results.NoContent();
    }

    public async Task<IResult> ListAsync(ListAccountTypesRequest request)
    {
        var results = accountTypeRepository.ListAsNoTracking()
            .WhereIf(request.Id is not null, accountType => accountType.Id == request.Id)
            .WhereIf(request.Name is not null, accountType => accountType.Name == request.Name)
            .WhereIf(request.Description is not null, accountType => accountType.Description == request.Description)
                .Skip(request.Page == 1 ? 0 : request.Page * request.PageSize)
                .Take(request.PageSize)
            .Select(r => new ListAccountTypesResponseItem(r.Id, r.Name, r.Description))
            .ToList();

        return Results.Ok(new PaginatedResponse<ListAccountTypesResponseItem>(results, request.Page, request.PageSize));
    }

    public async Task<IResult> GetAsync(GetAccountTypeRequest request)
    {
        var result = await accountTypeRepository.GetAsync(request.Id);

        return result is not null
            ? Results.Ok(new GetAccountTypeResponse(result.Id, result.Name, result.Description))
            : Results.NotFound();
    }
}
