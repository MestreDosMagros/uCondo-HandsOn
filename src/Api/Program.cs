using Api.Endpoints;
using Api.Extensions;
using Application.Services;
using Application.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Extensions;
using Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

builder.Services.AddContext(builder.Configuration.GetConnectionString("uCondo")!);

builder.Services.AddTransient<IAccountService, AccountService>();
builder.Services.AddTransient<IAccountTypeService, AccountTypeService>();

builder.Services.AddTransient<IAccountRepository, AccountRepository>();
builder.Services.AddTransient<IAccountTypeRepository, AccountTypeRepository>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateAccountRequestValidator>();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Version = "v1",
            Title = "Accounts",
            Description = "uCondo - Hands On"
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.MapOpenApi();
app.UseHttpsRedirection();

app.ApplyMigrations();
app.MapAccountEndpoints();
app.MapAccountTypeEndpoints();

app.UseProblemDetailsExceptionHandler();

app.Run();