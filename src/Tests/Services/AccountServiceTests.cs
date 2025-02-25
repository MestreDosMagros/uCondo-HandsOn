using Application.Contracts;
using Application.Services;
using Domain.Aggregates;
using Domain.Exceptions;
using Domain.ValueObjects;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace Tests.Services;

[TestClass]
public sealed class AccountServiceTests
{
    private IAccountService accountService;
    private Mock<IAccountRepository> accountRepositoryMoq;
    private Mock<IAccountTypeRepository> accountTypeRepositoryMoq;

    [TestInitialize]
    public void Setup()
    {
        accountRepositoryMoq = new Mock<IAccountRepository>();
        accountTypeRepositoryMoq = new Mock<IAccountTypeRepository>();
        accountService = new AccountService(accountRepositoryMoq.Object, accountTypeRepositoryMoq.Object);
    }

    [TestMethod]
    [DataRow(1, "1", "1.1")]
    [DataRow(2, "2", "2.1")]
    [DataRow(3, "3", "3.1")]
    [DataRow(4, "4.1", "4.1.1")]
    [DataRow(5, "5.2", "5.2.1")]
    [DataRow(6, "6.1.1", "6.1.1.1")]
    [DataRow(7, "7.1.1.1", "7.1.1.1.1")]
    [DataRow(8, "8.1.1.1.1", "8.1.1.1.1.1")]
    [DataRow(9, "9.1.1.1.1.1", "9.1.1.1.1.1.1")]
    [DataRow(10, "10.1.1.1.1.1.1", "10.1.1.1.1.1.1.1")]
    public async Task GetNextCode_Should_Return_Next_Child_Code(int accountId, string parentCode, string expectedCode)
    {
        var account = new Account
        {
            Id = accountId,
            Code = new CodeVo(parentCode),
        };

        accountRepositoryMoq.Setup(repo => repo.ListAsNoTracking())
            .Returns(new List<Account> { account }.AsQueryable());

        var result = await accountService.GetNextCodeAsync(new GetNextCodeRequest(accountId));

        var suggestedCode = ((Ok<GetNextCodeResponse>)result)!.Value!.Code;
        Assert.AreEqual(expectedCode, suggestedCode);
    }

    [TestMethod]
    public async Task GetNextCode_Should_Return_Next_Child_Code()
    {
        var parent = new Account
        {
            Id = 1,
            AccountTypeId = 1,
            Code = new CodeVo("4.1"),
        };

        var child1 = new Account
        {
            Id = 1,
            AccountTypeId = 1,
            Code = new CodeVo("4.1.1"),
        };

        var child2 = new Account
        {
            Id = 1,
            AccountTypeId = 1,
            Code = new CodeVo("4.1.2"),
        };

        parent.AddChild(child1);
        parent.AddChild(child2);

        accountRepositoryMoq.Setup(repo => repo.ListAsNoTracking())
            .Returns(new List<Account> { parent }.AsQueryable());

        var result = await accountService.GetNextCodeAsync(new GetNextCodeRequest(parent.Id));

        var suggestedCode = ((Ok<GetNextCodeResponse>)result)!.Value!.Code;
        Assert.AreEqual("4.1.3", suggestedCode);
    }

    [TestMethod]
    public async Task GetNextCode_Should_Return_One_Parent_Level()
    {
        var repo = new List<Account>();

        var parent = new Account
        {
            Id = 1,
            AccountTypeId = 1,
            Code = new CodeVo("4.1"),
        };

        var child1 = new Account
        {
            Id = 1,
            Parent = parent,
            AccountTypeId = 1,
            Code = new CodeVo("4.1"),
        };

        repo.Add(parent);
        repo.Add(child1);
        parent.AddChild(child1);

        for (var i = 1; i < 1000; i++)
        {
            var childX = new Account
            {
                Id = i,
                Parent = child1,
                AccountTypeId = 1,
                Code = new CodeVo($"4.1.{i}"),
            };

            child1.AddChild(childX);
            repo.Add(childX);
        }

        accountRepositoryMoq.Setup(repo => repo.ListAsNoTracking())
            .Returns(repo.AsQueryable());

        var result = await accountService.GetNextCodeAsync(new GetNextCodeRequest(parent.Id));

        var suggestedCode = ((Ok<GetNextCodeResponse>)result)!.Value!.Code;
        Assert.AreEqual("4.2", suggestedCode);
    }

    [TestMethod]
    public async Task GetNextCode_Should_Return_One_Parent_Level_2()
    {
        var repo = new List<Account>();
        var main = new Account
        {
            Id = 1,
            AccountTypeId = 1,
            Code = new CodeVo("9"),
        };

        var parent1 = new Account
        {
            Id = 2,
            Parent = main,
            AccountTypeId = 1,
            Code = new CodeVo($"9.9")
        };

        var child2 = new Account
        {
            Id = 3,
            Parent = main,
            AccountTypeId = 1,
            Code = new CodeVo($"9.10")
        };

        main.AddChild(parent1);
        main.AddChild(child2);

        repo.Add(main);
        repo.Add(parent1);
        repo.Add(child2);

        for (int i = 1; i < 1000; i++)
        {
            var parent2 = new Account
            {
                Id = i,
                Parent = parent1,
                AccountTypeId = 1,
                Code = new CodeVo($"9.9.{i}")
            };

            parent1.AddChild(parent2);
            repo.Add(parent2);
        }

        accountRepositoryMoq.Setup(repo => repo.ListAsNoTracking())
            .Returns(repo.AsQueryable());

        var result = await accountService.GetNextCodeAsync(new GetNextCodeRequest(parent1.Id));

        var suggestedCode = ((Ok<GetNextCodeResponse>)result)!.Value!.Code;
        Assert.AreEqual("9.11", suggestedCode);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldCreateAccount_WhenValidRequest()
    {
        // Arrange
        var request = new CreateAccountRequest("Test Account", "1.1.1", "Description", true, null, 1);

        var accountType = new AccountType("Test Type", "Test") { Id = 1 };
        accountTypeRepositoryMoq.Setup(r => r.GetAsync(1))
            .ReturnsAsync(accountType);
        accountRepositoryMoq.Setup(r => r.ListAsNoTracking())
            .Returns(Enumerable.Empty<Account>().AsQueryable());

        // Act
        var result = await accountService.CreateAsync(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(Created<CreateAccountResponse>));
        accountRepositoryMoq.Verify(r => r.CreateAsync(It.IsAny<Account>()), Times.Once);
        accountRepositoryMoq.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldReturnBadRequest_WhenParentAccountNotFound()
    {
        // Arrange
        var request = new CreateAccountRequest("Test Account", "1.2", "Description", true, 1, null);
        accountRepositoryMoq.Setup(r => r.GetAsync(request.ParentId!.Value))
            .ReturnsAsync((Account)null!);

        // Act
        var result = await accountService.CreateAsync(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequest<string>));
        Assert.AreEqual("Parent account not found", ((BadRequest<string>)result).Value);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldReturnBadRequest_WhenChildAccountHasDifferentType()
    {
        // Arrange
        var parentAccount = new Account("Parent", "1.1", "Parent Desc", true, null, 2);
        var request = new CreateAccountRequest("Test Account", "1.1.2", "Description", true, parentAccount.Id, 1);

        accountTypeRepositoryMoq.Setup(r => r.GetAsync(2))
            .ReturnsAsync((AccountType)null);

        accountRepositoryMoq.Setup(r => r.GetAsync(request.ParentId!.Value))
            .ReturnsAsync(parentAccount);

        // Act
        var result = await accountService.CreateAsync(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequest<string>));
        Assert.AreEqual("Child account can't have type different from parent account", ((BadRequest<string>)result).Value);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldReturnBadRequest_WhenAccountTypeNotFound()
    {
        // Arrange
        var request = new CreateAccountRequest("Test Account", "2.2.1", "Description", true, null, 1);

        accountTypeRepositoryMoq.Setup(r => r.GetAsync(1))
            .ReturnsAsync(new AccountType() { Id = 1 });

        accountTypeRepositoryMoq.Setup(r => r.GetAsync(request.AccountTypeId!.Value))
            .ReturnsAsync((AccountType)null!);

        // Act
        var result = await accountService.CreateAsync(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequest<string>));
        Assert.AreEqual("Accoutn type not found", ((BadRequest<string>)result).Value);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldReturnBadRequest_WhenCodeAlreadyExists()
    {
        // Arrange
        var request = new CreateAccountRequest("Test Account", "1.1.1", "Description", true, null, 1);
        var existingAccount = new Account("Existing", "1.1.1", "Existing Desc", true, null, 1);

        accountTypeRepositoryMoq.Setup(r => r.GetAsync(1))
            .ReturnsAsync(new AccountType() { Id = 1 });

        accountRepositoryMoq.Setup(r => r.ListAsNoTracking())
            .Returns(new[] { existingAccount }.AsQueryable());

        // Act
        var result = await accountService.CreateAsync(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequest<string>));
        Assert.AreEqual("Code already in use", ((BadRequest<string>)result).Value);
    }

    [TestMethod]
    [DataRow("abc")]
    [DataRow("1.1.a")]
    public async Task CreateAsync_ShouldReturnBadRequest_WhenCodeIsInvalid(string code)
    {

        var request = new CreateAccountRequest("Test Account", code, "Description", true, null, 1);

        accountTypeRepositoryMoq.Setup(r => r.GetAsync(1))
            .ReturnsAsync(new AccountType() { Id = 1 });
        // Act
        Assert.ThrowsExceptionAsync<InvalidCodeException>(async () => await accountService.CreateAsync(request));
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldReturnBadRequest_WhenAccountNotFound()
    {
        // Arrange
        var request = new DeleteAccountRequest(1);
        accountRepositoryMoq.Setup(r => r.List())
            .Returns(Enumerable.Empty<Account>().AsQueryable());

        // Act
        var result = await accountService.DeleteAsync(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequest<string>));
        Assert.AreEqual("Entity not found", ((BadRequest<string>)result).Value);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldReturnBadRequest_WhenAccountHasChildAccounts()
    {
        // Arrange
        var accountId =1;
        var childAccount = new Account("Child", "1.1.1", "Child Account", true, accountId,1);
        var parentAccount = new Account("Parent", "1.1", "Parent Account", true, null,1);
        parentAccount.AddChild(childAccount);
        var request = new DeleteAccountRequest(parentAccount.Id);
        accountRepositoryMoq.Setup(r => r.List())
            .Returns(new[] { parentAccount }.AsQueryable());

        // Act
        var result = await accountService.DeleteAsync(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequest<string>));
        Assert.AreEqual("Can't remove an account with child accounts", ((BadRequest<string>)result).Value);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldDeleteAccount_WhenValidRequest()
    {
        // Arrange
        var account = new Account("Test Account", "1.2", "Description", false, null,1);

        var request = new DeleteAccountRequest(account.Id);
        accountRepositoryMoq.Setup(r => r.List())
            .Returns(new[] { account }.AsQueryable());

        // Act
        var result = await accountService.DeleteAsync(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NoContent));
        accountRepositoryMoq.Verify(r => r.DeleteAsync(account), Times.Once);
        accountRepositoryMoq.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnBadRequest_WhenAccountNotFound()
    {
        // Arrange
        var request = new UpdateAccountRequest("1.2", "Updated", "Updated Desc", false, 2, null);
        accountRepositoryMoq.Setup(r => r.List())
            .Returns(Enumerable.Empty<Account>().AsQueryable());

        // Act
        var result = await accountService.UpdateAsync(1, request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequest<string>));
        Assert.AreEqual("Entity not foud", ((BadRequest<string>)result).Value);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnBadRequest_WhenChangingCodeOnAccountWithParent()
    {
        // Arrange
        var parentAccount = new Account("Parent", "1", "Parent Desc", true, null, 1);
        var account = new Account("Child", "1.1", "Child Desc", true, 1, 2) { Parent = parentAccount };

        var request = new UpdateAccountRequest("1.2", "Updated", "Updated Desc", false, 2, null);
        accountRepositoryMoq.Setup(r => r.List())
            .Returns(new[] { account }.AsQueryable());

        // Act
        var result = await accountService.UpdateAsync(account.Id, request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequest<string>));
        Assert.AreEqual("Can't change code on a account with a parent account", ((BadRequest<string>)result).Value);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnBadRequest_WhenChangingCodeOnAccountWithChildren()
    {
        // Arrange
        var account = new Account("Parent", "1", "Parent Desc", true, null, 1);
        var childAccount = new Account("Child", "1.1", "Child Desc", true, account.Id, 1);
        account.AddChild(childAccount);

        var request = new UpdateAccountRequest("2", "Updated", "Updated Desc", false, 2, null);

        accountTypeRepositoryMoq.Setup(r => r.GetAsync(1))
            .ReturnsAsync(new AccountType() { Id = 1 });

        accountRepositoryMoq.Setup(r => r.List())
            .Returns(new[] { account }.AsQueryable());

        // Act
        var result = await accountService.UpdateAsync(account.Id, request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequest<string>));
        Assert.AreEqual("Can't change code of a account with child accounts", ((BadRequest<string>)result).Value);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnBadRequest_WhenCodeAlreadyExists()
    {
        // Arrange
        var existingAccount = new Account("Existing", "2", "Existing Desc", true, null, 1);
        var account = new Account("Account", "1", "Desc", true, null, 2);

        var request = new UpdateAccountRequest("2", "Updated", "Updated Desc", false, 2, null);
        accountRepositoryMoq.Setup(r => r.List())
            .Returns(new[] { account, existingAccount }.AsQueryable());

        // Act
        var result = await accountService.UpdateAsync(account.Id, request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequest<string>));
        Assert.AreEqual("Code already in use", ((BadRequest<string>)result).Value);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnBadRequest_WhenAccountTypeInvalid()
    {
        // Arrange
        var account = new Account("Account", "1", "Desc", true, null, 2);
        var request = new UpdateAccountRequest("1", "Updated", "Updated Desc", false, 99, null);

        accountRepositoryMoq.Setup(r => r.List())
            .Returns(new[] { account }.AsQueryable());
        accountTypeRepositoryMoq.Setup(r => r.ListAsNoTracking())
            .Returns(Enumerable.Empty<AccountType>().AsQueryable());

        // Act
        var result = await accountService.UpdateAsync(account.Id, request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequest<string>));
        Assert.AreEqual("Invalid account type", ((BadRequest<string>)result).Value);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldReturnBadRequest_WhenParentIdInvalid()
    {
        // Arrange
        var account = new Account("Account", "1", "Desc", true, null, 2);
        var request = new UpdateAccountRequest("1", "Updated", "Updated Desc", false, 2, 999);

        accountRepositoryMoq.Setup(r => r.List())
            .Returns(new[] { account }.AsQueryable());
        accountRepositoryMoq.Setup(r => r.GetAsync(999))
            .ReturnsAsync((Account)null);

        // Act
        var result = await accountService.UpdateAsync(account.Id, request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequest<string>));
        Assert.AreEqual("Invalid parent account id", ((BadRequest<string>)result).Value);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateAccount_WhenValidRequest()
    {
        // Arrange
        var account = new Account("Old Name", "1", "Old Desc", false, null, 2);
        var request = new UpdateAccountRequest("1", "New Name", "New Desc", false, 2, null);

        accountRepositoryMoq.Setup(r => r.List())
            .Returns(new[] { account }.AsQueryable());

        // Act
        var result = await accountService.UpdateAsync(account.Id, request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(Ok<UpdateAccountResponse>));
        var response = ((Ok<UpdateAccountResponse>)result).Value;
        Assert.AreEqual("New Name", response.Name);
        Assert.AreEqual("New Desc", response.Description);
        accountRepositoryMoq.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
