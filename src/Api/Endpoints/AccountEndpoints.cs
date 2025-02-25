using Api.Filters;
using Application.Contracts;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
        var accountsGroup = app.MapGroup("api/v1/accounts")
            .WithOpenApi()
            .WithTags("Accounts");

        accountsGroup.MapPost("/", async ([FromBody] CreateAccountRequest request, [FromServices] IAccountService service) =>
        {
            return await service.CreateAsync(request);
        })
        .WithName("CreateAccount")
        .Produces(StatusCodes.Status400BadRequest)
        .Produces<CreateAccountResponse>(StatusCodes.Status201Created)
        .AddEndpointFilter<FluentValidatorFilter<CreateAccountRequest>>()
        .WithDescription("Create a new account");


        accountsGroup.MapPut("/{id:int}", async ([FromRoute] int id, [FromBody] UpdateAccountRequest request, [FromServices] IAccountService service) =>
        {
            return await service.UpdateAsync(id, request);
        })
        .WithName("UpdateAccount")
        .Produces(StatusCodes.Status400BadRequest)
        .Produces<UpdateAccountResponse>(StatusCodes.Status200OK)
        .AddEndpointFilter<FluentValidatorFilter<UpdateAccountRequest>>()
        .WithDescription("Update an account");


        accountsGroup.MapDelete("/{id:int}", async ([FromRoute] int id, [FromServices] IAccountService service) =>
        {
            return await service.DeleteAsync(new DeleteAccountRequest(id));
        })
        .WithName("DeleteAccount")
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status204NoContent)
        .WithDescription("Delete an account");

        accountsGroup.MapGet("/{id:int}", async ([FromRoute] int id, [FromServices] IAccountService service) =>
        {
            return await service.GetAsync(new GetAccountRequest(id));
        })
        .WithName("GetAccount")
        .Produces<PaginatedResponse<ListAccountsResponseItem>>(StatusCodes.Status200OK)
        .WithDescription("Get an account for id");

        accountsGroup.MapGet("/", async (
            [FromQuery] int? id, [FromQuery] string? code, [FromQuery] string? name,
            [FromQuery] string? description, [FromQuery] bool? canHaveEntries, [FromQuery] int? parentId,
            [FromQuery] int? accountTypeId, [FromQuery] int page, [FromQuery] int pageSize, [FromServices] IAccountService service) =>
        {
            return await service.ListAsync(new ListAccountsRequest(
                id, code, name, description, canHaveEntries, parentId, accountTypeId){ Page = page, PageSize = pageSize });
        })
        .WithName("ListAccounts")
        .Produces<PaginatedResponse<ListAccountsResponseItem>>(StatusCodes.Status200OK)
        .WithDescription("List all accounts");

        accountsGroup.MapGet("/next-code", async ([FromQuery] int? accountId, [FromServices] IAccountService service) =>
        {
            return await service.GetNextCodeAsync(new GetNextCodeRequest(accountId));
        })
        .WithName("GetNextCode")
        .Produces<string>(StatusCodes.Status200OK)
        .WithDescription("Get a next code for specific account");
    }
}
