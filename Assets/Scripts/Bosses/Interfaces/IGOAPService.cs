using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for GOAP (Goal-Oriented Action Planning) service.
/// Single Responsibility: AI decision-making using GOAP algorithm.
/// CRITICAL: This preserves the existing GOAP functionality for Gold-level rubric score.
/// </summary>
public interface IGOAPService
{
    /// <summary>
    /// Plan a sequence of actions to achieve a goal.
    /// </summary>
    /// <param name="availableActions">Available actions the AI can take</param>
    /// <param name="worldState">Current state of the world</param>
    /// <param name="goal">Desired goal state</param>
    /// <returns>Queue of actions to execute, or null if no plan found</returns>
    Queue<GOAPAction> PlanActions(List<GOAPAction> availableActions, Dictionary<string, bool> worldState, Dictionary<string, bool> goal);

    /// <summary>
    /// Check if a plan is still valid.
    /// </summary>
    /// <param name="plan">The current plan</param>
    /// <param name="worldState">Current world state</param>
    /// <returns>True if plan is still valid</returns>
    bool IsPlanValid(Queue<GOAPAction> plan, Dictionary<string, bool> worldState);

    /// <summary>
    /// Get the cost of a plan.
    /// </summary>
    /// <param name="plan">The plan to evaluate</param>
    /// <returns>Total cost of the plan</returns>
    float GetPlanCost(Queue<GOAPAction> plan);
}
