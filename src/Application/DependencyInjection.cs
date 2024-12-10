using Application.Abstractions.Behaviors;
using Application.Abstractions.Services;
using Application.Services;
using Application.Services.ElevatorSelection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            config.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        // Register elevator services
        services.AddSingleton<IInMemoryElevatorPoolService, InMemoryElevatorPoolService>();
        services.AddTransient<IElevatorOrchestratorService, ElevatorOrchestratorService>();

        // Strategy pattern for elevator selection
        services.AddTransient<IElevatorSelectionContext, ElevatorSelectionContext>();
        services.AddTransient<IClosestElevatorStrategy, ClosestElevatorStrategy>();
        services.AddTransient<IQueueCapacityStrategy, QueueCapacityStrategy>();
        return services;
    }
}
