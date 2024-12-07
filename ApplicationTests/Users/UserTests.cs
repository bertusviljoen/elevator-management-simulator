using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Users.Login;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using SharedKernel;
using Xunit;

namespace ApplicationTests.Users;


    public class UserTests
    {
        [Fact]
        public async Task Handle_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var command = new LoginUserCommand("valid@example.com", "ValidPassword");
            var user = new User 
            { 
                Id = Guid.NewGuid(), 
                Email = "valid@example.com",
                PasswordHash = "hashedPassword", 
                FirstName = "Bertus", 
                LastName = "Viljoen" 
            };

            Mock<DbSet<User>> usersMock = DbSetMock.CreateMockSet(new List<User> { user });

            var contextMock = new Mock<IApplicationDbContext>();
            contextMock.Setup(c => c.Users).Returns(usersMock.Object);

            var passwordHasherMock = new Mock<IPasswordHasher>();
            passwordHasherMock.Setup(p => p.Verify("ValidPassword", "hashedPassword"))
                .Returns(true);

            var tokenProviderMock = new Mock<ITokenProvider>();
            tokenProviderMock.Setup(t => t.Create(user))
                .Returns("validToken");

            var handler = new LoginUserCommandHandler(contextMock.Object, passwordHasherMock.Object, tokenProviderMock.Object);

            // Act
            Result<string> result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("validToken", result.Value);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var command = new LoginUserCommand("invalid@example.com", "InvalidPassword");

            // No users in the database set
            var usersMock = DbSetMock.CreateMockSet(new List<User>());

            var contextMock = new Mock<IApplicationDbContext>();
            contextMock.Setup(c => c.Users).Returns(usersMock.Object);

            var passwordHasherMock = new Mock<IPasswordHasher>();
            var tokenProviderMock = new Mock<ITokenProvider>();

            var handler = new LoginUserCommandHandler(contextMock.Object,
                passwordHasherMock.Object, tokenProviderMock.Object);

            // Act
            Result<string> result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(UserErrors.NotFoundByEmail, result.Error);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenPasswordIsInvalid()
        {
            // Arrange
            var command = new LoginUserCommand("valid@example.com", "InvalidPassword");
            var user = new User 
            { 
                Id = Guid.NewGuid(), 
                Email = "valid@example.com", 
                PasswordHash = "hashedPassword", 
                FirstName = "Bertus", 
                LastName = "Viljoen" 
            };

            var usersMock = DbSetMock.CreateMockSet(new List<User> { user });

            var contextMock = new Mock<IApplicationDbContext>();
            contextMock.Setup(c => c.Users).Returns(usersMock.Object);

            var passwordHasherMock = new Mock<IPasswordHasher>();
            passwordHasherMock.Setup(p => p.Verify("InvalidPassword", "hashedPassword")).Returns(false);

            var tokenProviderMock = new Mock<ITokenProvider>();

            var handler = new LoginUserCommandHandler(contextMock.Object, passwordHasherMock.Object, tokenProviderMock.Object);

            // Act
            Result<string> result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(UserErrors.NotFoundByEmail, result.Error);
        }
    }
