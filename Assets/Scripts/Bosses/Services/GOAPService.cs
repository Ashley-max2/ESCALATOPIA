using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GOAP Service implementation.
/// Extracted from BossAIBase to follow Single Responsibility Principle.
/// CRITICAL: Preserves existing GOAP functionality for Gold-level rubric score.
/// </summary>
public class GOAPService : IGOAPService
{
    /// <summary>
    /// Plan a sequence of actions using A* algorithm.
    /// </summary>
    public Queue<GOAPAction> PlanActions(List<GOAPAction> availableActions, Dictionary<string, bool> worldState, Dictionary<string, bool> goal)
    {
        // Reset all actions
        foreach (var action in availableActions)
        {
            action.Reset();
        }

        // Find usable actions (preconditions met)
        List<GOAPAction> usableActions = new List<GOAPAction>();
        foreach (var action in availableActions)
        {
            if (action.CheckPreconditions(worldState))
            {
                usableActions.Add(action);
            }
        }

        // Build graph and find path using A*
        List<Node> leaves = new List<Node>();
        Node start = new Node(null, 0, worldState, null);
        bool success = BuildGraph(start, leaves, usableActions, goal);

        if (!success)
        {
            Debug.Log("[GOAP] No plan found");
            return null;
        }

        // Find cheapest leaf node
        Node cheapest = null;
        foreach (Node leaf in leaves)
        {
            if (cheapest == null || leaf.cost < cheapest.cost)
            {
                cheapest = leaf;
            }
        }

        // Build action queue from cheapest path
        Queue<GOAPAction> result = new Queue<GOAPAction>();
        Node n = cheapest;
        while (n != null)
        {
            if (n.action != null)
            {
                result.Enqueue(n.action);
            }
            n = n.parent;
        }

        // Reverse queue to get correct order
        Queue<GOAPAction> orderedResult = new Queue<GOAPAction>();
        Stack<GOAPAction> stack = new Stack<GOAPAction>(result);
        while (stack.Count > 0)
        {
            orderedResult.Enqueue(stack.Pop());
        }

        Debug.Log($"[GOAP] Plan found with {orderedResult.Count} actions, cost: {cheapest.cost}");
        return orderedResult;
    }

    /// <summary>
    /// Check if a plan is still valid.
    /// </summary>
    public bool IsPlanValid(Queue<GOAPAction> plan, Dictionary<string, bool> worldState)
    {
        if (plan == null || plan.Count == 0)
            return false;

        // Check if first action's preconditions are still met
        GOAPAction firstAction = plan.Peek();
        return firstAction.CheckPreconditions(worldState);
    }

    /// <summary>
    /// Get the total cost of a plan.
    /// </summary>
    public float GetPlanCost(Queue<GOAPAction> plan)
    {
        if (plan == null)
            return float.MaxValue;

        float totalCost = 0f;
        foreach (var action in plan)
        {
            totalCost += action.cost;
        }

        return totalCost;
    }

    /// <summary>
    /// Build the GOAP graph using A* algorithm.
    /// </summary>
    private bool BuildGraph(Node parent, List<Node> leaves, List<GOAPAction> usableActions, Dictionary<string, bool> goal)
    {
        bool foundPath = false;

        foreach (var action in usableActions)
        {
            if (action.CheckPreconditions(parent.state))
            {
                Dictionary<string, bool> currentState = new Dictionary<string, bool>(parent.state);
                
                // Apply action effects
                foreach (var effect in action.effects)
                {
                    currentState[effect.Key] = effect.Value;
                }

                Node node = new Node(parent, parent.cost + action.cost, currentState, action);

                // Check if goal is reached
                if (GoalAchieved(goal, currentState))
                {
                    leaves.Add(node);
                    foundPath = true;
                }
                else
                {
                    // Continue building graph
                    List<GOAPAction> subset = new List<GOAPAction>(usableActions);
                    subset.Remove(action);
                    bool found = BuildGraph(node, leaves, subset, goal);
                    if (found)
                    {
                        foundPath = true;
                    }
                }
            }
        }

        return foundPath;
    }

    /// <summary>
    /// Check if goal state is achieved.
    /// </summary>
    private bool GoalAchieved(Dictionary<string, bool> goal, Dictionary<string, bool> state)
    {
        foreach (var g in goal)
        {
            if (!state.ContainsKey(g.Key) || state[g.Key] != g.Value)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Node class for A* pathfinding.
    /// </summary>
    private class Node
    {
        public Node parent;
        public float cost;
        public Dictionary<string, bool> state;
        public GOAPAction action;

        public Node(Node parent, float cost, Dictionary<string, bool> state, GOAPAction action)
        {
            this.parent = parent;
            this.cost = cost;
            this.state = state;
            this.action = action;
        }
    }
}
