using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using MediatR;

namespace Domain;

/// <summary> Entity representing a building. </summary>
public sealed class Building : AuditableEntity
{
    /// <summary> Get or set the unique identifier for the building. </summary>
    public required Guid Id { get; init; }
    /// <summary> Get or set the name of the building. </summary>
    public required string Name { get; set; }
    /// <summary> Get or set the number of floors in the building. </summary>
    public required int NumberOfFloors { get; set; }

    /// <summary> Get or set if this building is the default building. </summary>
    public bool IsDefault { get; set; } = true;
}


/// <summary> Event that is raised when a new building is created. </summary>
/// <param name="BuildingId"> The unique identifier of the building. </param>
public sealed record BuildingCreatedDomainEvent(Guid BuildingId) : IDomainEvent;


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




/// <summary> Base abstract class for entities that have audit and timestamp tracking. </summary>
public abstract class AuditableEntity 
    : Entity, ITimeAuditedEntity, IUserAuditedEntity
{
    /// <inheritdoc />
    [Required]
    public DateTime CreatedDateTimeUtc { get; set; }
    /// <inheritdoc />
    public DateTime? UpdatedDateTimeUtc { get; set; }
    /// <inheritdoc />
    [Required]
    public Guid CreatedByUserId { get; set; }
    /// <inheritdoc />
    public Guid? UpdatedByUserId { get; set; }
    /// <inheritdoc />
    [Required]
    public User CreatedByUser { get; set; }
    /// <inheritdoc />
    public User? UpdatedByUser { get; set; }
}


/// <summary> Base abstract class for entities which have domain events. </summary>
public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public List<IDomainEvent> DomainEvents => [.. _domainEvents];

    /// <summary> Clear all domain events to ensure that they are not dispatched more than once. </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary> Add a domain event to the entity to be dispatched. Raise actually adds the event to the list. </summary>
    public void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}


public record Error
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new(
        "General.Null",
        "Null value was provided",
        ErrorType.Failure);

    public Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public string Code { get; }

    public string Description { get; }

    public ErrorType Type { get; }

    public static Error Failure(string code, string description)
    {
        return new(code, description, ErrorType.Failure);
    }

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);
}


public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    Problem = 2,
    NotFound = 3,
    Conflict = 4
}


public interface IDateTimeProvider
{
    public DateTime UtcNow { get; }
}




public interface IDomainEvent : INotification;


/// <summary> Describes database entities that tracks timestamps when data is modified. </summary>
public interface ITimeAuditedEntity
{
    /// <summary> The timestamp of when the entity was created. This is required. </summary>
    DateTime CreatedDateTimeUtc { get; set; }
    /// <summary> The timestamp of when the entity was modified.</summary>
    DateTime? UpdatedDateTimeUtc { get; set; }
}


/// <summary> Describes database entities that tracks users that modify data. </summary>
public interface IUserAuditedEntity
{
    /// <summary> The user that created the entity. This is required. </summary>
    Guid CreatedByUserId { get; set; }

    /// <summary> The user that modified the entity. This is required and will default to <seealso cref="CreatedByUserId"/>. </summary>
    Guid? UpdatedByUserId { get; set; }

    /// <summary> Created by User which is a navigation property to the User entity. </summary>
    public abstract  User CreatedByUser { get; set; }
    /// <summary> Updated by User which is a navigation property to the User entity. </summary>
    public abstract User? UpdatedByUser { get; set; }
}




public class Result
{
    public Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    public static Result<TValue> ValidationFailure(Error error) =>
        new(default, false, error);
}


public sealed record ValidationError : Error
{
    public ValidationError(Error[] errors)
        : base(
            "Validation.General",
            "One or more validation errors occurred",
            ErrorType.Validation)
    {
        Errors = errors;
    }

    public Error[] Errors { get; }

    public static ValidationError FromResults(IEnumerable<Result> results)
    {
        return new(results.Where(r => r.IsFailure).Select(r => r.Error).ToArray());
    }
}




/// <summary> Entity representing an elevator in a building. </summary>
public class Elevator : AuditableEntity
{
    /// <summary> Get or set the unique identifier for the elevator. </summary>
    public required Guid Id { get; set; }
    /// <summary> Get or set the number of the elevator. </summary>
    public required int Number { get; set; }
    /// <summary> Get or set the current floor the elevator is on. </summary>
    public required int CurrentFloor { get; set; }

    /// <summary> Get or set the destination floor the elevator is moving to. </summary>
    public int DestinationFloor { get; set; } = 0;

    /// <summary> Get or set the destination floors the elevator is moving to. </summary>
    public string DestinationFloors { get; set; } = string.Empty;
    /// <summary> Get or set the status of the elevator door. </summary>
    public ElevatorDoorStatus DoorStatus { get; set; }
    
    /// <summary> Get or set the direction the elevator is moving. </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ElevatorDirection ElevatorDirection { get; set; }
    /// <summary> Get or set the status of the elevator. </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ElevatorStatus ElevatorStatus { get; set; }
    /// <summary> Get or set the type of elevator. </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ElevatorType ElevatorType { get; set; }
    /// <summary> Get or set the speed of the elevator. </summary>
    public int FloorsPerSecond { get; set; } = 1;
    /// <summary> Get or set the capacity of the elevator. </summary>
    public int QueueCapacity { get; set; } = 3;
    /// <summary> Get or set the unique identifier of the building the elevator is in. </summary>
    public required Guid BuildingId { get; set; }
    /// <summary> Get or set the building the elevator is in. </summary>
    public virtual Building Building { get; set; }

    /// <summary> Override the ToString method to provide a single line string representation of the elevator. </summary>
    /// <returns></returns>
    public override string ToString()
    {
        //Single line with all the property values
        return $"Elevator {Number} is on floor {CurrentFloor} and is moving to {DestinationFloor} with status {ElevatorStatus}";
    }
}


/// <summary> Enum representing the direction an elevator is moving. </summary>
public enum ElevatorDirection
{
    /// <summary> The elevator is moving up. </summary>
    Up,
    /// <summary> The elevator is moving down. </summary>
    Down,
    /// <summary> The elevator is not moving. </summary>    
    None
}


/// <summary> Enum representing the status of an elevator. </summary>
public enum ElevatorDoorStatus
{
    /// <summary> The elevator door is open. </summary>
    Open,
    /// <summary> The elevator door is closed. </summary>
    Closed
}


public static class ElevatorErrors
{
    public static Error NoElevatorFound(Guid elevatorId) => Error.NotFound(
        "Elevator.NotFound",
            $"No elevator found with ID {elevatorId}.");
    
    public static Error NoElevatorFound(int elevatorNumber) => Error.NotFound(
        "Elevator.NotFound",
            $"No elevator found with number {elevatorNumber}.");
    
    public static Error ElevatorRequestFailed(int floorNumber) => Error.Problem(
        "Elevator.RequestFailed",
            $"The request to the elevator service failed for floor number {floorNumber}.");
    
    public static Error NoElevatorsAvailable() => Error.Problem(
        "Elevator.NoElevatorsAvailable",
            "No elevators are available to service the request.");
}




/// <summary> Data transfer object representing an elevator in memory. </summary>
public class ElevatorItem
{
    /// <summary> Get or set the unique identifier for the elevator. </summary>
    public Guid Id { get; set; }

    /// <summary> Get or set the number of the elevator. </summary>
    public int Number { get; set; }

    /// <summary> Get or set the current floor the elevator is on. </summary>
    public int CurrentFloor { get; set; }
    
    /// <summary> Get or set the destination floor the elevator is moving to. </summary>
    public int DestinationFloor { get; set; }
    
    /// <summary> Get or set the destination floors the elevator is moving to. </summary>
    public Queue<int> DestinationFloors { get; set; } = new();
    
    /// <summary> Get or set the status of the elevator door. </summary>
    public ElevatorDoorStatus DoorStatus { get; set; }

    /// <summary> Get or set the direction the elevator is moving. </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ElevatorDirection ElevatorDirection { get; set; }

    /// <summary> Get or set the status of the elevator. </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ElevatorStatus ElevatorStatus { get; set; }

    /// <summary> Get or set the type of elevator. </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ElevatorType ElevatorType { get; set; }

    /// <summary> Get or set the speed of the elevator. </summary>
    public int FloorsPerSecond { get; set; } = 1;

    /// <summary> Get or set the capacity of the elevator. </summary>
    public int QueueCapacity { get; set; } = 10;

    /// <summary> Get or set the unique identifier of the building the elevator is in. </summary>
    public Guid BuildingId { get; set; }

    public static ElevatorItem FromElevator(Elevator elevator) => new()
    {
        Id = elevator.Id,
        Number = elevator.Number,
        CurrentFloor = elevator.CurrentFloor,
        ElevatorDirection = elevator.ElevatorDirection,
        ElevatorStatus = elevator.ElevatorStatus,
        ElevatorType = elevator.ElevatorType,
        FloorsPerSecond = elevator.FloorsPerSecond,
        QueueCapacity = elevator.QueueCapacity,
        BuildingId = elevator.BuildingId,
        DestinationFloor = elevator.DestinationFloor,
        DestinationFloors = new Queue<int>(string.IsNullOrWhiteSpace(elevator.DestinationFloors) ? new int[0] : elevator.DestinationFloors.Split(',').Select(int.Parse)),
        DoorStatus = elevator.DoorStatus
    };
    

    /// <summary> Creates a deep copy of the elevator item. </summary>
    public ElevatorItem Clone() => new()
    {
        Id = Id,
        Number = Number,
        CurrentFloor = CurrentFloor,
        ElevatorDirection = ElevatorDirection,
        ElevatorStatus = ElevatorStatus,
        ElevatorType = ElevatorType,
        FloorsPerSecond = FloorsPerSecond,
        QueueCapacity = QueueCapacity,
        BuildingId = BuildingId,
        DestinationFloor = DestinationFloor,
        DestinationFloors = new Queue<int>(DestinationFloors),
        DoorStatus = DoorStatus
    };
}


public static class ElevatorSectionErrors
{
    public static Error NoElevatorsAvailable() => Error.Problem(
        "ElevatorSection.NoElevatorsAvailable",
            "No elevators are available to service the request.");
    public static Error NoElevatorFound() => Error.NotFound(
        "ElevatorSection.NoElevatorFound",
            $"No elevator found");
}


/// <summary> Enum representing the status of an elevator. </summary>
public enum ElevatorStatus
{
    /// <summary> The elevator is active and in service. </summary>
    Active,
    /// <summary> The elevator is inactive and not in service. </summary>
    Inactive,
    /// <summary> The elevator is under maintenance. </summary>
    Maintenance,
    /// <summary> The elevator is out of service. </summary>
    OutOfService
}


/// <summary> Enum representing the type of elevator. </summary>
public enum ElevatorType
{
    /// <summary> Passenger elevators are designed to carry people between floors of a building. </summary>
    Passenger,
    /// <summary> Freight elevators are designed to carry heavy loads between floors of a building. </summary>
    Freight,
    /// <summary> Service elevators are designed to carry people and heavy loads between floors of a building and are typically used by staff. </summary>
    Service,
    /// <summary> High-speed elevators are designed to carry people between floors of a building at high speeds. </summary>
    HighSpeed,
}


/// <summary> Domain event for when an elevator is updated. </summary>
public record ElevatorUpdatedDomainEvent(Elevator Elevator) : IDomainEvent;




/// <summary> The following class represents a user entity. </summary>
public sealed class User : Entity
{
    /// <summary> Get the user's email. </summary>
    public required Guid Id { get; init; }
    /// <summary> Get the user's email. </summary>
    public required string Email { get; init; }
    /// <summary> Get the user's first name. </summary>
    public required string FirstName { get; init; }
    /// <summary> Get the user's last name. </summary>
    public required string LastName { get; init; }
    /// <summary> Get the user's password hash. </summary>
    public required string PasswordHash { get; init; }
}


public static class UserErrors
{
    public static Error NotFound(Guid userId) => Error.NotFound(
        "Users.NotFound",
        $"The user with the Id = '{userId}' was not found");

    public static Error Unauthorized() => Error.Failure(
        "Users.Unauthorized",
        "You are not authorized to perform this action.");

    public static readonly Error NotFoundByEmail = Error.NotFound(
        "Users.NotFoundByEmail",
        "The user with the specified email was not found");

    public static readonly Error EmailNotUnique = Error.Conflict(
        "Users.EmailNotUnique",
        "The provided email is not unique");
}


public sealed record UserRegisteredDomainEvent(Guid UserId) : IDomainEvent;
