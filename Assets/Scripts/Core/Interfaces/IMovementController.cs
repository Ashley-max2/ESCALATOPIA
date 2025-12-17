using UnityEngine;

/// <summary>
/// Interface for movement capabilities following Interface Segregation Principle.
/// Entities that can move should implement this interface.
/// </summary>
public interface IMovementController
{
    /// <summary>
    /// Move the entity in a given direction.
    /// </summary>
    /// <param name="direction">Normalized direction vector</param>
    /// <param name="speed">Movement speed</param>
    void Move(Vector3 direction, float speed);

    /// <summary>
    /// Rotate the entity to face a direction.
    /// </summary>
    /// <param name="direction">Direction to face</param>
    /// <param name="smoothness">Rotation smoothness factor</param>
    void Rotate(Vector3 direction, float smoothness);

    /// <summary>
    /// Check if the entity is currently grounded.
    /// </summary>
    bool IsGrounded { get; }

    /// <summary>
    /// Get the current velocity of the entity.
    /// </summary>
    Vector3 Velocity { get; }

    /// <summary>
    /// Get the Rigidbody component.
    /// </summary>
    Rigidbody Rigidbody { get; }
}
