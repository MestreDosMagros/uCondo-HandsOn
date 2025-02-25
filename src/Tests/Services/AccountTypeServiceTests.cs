using Application.Contracts;
using Application.Services;
using Domain.Aggregates;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace Tests.Services;

[TestClass]
public sealed class AccountTypeServiceTests
{
    private IAccountTypeService accountTypeService;
    private Mock<IAccountRepository> accountRepositoryMoq;
    private Mock<IAccountTypeRepository> accountTypeRepositoryMoq;

    [TestInitialize]
    public void Setup()
    {
        accountRepositoryMoq = new Mock<IAccountRepository>();
        accountTypeRepositoryMoq = new Mock<IAccountTypeRepository>();
        accountTypeService = new AccountTypeService(accountTypeRepositoryMoq.Object, accountRepositoryMoq.Object);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldReturnBadRequest_WhenNameAlreadyExists()
    {
        // Arrange
        var request = new CreateAccountTypeRequest("ExistingType", "Description");
        var existingAccountType = new AccountType("ExistingType", "Some Desc");

        accountTypeRepositoryMoq.Setup(r => r.ListAsNoTracking())
            .Returns(new[] { existingAccountType }.AsQueryable());

        // Act
        var result = await accountTypeService.CreateAsync(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequest<string>));
        Assert.AreEqual("Name already in use", ((BadRequest<string>)result).Value);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldCreateAccountType_WhenValidRequest()
    {
        // Arrange
        var request = new CreateAccountTypeRequest("NewType", "New Description");
        accountTypeRepositoryMoq.Setup(r => r.ListAsNoTracking())
            .Returns(Enumerable.Empty<AccountType>().AsQueryable());

        // Act
        var result = await accountTypeService.CreateAsync(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(Created<CreateAccountTypeResponse>));

        var response = ((Created<CreateAccountTypeResponse>)result).Value;
        Assert.AreEqual("NewType", response.Name);
        Assert.AreEqual("New Description", response.Description);

        accountTypeRepositoryMoq.Verify(r => r.CreateAsync(It.IsAny<AccountType>()), Times.Once);
        accountTypeRepositoryMoq.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnBadRequest_WhenEntityNotFound()
    {
        // Arrange
        var request = new UpdateAccountTypeRequest("Updated Name", "Updated Description");
        accountTypeRepositoryMoq.Setup(r => r.GetAsync(It.IsAny<int>()))
            .ReturnsAsync((AccountType)null);

        // Act
        var result = await accountTypeService.UpdateAsync(1, request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequest<string>));
        Assert.AreEqual("Entity not found", ((BadRequest<string>)result).Value);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnBadRequest_WhenNameAlreadyExists()
    {
        // Arrange
        var existingAccountType = new AccountType("Existing Name", "Some Desc") { Id = 1 };
        var anotherAccountType = new AccountType("Updated Name", "Another Desc") { Id = 2 };

        var request = new UpdateAccountTypeRequest("Updated Name", "Updated Description");

        accountTypeRepositoryMoq.Setup(r => r.GetAsync(1))
            .ReturnsAsync(existingAccountType);

        accountTypeRepositoryMoq.Setup(r => r.ListAsNoTracking())
            .Returns(new[] { anotherAccountType }.AsQueryable());

        // Act
        var result = await accountTypeService.UpdateAsync(1, request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequest<string>));
        Assert.AreEqual("Name already in use", ((BadRequest<string>)result).Value);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateAccountType_WhenValidRequest()
    {
        // Arrange
        var existingAccountType = new AccountType("Old Name", "Old Desc") { Id = 1 };
        var request = new UpdateAccountTypeRequest("Updated Name", "Updated Description");

        accountTypeRepositoryMoq.Setup(r => r.GetAsync(1))
            .ReturnsAsync(existingAccountType);

        accountTypeRepositoryMoq.Setup(r => r.ListAsNoTracking())
            .Returns(Enumerable.Empty<AccountType>().AsQueryable());

        // Act
        var result = await accountTypeService.UpdateAsync(1, request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(Ok<UpdateAccountTypeResponse>));

        var response = ((Ok<UpdateAccountTypeResponse>)result).Value;
        Assert.AreEqual("Updated Name", response.Name);
        Assert.AreEqual("Updated Description", response.Description);
        Assert.AreEqual(1, response.Id);

        accountTypeRepositoryMoq.Verify(r => r.UpdateAsync(It.IsAny<AccountType>()), Times.Once);
        accountTypeRepositoryMoq.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldReturnBadRequest_WhenEntityNotFound()
    {
        // Arrange
        var request = new DeleteAccountTypeRequest(1);
        accountTypeRepositoryMoq.Setup(r => r.GetAsync(It.IsAny<int>()))
            .ReturnsAsync((AccountType)null);

        // Act
        var result = await accountTypeService.DeleteAsync(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequest<string>));
        Assert.AreEqual("Entity not found", ((BadRequest<string>)result).Value);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldReturnBadRequest_WhenAccountTypeHasActiveAccounts()
    {
        // Arrange
        var request = new DeleteAccountTypeRequest(1);
        var existingAccountType = new AccountType("Type1", "Description") { Id = 1 };

        accountTypeRepositoryMoq.Setup(r => r.GetAsync(1))
            .ReturnsAsync(existingAccountType);

        accountRepositoryMoq.Setup(r => r.ListAsNoTracking())
            .Returns(new[] { new Account("Account1", "1.1", "Desc", true, null, 1) { Id = 1 } }.AsQueryable());

        // Act
        var result = await accountTypeService.DeleteAsync(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequest<string>));
        Assert.AreEqual("Can't delete an account type that has active accounts", ((BadRequest<string>)result).Value);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldDeleteAccountType_WhenNoActiveAccounts()
    {
        // Arrange
        var request = new DeleteAccountTypeRequest(1);
        var existingAccountType = new AccountType("Type1", "Description") { Id = 1 };

        accountTypeRepositoryMoq.Setup(r => r.GetAsync(1))
            .ReturnsAsync(existingAccountType);

        accountRepositoryMoq.Setup(r => r.ListAsNoTracking())
            .Returns(Enumerable.Empty<Account>().AsQueryable());

        // Act
        var result = await accountTypeService.DeleteAsync(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NoContent));

        accountTypeRepositoryMoq.Verify(r => r.DeleteAsync(existingAccountType), Times.Once);
        accountTypeRepositoryMoq.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
