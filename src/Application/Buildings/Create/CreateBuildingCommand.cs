using Application;

namespace Application;

public sealed record CreateBuildingCommand(string Name, int NumberOfFloors)
    : ICommand<Guid>;
