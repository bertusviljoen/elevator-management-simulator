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
