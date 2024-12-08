namespace Domain.Elevators;

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
