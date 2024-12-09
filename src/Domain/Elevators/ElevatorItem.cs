using System.Text.Json.Serialization;

namespace Domain.Elevators;

/// <summary> Data transfer object representing an elevator in memory. </summary>
public class ElevatorItem
{
    /// <summary> Get or set the unique identifier for the elevator. </summary>
    public Guid Id { get; set; }

    /// <summary> Get or set the number of the elevator. </summary>
    public int Number { get; set; }

    /// <summary> Get or set the current floor the elevator is on. </summary>
    public int CurrentFloor { get; set; }

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
    public double Speed { get; set; } = 0.5;

    /// <summary> Get or set the capacity of the elevator. </summary>
    public int Capacity { get; set; } = 10;

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
        Speed = elevator.Speed,
        Capacity = elevator.Capacity,
        BuildingId = elevator.BuildingId
    };

    public Elevator ToElevator() => new()
    {
        Id = Id,
        Number = Number,
        CurrentFloor = CurrentFloor,
        ElevatorDirection = ElevatorDirection,
        ElevatorStatus = ElevatorStatus,
        ElevatorType = ElevatorType,
        Speed = Speed,
        Capacity = Capacity,
        BuildingId = BuildingId
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
        Speed = Speed,
        Capacity = Capacity,
        BuildingId = BuildingId
    };
}
