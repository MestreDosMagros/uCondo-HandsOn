using Api.Filters;
using Application.Contracts;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class AccountTypeEndpoints
{
    public static void MapAccountTypeEndpoints(this WebApplication app)
    {
        var accountTypesGroup = app.MapGroup("api/v1/account-types")
           .WithOpenApi()
           .WithTags("AccountsTypes");

        accountTypesGroup.MapPost("/", async ([FromBody] CreateAccountTypeRequest request, [FromServices] IAccountTypeService service) =>
        {
            return await service.CreateAsync(request);
        })
            .WithName("CreateAccountType")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces<CreateAccountResponse>(StatusCodes.Status201Created)
            .AddEndpointFilter<FluentValidatorFilter<CreateAccountTypeRequest>>()
            .WithDescription("Create a new account type");


        accountTypesGroup.MapPut("/{id:int}", async ([FromRoute] int id, [FromBody] UpdateAccountTypeRequest request, [FromServices] IAccountTypeService service) =>
        {
            return await service.UpdateAsync(id, request);
        })
            .WithName("UpdateAccountType")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces<UpdateAccountResponse>(StatusCodes.Status200OK)
            .AddEndpointFilter<FluentValidatorFilter<UpdateAccountTypeRequest>>()
            .WithDescription("Update an account type");


        accountTypesGroup.MapDelete("/{id:int}", async ([FromRoute] int id, [FromServices] IAccountTypeService service) =>
        {
            return await service.DeleteAsync(new DeleteAccountTypeRequest(id));
        })
            .WithName("DeleteAccountType")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent)
            .WithDescription("Delete an account type");

        accountTypesGroup.MapGet("/", async (
            [FromQuery] int? id, [FromQuery] string? name, [FromQuery] string? description,
            [FromQuery] int page, [FromQuery] int pageSize, [FromServices] IAccountTypeService service) =>
        {
            return await service.ListAsync(new ListAccountTypesRequest(id, name, description) { Page = page, PageSize = pageSize });
        })
            .WithName("ListAccountTypes")
            .Produces<PaginatedResponse<ListAccountTypesResponseItem>>(StatusCodes.Status200OK)
            .WithDescription("List all account types");

        accountTypesGroup.MapGet("/{id:int}", async ([FromRoute] int id, [FromServices] IAccountTypeService service) =>
        {
            return await service.GetAsync(new GetAccountTypeRequest(id));
        })
            .WithName("GetAccountType")
            .Produces<PaginatedResponse<ListAccountTypesResponseItem>>(StatusCodes.Status200OK)
            .WithDescription("Get an account type for id");
    }
}
