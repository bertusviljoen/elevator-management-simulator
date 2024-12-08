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
}
