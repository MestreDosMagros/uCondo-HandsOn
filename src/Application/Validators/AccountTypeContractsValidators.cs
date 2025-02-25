using Application.Contracts;
using FluentValidation;

namespace Application.Validators;

public sealed class CreateAccountTypeRequestValidator : AbstractValidator<CreateAccountTypeRequest>
{
    public CreateAccountTypeRequestValidator()
    {
        RuleFor(a => a.Name).NotEmpty().MinimumLength(3).MaximumLength(50);
        RuleFor(a => a.Description).MinimumLength(3).MaximumLength(250).When(a => !string.IsNullOrEmpty(a.Description));
    }
}

public sealed class UpdateAccountTypeRequestValidator : AbstractValidator<UpdateAccountTypeRequest>
{
    public UpdateAccountTypeRequestValidator()
    {
        RuleFor(a => a.Name).MinimumLength(3).MaximumLength(50).When(a => !string.IsNullOrEmpty(a.Name));
        RuleFor(a => a.Description).MinimumLength(3).MaximumLength(250).When(a => !string.IsNullOrEmpty(a.Description));
    }
}
