using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentValidation.TestHelper;
using Application.Contracts;
using Application.Validators;
using System.Globalization;

namespace Tests.Validations;

[TestClass]
public sealed class CreateAccountTypeRequestValidatorTests
{
    private CreateAccountTypeRequestValidator validator;

    [TestInitialize]
    public void Setup()
    {
        validator = new CreateAccountTypeRequestValidator();
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
    }

    [TestMethod]
    public void Should_HaveError_When_NameIsEmpty()
    {
        var request = new CreateAccountTypeRequest("", "Valid Description");
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Name)
            .WithErrorMessage("'Name' must not be empty.");
    }

    [TestMethod]
    public void Should_HaveError_When_NameIsTooShort()
    {
        var request = new CreateAccountTypeRequest("AB", "Valid Description");
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Name)
            .WithErrorMessage("The length of 'Name' must be at least 3 characters. You entered 2 characters.");
    }

    [TestMethod]
    public void Should_HaveError_When_NameIsTooLong()
    {
        var request = new CreateAccountTypeRequest(new string('A', 51), "Valid Description");
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Name)
            .WithErrorMessage("The length of 'Name' must be 50 characters or fewer. You entered 51 characters.");
    }

    [TestMethod]
    public void Should_HaveError_When_DescriptionIsTooShort()
    {
        var request = new CreateAccountTypeRequest("Valid Name", "AB");
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Description)
            .WithErrorMessage("The length of 'Description' must be at least 3 characters. You entered 2 characters.");
    }

    [TestMethod]
    public void Should_HaveError_When_DescriptionIsTooLong()
    {
        var request = new CreateAccountTypeRequest("Valid Name", new string('A', 251));
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Description)
            .WithErrorMessage("The length of 'Description' must be 250 characters or fewer. You entered 251 characters.");
    }

    [TestMethod]
    public void Should_NotHaveErrors_When_RequestIsValid()
    {
        var request = new CreateAccountTypeRequest("Valid Name", "Valid Description");
        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
