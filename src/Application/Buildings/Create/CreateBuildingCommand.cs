using Application.Abstractions.Messaging;

namespace Application.Building.Create;

public sealed record CreateBuildingCommand(string Name, int NumberOfFloors)
    : ICommand<Guid>;
