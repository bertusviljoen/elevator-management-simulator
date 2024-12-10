using Application.Abstractions.Services;
using Domain.Elevators;
using Infrastructure.Persistence.SeedData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary> Background service for simulating elevator operations. </summary>
public class ElevatorSimulationHostedService(
    ILogger<ElevatorSimulationHostedService> logger,
    IInMemoryElevatorPoolService elevatorPoolService)
    : BackgroundService
{
    private readonly TimeSpan _simulationInterval = TimeSpan.FromSeconds(1);

    //ToDo: This should be configurable
    private readonly Guid _buildingId = ApplicationDbContextSeedData.GetSeedBuildings()!.FirstOrDefault()!.Id;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Elevator simulation service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Get all the elevators based on the building id
                var elevatorsResult = await elevatorPoolService.GetAllElevatorsAsync(_buildingId, stoppingToken);
                if (elevatorsResult.IsFailure)
                {
                    logger.LogWarning("Failed to get elevators: {Error}", elevatorsResult.Error);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Back off on error
                    continue;
                }

                foreach (var elevator in elevatorsResult.Value.AsParallel())
                {
                    logger.LogInformation(
                        "Simulating elevator {ElevatorId} on floor {Floor}",
                        elevator.Id, elevator.CurrentFloor);
                    
                    var elevatorChanged = true;
                    if (elevator.ElevatorStatus != ElevatorStatus.Active)
                    {
                        continue;
                    }

                    // Simulate elevator movement based on direction
                    switch (elevator.ElevatorDirection)
                    {
                        case ElevatorDirection.Up:
                            elevator.CurrentFloor += elevator.FloorsPerSecond;
                            //check for when speed elevators pass the destination floor
                            if (elevator.ElevatorType == ElevatorType.HighSpeed &&
                                elevator.CurrentFloor > elevator.DestinationFloor)
                            {
                                elevator.CurrentFloor = elevator.DestinationFloor;
                            }
                            break;
                        case ElevatorDirection.Down:
                            elevator.CurrentFloor -= elevator.FloorsPerSecond;
                            //check for when speed elevators pass the destination floor
                            if (elevator.ElevatorType == ElevatorType.HighSpeed &&
                                elevator.CurrentFloor < elevator.DestinationFloor)
                            {
                                elevator.CurrentFloor = elevator.DestinationFloor;
                            }
                            break;
                        case ElevatorDirection.None:
                            {
                                if (elevator.DestinationFloors.Count > 0)
                                {
                                    var destinationFloor = elevator.DestinationFloors.Dequeue();
                                    elevator.ElevatorDirection = destinationFloor > elevator.CurrentFloor
                                        ? ElevatorDirection.Up
                                        : ElevatorDirection.Down;
                                    elevator.DestinationFloor = destinationFloor;
                                    elevator.DoorStatus = ElevatorDoorStatus.Closed;
                                    break;
                                }
                                elevatorChanged = false;
                            }
                            break;
                    }
                    
                    if (elevator.CurrentFloor == elevator.DestinationFloor)
                    {
                        elevator.ElevatorDirection = ElevatorDirection.None;
                        elevator.DoorStatus = ElevatorDoorStatus.Open;
                    }
                    
                    if (elevator.ElevatorDirection == ElevatorDirection.None)
                    {
                        elevator.DoorStatus = ElevatorDoorStatus.Open;
                    }

                    // Update elevator state with new floor
                    if (elevatorChanged)
                    {
                        await elevatorPoolService.UpdateElevatorAsync(
                            elevator, stoppingToken);

                        logger.LogInformation(
                            "Elevator {ElevatorId} moved to floor {Floor}",
                            elevator.Id, elevator.CurrentFloor);
                    }
                }

                await Task.Delay(_simulationInterval, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "An error occurred while simulating elevator movements");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Back off on error
            }
        }
    }

}
