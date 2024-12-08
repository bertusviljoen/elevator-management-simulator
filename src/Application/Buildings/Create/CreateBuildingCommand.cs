using Application.Abstractions.Messaging;

namespace Application.Buildings.Create;

public sealed record CreateBuildingCommand(string Name, int NumberOfFloors)
    : ICommand<Guid>;
