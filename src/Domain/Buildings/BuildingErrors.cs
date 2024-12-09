using Domain.Common;

namespace Domain.Buildings;

public static class BuildingErrors
{
    public static Error NotFound(Guid buildingId) => Error.NotFound(
        "Buildings.NotFound",
        $"The building with the Id = '{buildingId}' was not found");

    public static readonly Error NotFoundByName = Error.NotFound(
        "Buildings.NotFoundByName",
        "The building with the specified name was not found");
    public static Error NameNotUnique(string name) => Error.Conflict(
        "Buildings.NameNotUnique",
        $"The provided name '{name}' is not unique");
    public static Error InvalidFloorNumber(int floorNumber) => Error.Problem(
        "Buildings.InvalidFloorNumber",
        $"The provided floor number '{floorNumber}' is invalid. Floor numbers must be greater than 0");
    
    public static Error FloorDoesNotExist(int floorNumber) => Error.Problem(
        "Buildings.FloorDoesNotExist",
        $"The provided floor number '{floorNumber}' does not exist in the building");
}
