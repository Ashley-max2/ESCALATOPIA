using UnityEngine;

/// <summary>
/// Interface for hook physics and movement services.
/// Single Responsibility: Hook physics calculations and player impulse.
/// </summary>
public interface IHookPhysicsService
{
    /// <summary>
    /// Check if currently impulsing the player.
    /// </summary>
    bool IsImpulsing { get; }

    /// <summary>
    /// Calculate the impulse force for the hook.
    /// </summary>
    /// <param name="playerPosition">Current player position</param>
    /// <param name="hookPosition">Hook point position</param>
    /// <param name="playerRigidbody">Player rigidbody</param>
    /// <returns>The calculated impulse force</returns>
    Vector3 CalculateImpulseForce(Vector3 playerPosition, Vector3 hookPosition, Rigidbody playerRigidbody);

    /// <summary>
    /// Apply impulse to the player towards the hook point.
    /// </summary>
    /// <param name="playerRigidbody">Player rigidbody</param>
    /// <param name="hookPosition">Hook point position</param>
    void ApplyImpulse(Rigidbody playerRigidbody, Vector3 hookPosition);

    /// <summary>
    /// Update the hook impulse (called each frame while impulsing).
    /// </summary>
    /// <param name="playerPosition">Current player position</param>
    /// <param name="hookPosition">Hook point position</param>
    /// <param name="playerRigidbody">Player rigidbody</param>
    void UpdateImpulse(Vector3 playerPosition, Vector3 hookPosition, Rigidbody playerRigidbody);

    /// <summary>
    /// Stop the impulse.
    /// </summary>
    void StopImpulse();

    /// <summary>
    /// Check if the player has reached the hook point.
    /// </summary>
    /// <param name="playerPosition">Current player position</param>
    /// <param name="hookPosition">Hook point position</param>
    /// <returns>True if player has reached the hook point</returns>
    bool HasReachedHookPoint(Vector3 playerPosition, Vector3 hookPosition);
}
