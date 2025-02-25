using FluentValidation;

namespace Api.Filters;

public sealed class FluentValidatorFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (context.HttpContext.Request.Method == "GET")
        {
            return next;
        }

        var entity = context.Arguments.OfType<T>().FirstOrDefault();
        if (entity is null)
        {
            return Results.BadRequest();
        }

        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null)
        {
            return await next(context);
        }

        var validationResult = await validator.ValidateAsync(entity);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        return await next(context);
    }
}