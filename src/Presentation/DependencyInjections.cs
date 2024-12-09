using Infrastructure.Database;
using Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Presentation.Screens;

namespace Presentation;

/// <summary> Contains service collection extension methods for registering screens. </summary>
public static class DependencyInjections
{
    /// <summary> Registers the screens. </summary>
    public static IServiceCollection AddScreens(
        this IServiceCollection services,
        IConfiguration configuration)
    {
         services.AddTransient<MenuScreen>();
         services.AddTransient<RegisterScreen>();
         services.AddTransient<LoginScreen>();
         return services;
    }
    

}
