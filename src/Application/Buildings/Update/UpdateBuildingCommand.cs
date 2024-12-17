using Application;

namespace Application;

/// <summary> Command to update a building's name and floors </summary>
public sealed record UpdateBuildingCommand(Guid Id, string Name, int NumberOfFloors)
    : ICommand;
