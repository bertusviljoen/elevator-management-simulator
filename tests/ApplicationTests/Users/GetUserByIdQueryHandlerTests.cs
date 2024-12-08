using Application.Abstractions.Authentication;
using Application.Users.GetById;
using Domain.Users;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Domain.Common;
using Infrastructure.Persistence.Database;

namespace ApplicationTests.Users;

public class GetUserByIdQueryHandlerTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public GetUserByIdQueryHandlerTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var publisherMock = new Mock<IPublisher>();
        publisherMock.Setup(x => x.Publish(It.IsAny<INotification>(), 
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var searchUserId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var query = new GetUserByIdQuery(searchUserId);

        await using var context = new ApplicationDbContext(_options);
        await context.Users.AddAsync(new User
        {
            Id = currentUserId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@yahoo.com",
            PasswordHash = "password"
        });
        await context.SaveChangesAsync();

        var handler = new GetUserByIdQueryHandler(context);

        // Act
        Result<UserResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.NotFound(query.UserId).Code, result.Error?.Code);
    }
}
