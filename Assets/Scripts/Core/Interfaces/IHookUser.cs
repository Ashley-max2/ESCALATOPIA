using UnityEngine;

/// <summary>
/// Interface for entities that can use the hook system.
/// Follows Dependency Inversion Principle - HookSystem depends on this abstraction.
/// </summary>
public interface IHookUser
{
    /// <summary>
    /// Get the transform of the hook user.
    /// </summary>
    Transform Transform { get; }

    /// <summary>
    /// Get the rigidbody of the hook user.
    /// </summary>
    Rigidbody Rigidbody { get; }

    /// <summary>
    /// Get the position where the hook originates from.
    /// </summary>
    Vector3 HookOriginPosition { get; }

    /// <summary>
    /// Check if the hook user is currently being impulsed by the hook.
    /// </summary>
    bool IsBeingImpulsed { get; set; }

    /// <summary>
    /// Notify the hook user that the hook has been launched.
    /// </summary>
    void OnHookLaunched();

    /// <summary>
    /// Notify the hook user that the hook has attached to a point.
    /// </summary>
    /// <param name="hookPoint">The hook point that was attached</param>
    void OnHookAttached(IHookable hookPoint);

    /// <summary>
    /// Notify the hook user that the hook impulse has started.
    /// </summary>
    void OnHookImpulseStarted();

    /// <summary>
    /// Notify the hook user that the hook impulse has ended.
    /// </summary>
    void OnHookImpulseEnded();

    /// <summary>
    /// Notify the hook user that the hook has been cancelled.
    /// </summary>
    void OnHookCancelled();
}
