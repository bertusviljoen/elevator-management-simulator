using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Domain.Buildings;
using Domain.Common;
using Domain.Elevators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Elevators.Request;

/// <summary> Request an elevator to a specific floor. </summary>
public class RequestElevatorCommandHandler(
    ILogger<RequestElevatorCommandHandler> logger,
    IElevatorOrchestratorService elevatorOrchestratorService,
    IApplicationDbContext applicationDbContext
    ) : ICommandHandler<RequestElevatorCommand,Guid>
{
    /// <summary> Handle the request to send an elevator to a specific floor. </summary>
    public async Task<Result<Guid>> Handle(RequestElevatorCommand request, CancellationToken cancellationToken)
    {
        //Check if building exists
        var building = await applicationDbContext.Buildings.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.BuildingId, cancellationToken);

        if (building == null)
        {
            return Result.Failure<Guid>(BuildingErrors.NotFound(request.BuildingId));
        }
        
        if (request.FloorNumber < 1 || request.FloorNumber > building.NumberOfFloors)
        {
            return Result.Failure<Guid>(BuildingErrors.InvalidFloorNumber(request.FloorNumber));
        }
        
        logger.LogInformation("Requesting elevator to floor {Floor}", request.FloorNumber);
        var requestResponse =  await elevatorOrchestratorService
            .RequestElevatorAsync(request.BuildingId, request.FloorNumber, cancellationToken);
        
        if (requestResponse.IsFailure)
        {
            logger.LogError("Failed to request elevator to floor {Floor}", request.FloorNumber);
            return Result.Failure<Guid>(ElevatorErrors.ElevatorRequestFailed(request.FloorNumber));
        }
        
        logger.LogInformation("Elevator requested to floor {Floor}", request.FloorNumber);
        return Guid.NewGuid();
    }
}
