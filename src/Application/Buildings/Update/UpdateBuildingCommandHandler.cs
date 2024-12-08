using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Buildings;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Buildings.Update;

public class UpdateBuildingCommandHandler(
    IApplicationDbContext applicationDbContext) : ICommandHandler<UpdateBuildingCommand>
{
    public async Task<Result> Handle(UpdateBuildingCommand request, CancellationToken cancellationToken)
    {
        Building? building = await applicationDbContext.Buildings
            .SingleOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (building is null)
        {
            return Result.Failure(BuildingErrors.NotFound(request.Id));
        }

        //update the building
        building.Name = request.Name;
        building.NumberOfFloors = request.NumberOfFloors;

        applicationDbContext.Buildings.Update(building);

        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
