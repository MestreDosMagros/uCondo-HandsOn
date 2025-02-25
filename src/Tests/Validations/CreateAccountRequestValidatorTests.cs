using Application.Contracts;
using Application.Validators;
using FluentValidation.TestHelper;

namespace Tests.Validations;

[TestClass]
public sealed class CreateAccountRequestValidatorTests
{
    private CreateAccountRequestValidator validator;

    [TestInitialize]
    public void Setup()
    {
        validator = new CreateAccountRequestValidator();
    }

    [TestMethod]
    public void Should_HaveError_When_NameIsEmpty()
    {
        var request = new CreateAccountRequest("", "1.1.1", "Valid Description", true, null, null);
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Name)
            .WithErrorMessage("'Name' must not be empty.");
    }

    [TestMethod]
    public void Should_HaveError_When_NameIsTooShort()
    {
        var request = new CreateAccountRequest("AB", "1.1.1", "Valid Description", true, null, null);
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Name)
            .WithErrorMessage("The length of 'Name' must be at least 3 characters. You entered 2 characters.");
    }

    [TestMethod]
    public void Should_HaveError_When_NameIsTooLong()
    {
        var request = new CreateAccountRequest(new string('A', 101), "1.1.1", "Valid Description", true, null, null);
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Name)
            .WithErrorMessage("The length of 'Name' must be 100 characters or fewer. You entered 101 characters.");
    }

    [TestMethod]
    public void Should_HaveError_When_DescriptionIsTooShort()
    {
        var request = new CreateAccountRequest("Valid Name", "1.1.1", "AB", true, null, null);
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Description)
            .WithErrorMessage("The length of 'Description' must be at least 3 characters. You entered 2 characters.");
    }

    [TestMethod]
    public void Should_HaveError_When_DescriptionIsTooLong()
    {
        var request = new CreateAccountRequest("Valid Name", "1.1.1", new string('A', 251), true, null, null);
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Description)
            .WithErrorMessage("The length of 'Description' must be 250 characters or fewer. You entered 251 characters.");
    }

    [TestMethod]
    public void Should_HaveError_When_CodeIsEmpty()
    {
        var request = new CreateAccountRequest("Valid Name", "", "Valid Description", true, null, null);
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Code)
            .WithErrorMessage("'Code' must not be empty.");
    }

    [TestMethod]
    public void Should_HaveError_When_CodeIsInvalid()
    {
        var request = new CreateAccountRequest("Valid Name", "invalid-code", "Valid Description", true, null, null);
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Code)
            .WithErrorMessage("Code is invalid.");
    }

    [TestMethod]
    public void Should_NotHaveErrors_When_RequestIsValid()
    {
        var request = new CreateAccountRequest("Valid Name", "1.1.1", "Valid Description", true, null, 1);
        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
