namespace Application;

/// <summary> Strategy that selects an elevator based on its current queue capacity. </summary>
public interface IQueueCapacityStrategy : IElevatorSelectionStrategy { }
