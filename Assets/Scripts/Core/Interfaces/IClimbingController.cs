using UnityEngine;

/// <summary>
/// Interface for climbing capabilities following Interface Segregation Principle.
/// Entities that can climb should implement this interface.
/// </summary>
public interface IClimbingController
{
    /// <summary>
    /// Check if the entity is in a climbable zone.
    /// </summary>
    bool IsInClimbableZone { get; }

    /// <summary>
    /// Check if the entity can start climbing.
    /// </summary>
    /// <returns>True if climbing can be initiated</returns>
    bool CanStartClimbing();

    /// <summary>
    /// Perform climbing movement.
    /// </summary>
    /// <param name="direction">Climbing direction (vertical and horizontal)</param>
    /// <param name="speed">Climbing speed</param>
    void Climb(Vector3 direction, float speed);

    /// <summary>
    /// Enter a climbable zone.
    /// </summary>
    void EnterClimbableZone();

    /// <summary>
    /// Exit a climbable zone.
    /// </summary>
    void ExitClimbableZone();

    /// <summary>
    /// Get the climbing surface normal.
    /// </summary>
    Vector3 ClimbingSurfaceNormal { get; }
}
