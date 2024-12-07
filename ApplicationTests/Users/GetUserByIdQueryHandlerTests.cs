using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Users.GetById;
using Domain.Users;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using SharedKernel;

namespace ApplicationTests.Users;

public class GetUserByIdQueryHandlerTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly IUserContext _userContext;
    private readonly IPublisher _publisher;

    public GetUserByIdQueryHandlerTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var userContextMock = new Mock<IUserContext>();
        userContextMock.SetupGet(x => x.UserId)
            .Returns(Guid.NewGuid());
        _userContext = userContextMock.Object;

        var publisherMock = new Mock<IPublisher>();
        publisherMock.Setup(x => x.Publish(It.IsAny<INotification>(), 
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _publisher = publisherMock.Object;
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var searchUserId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var query = new GetUserByIdQuery(searchUserId);

        await using var context = new ApplicationDbContext(_options, _publisher);
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
