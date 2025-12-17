using UnityEngine;

/// <summary>
/// Interface for boss movement capabilities.
/// Single Responsibility: Boss movement logic.
/// </summary>
public interface IBossMovement
{
    /// <summary>
    /// Move the boss towards a target position.
    /// </summary>
    /// <param name="targetPosition">Target position to move to</param>
    /// <param name="speed">Movement speed</param>
    void MoveTowards(Vector3 targetPosition, float speed);

    /// <summary>
    /// Climb vertically.
    /// </summary>
    /// <param name="direction">Climbing direction (up/down)</param>
    /// <param name="speed">Climbing speed</param>
    void Climb(Vector3 direction, float speed);

    /// <summary>
    /// Launch hook towards a target.
    /// </summary>
    /// <param name="hookPoint">Hook point to target</param>
    void LaunchHook(IHookable hookPoint);

    /// <summary>
    /// Check if the boss is on a climbable wall.
    /// </summary>
    bool IsOnClimbableWall { get; }

    /// <summary>
    /// Check if the boss is currently moving.
    /// </summary>
    bool IsMoving { get; }

    /// <summary>
    /// Check if the boss is currently climbing.
    /// </summary>
    bool IsClimbing { get; }

    /// <summary>
    /// Check if the boss is currently using the hook.
    /// </summary>
    bool IsHooking { get; }

    /// <summary>
    /// Stop all movement.
    /// </summary>
    void Stop();
}
