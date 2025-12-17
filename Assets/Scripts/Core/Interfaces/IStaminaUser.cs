/// <summary>
/// Interface for entities that use stamina/resistance.
/// Follows Interface Segregation Principle.
/// </summary>
public interface IStaminaUser
{
    /// <summary>
    /// Get the current stamina value.
    /// </summary>
    float CurrentStamina { get; }

    /// <summary>
    /// Get the maximum stamina value.
    /// </summary>
    float MaxStamina { get; }

    /// <summary>
    /// Check if there is enough stamina for an action.
    /// </summary>
    /// <param name="amount">Amount of stamina required</param>
    /// <returns>True if enough stamina is available</returns>
    bool HasStamina(float amount);

    /// <summary>
    /// Consume stamina for an action.
    /// </summary>
    /// <param name="amount">Amount of stamina to consume</param>
    /// <returns>True if stamina was consumed successfully</returns>
    bool ConsumeStamina(float amount);

    /// <summary>
    /// Regenerate stamina over time.
    /// </summary>
    /// <param name="amount">Amount of stamina to regenerate</param>
    void RegenerateStamina(float amount);

    /// <summary>
    /// Check if stamina is currently regenerating.
    /// </summary>
    bool IsRegenerating { get; }

    /// <summary>
    /// Check if stamina is depleted.
    /// </summary>
    bool IsDepleted { get; }
}
