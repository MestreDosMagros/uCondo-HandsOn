using Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace Application.Services;

public interface IAccountTypeService
{
    Task<IResult> ListAsync(ListAccountTypesRequest request);
    Task<IResult> DeleteAsync(DeleteAccountTypeRequest request);
    Task<IResult> CreateAsync(CreateAccountTypeRequest request);
    Task<IResult> UpdateAsync(int id, UpdateAccountTypeRequest request);
    Task<IResult> GetAsync(GetAccountTypeRequest getAccountTypeRequest);
}