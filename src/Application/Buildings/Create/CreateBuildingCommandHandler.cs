using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Buildings;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Buildings.Create;

public sealed class CreateBuildingCommandHandler(
    IApplicationDbContext applicationDbContext,
    IUserContext userContext) : ICommandHandler<CreateBuildingCommand,Guid>
{
    public async Task<Result<Guid>> Handle(CreateBuildingCommand request, CancellationToken cancellationToken)
    {
        //check if building with the same name exists
        Domain.Buildings.Building? building = await applicationDbContext.Buildings
            .SingleOrDefaultAsync(a => a.Name == request.Name, cancellationToken);

        if (building is not null)
        {
            return Result.Failure<Guid>(BuildingErrors.NameNotUnique(request.Name));
        }
        
        //create the building
        building = new Domain.Buildings.Building
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            NumberOfFloors = request.NumberOfFloors
        };
        
        building.Raise(new BuildingCreatedDomainEvent(building.Id));
        
        applicationDbContext.Buildings.Add(building);
        
        await applicationDbContext.SaveChangesAsync(cancellationToken);
        
        return building.Id;
    }
}
