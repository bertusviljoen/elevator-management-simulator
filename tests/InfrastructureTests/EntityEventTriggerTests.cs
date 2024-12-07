using System.Diagnostics.CodeAnalysis;
using Application;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Domain.Users;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InfrastructureTests;

[SuppressMessage("ReSharper", "UnusedVariable")]
[SuppressMessage("ReSharper", "SuggestVarOrType_SimpleTypes")]
public class EntityEventTriggerTests
{
    [Fact]
    public async Task UserRegisteredDomainEventTriggered()
    {
        IHost host = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddApplication();
                services.AddInfrastructure(hostContext.Configuration, true);
            })
            .Build();
        
        var passwordHasher = host.Services.GetRequiredService<IPasswordHasher>();
        var context = host.Services.GetRequiredService<IApplicationDbContext>();
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            PasswordHash = passwordHasher.Hash(Faker.Name.First()),
        };

        user.Raise(new UserRegisteredDomainEvent(user.Id));

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync(CancellationToken.None);
        
        //Check if interceptor is triggered
        Assert.False(user.DomainEvents.Any());
        
    }
}
