using System.Text.Json.Serialization;
using Domain.Buildings;
using Domain.Common;

namespace Domain.Elevators;

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
