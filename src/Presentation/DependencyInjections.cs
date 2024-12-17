using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Presentation;
using Presentation.ElevatorControl;

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
         services.AddTransient<DashboardScreen>();
         services.AddTransient<ElevatorControlScreen>();
         services.AddTransient<ElevatorControlMultipleRequestScreen>();
         services.AddTransient<ConfigurationMenu>();
         return services;
    }
    

}
