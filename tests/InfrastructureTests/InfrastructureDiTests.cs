using System.Diagnostics.CodeAnalysis;
using Application;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InfrastructureTests;

[SuppressMessage("ReSharper", "UnusedVariable")]
[SuppressMessage("ReSharper", "SuggestVarOrType_SimpleTypes")]
public class InfrastructureDiTests
{

    private readonly IServiceProvider _serviceProvider;
    
    public InfrastructureDiTests()
    {
        IHost host = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddApplication();
                services.AddInfrastructure(hostContext.Configuration);
            })
            .Build();
        
        _serviceProvider = host.Services;   
    }
    
    //Di check for each
    [Fact]
    public void CanResolveUserContext()
    {
        //Assert
        var userContext = _serviceProvider.GetRequiredService<IUserContext>();
        Assert.NotNull(userContext);
    }
    
    //token provider
    [Fact]
    public void CanResolveTokenProvider()
    {
        //Assert
        var tokenProvider = _serviceProvider.GetRequiredService<ITokenProvider>();
        Assert.NotNull(tokenProvider);
    }
    
    //password hasher
    [Fact]
    public void CanResolvePasswordHasher()
    {
        //Assert
        var passwordHasher = _serviceProvider.GetRequiredService<IPasswordHasher>();
        Assert.NotNull(passwordHasher);
    }
    
    //db context
    [Fact]
    public void CanResolveDbContext()
    {
        //Assert
        var dbContext = _serviceProvider.GetRequiredService<IApplicationDbContext>();
        Assert.NotNull(dbContext);
    }
}
