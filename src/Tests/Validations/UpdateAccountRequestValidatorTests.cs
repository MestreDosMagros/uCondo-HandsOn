using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentValidation.TestHelper;
using Application.Contracts;
using Application.Validators;
using System.Globalization;

namespace Tests.Validations;

[TestClass]
public sealed class UpdateAccountRequestValidatorTests
{
    private UpdateAccountRequestValidator validator;

    [TestInitialize]
    public void Setup()
    {
        validator = new UpdateAccountRequestValidator();
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
    }

    [TestMethod]
    public void Should_HaveError_When_NameIsTooShort()
    {
        var request = new UpdateAccountRequest("1.1.1", "AB", "Valid Description", true, null, null);
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Name)
            .WithErrorMessage("The length of 'Name' must be at least 3 characters. You entered 2 characters.");
    }

    [TestMethod]
    public void Should_HaveError_When_NameIsTooLong()
    {
        var request = new UpdateAccountRequest("1.1.1", new string('A', 101), "Valid Description", true, null, null);
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Name)
            .WithErrorMessage("The length of 'Name' must be 100 characters or fewer. You entered 101 characters.");
    }

    [TestMethod]
    public void Should_HaveError_When_DescriptionIsTooShort()
    {
        var request = new UpdateAccountRequest("1.1.1", "Valid Name", "AB", true, null, null);
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Description)
            .WithErrorMessage("The length of 'Description' must be at least 3 characters. You entered 2 characters.");
    }

    [TestMethod]
    public void Should_HaveError_When_DescriptionIsTooLong()
    {
        var request = new UpdateAccountRequest("1.1.1", "Valid Name", new string('A', 251), true, null, null);
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Description)
            .WithErrorMessage("The length of 'Description' must be 250 characters or fewer. You entered 251 characters.");
    }

    [TestMethod]
    public void Should_HaveError_When_CodeIsInvalid()
    {
        var request = new UpdateAccountRequest("invalid-code", "Valid Name", "Valid Description", true, null, null);
        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(a => a.Code)
            .WithErrorMessage("Code is invalid.");
    }

    [TestMethod]
    public void Should_NotHaveErrors_When_RequestIsValid()
    {
        var request = new UpdateAccountRequest("1.1.1", "Valid Name", "Valid Description", true, null, null);
        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
