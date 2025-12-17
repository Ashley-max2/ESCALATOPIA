using UnityEngine;

/// <summary>
/// Interface for hook targeting services.
/// Single Responsibility: Finding and validating hook targets.
/// </summary>
public interface IHookTargetingService
{
    /// <summary>
    /// Find the best hook point within range and view.
    /// </summary>
    /// <param name="origin">Origin position for the search</param>
    /// <param name="direction">Direction to search in</param>
    /// <param name="maxDistance">Maximum search distance</param>
    /// <returns>The best hook point found, or null if none</returns>
    IHookable FindBestHookPoint(Vector3 origin, Vector3 direction, float maxDistance);

    /// <summary>
    /// Check if a hook point is valid and reachable.
    /// </summary>
    /// <param name="hookPoint">The hook point to validate</param>
    /// <param name="origin">Origin position</param>
    /// <returns>True if the hook point is valid</returns>
    bool IsHookPointValid(IHookable hookPoint, Vector3 origin);

    /// <summary>
    /// Get all hook points within range.
    /// </summary>
    /// <param name="origin">Origin position</param>
    /// <param name="maxDistance">Maximum distance</param>
    /// <returns>Array of hook points within range</returns>
    IHookable[] GetHookPointsInRange(Vector3 origin, float maxDistance);

    /// <summary>
    /// Highlight the currently targeted hook point.
    /// </summary>
    /// <param name="hookPoint">Hook point to highlight</param>
    void HighlightTarget(IHookable hookPoint);

    /// <summary>
    /// Clear any target highlighting.
    /// </summary>
    void ClearHighlight();
}
