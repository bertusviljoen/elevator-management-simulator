namespace Domain.Elevators;

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
