using Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace Application.Services;

public interface IAccountService
{
    Task<IResult> GetAsync(GetAccountRequest request);
    Task<IResult> ListAsync(ListAccountsRequest request);
    Task<IResult> GetNextCodeAsync(GetNextCodeRequest request);
    Task<IResult> DeleteAsync(DeleteAccountRequest request);
    Task<IResult> CreateAsync(CreateAccountRequest request);
    Task<IResult> UpdateAsync(int id, UpdateAccountRequest request);
}