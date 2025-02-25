using Application.Contracts;
using Domain.ValueObjects;
using FluentValidation;

namespace Application.Validators;

public sealed class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountRequestValidator()
    {
        RuleFor(a => a.Name).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(a => a.AccountTypeId).NotNull().When(a => a.ParentId is null);
        RuleFor(a => a.Code).NotEmpty().Must(c => CodeVo.IsValid(c)).WithMessage("Code is invalid.");
        RuleFor(a => a.Description).MinimumLength(3).MaximumLength(250).When(a => !string.IsNullOrEmpty(a.Description));
    }
}

public sealed class UpdateAccountRequestValidator : AbstractValidator<UpdateAccountRequest>
{
    public UpdateAccountRequestValidator()
    {
        RuleFor(a => a.Name).MinimumLength(3).MaximumLength(100).When(a => !string.IsNullOrEmpty(a.Name));
        RuleFor(a => a.Description).MinimumLength(3).MaximumLength(250).When(a => !string.IsNullOrEmpty(a.Description));
        RuleFor(a => a.Code).Must(c => CodeVo.IsValid(c)).WithMessage("Code is invalid.").When(a => !string.IsNullOrEmpty(a.Code));
    }
}