using Application.Contracts;
using Application.Validators;
using FluentValidation.TestHelper;

namespace Tests.Validations;

[TestClass]
public sealed class UpdateAccountTypeRequestValidatorTests
{
    private UpdateAccountTypeRequestValidator validator;

    [TestInitialize]
    public void Setup()
    {
        validator = new UpdateAccountTypeRequestValidator();
    }

    [TestMethod]
    public void Should_HaveError_When_NameIsTooShort()
    {
        var request = new UpdateAccountTypeRequest("AB", "Valid Description");
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Name)
            .WithErrorMessage("The length of 'Name' must be at least 3 characters. You entered 2 characters.");
    }

    [TestMethod]
    public void Should_HaveError_When_NameIsTooLong()
    {
        var request = new UpdateAccountTypeRequest(new string('A', 51), "Valid Description");
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Name)
            .WithErrorMessage("The length of 'Name' must be 50 characters or fewer. You entered 51 characters.");
    }

    [TestMethod]
    public void Should_HaveError_When_DescriptionIsTooShort()
    {
        var request = new UpdateAccountTypeRequest("Valid Name", "AB");
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Description)
            .WithErrorMessage("The length of 'Description' must be at least 3 characters. You entered 2 characters.");
    }

    [TestMethod]
    public void Should_HaveError_When_DescriptionIsTooLong()
    {
        var request = new UpdateAccountTypeRequest("Valid Name", new string('A', 251));
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Description)
            .WithErrorMessage("The length of 'Description' must be 250 characters or fewer. You entered 251 characters.");
    }

    [TestMethod]
    public void Should_NotHaveErrors_When_RequestIsValid()
    {
        var request = new UpdateAccountTypeRequest("Valid Name", "Valid Description");
        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Should_NotHaveErrors_When_NameAndDescriptionAreNull()
    {
        var request = new UpdateAccountTypeRequest(null, null);
        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors(); // As propriedades podem ser nulas
    }
}
