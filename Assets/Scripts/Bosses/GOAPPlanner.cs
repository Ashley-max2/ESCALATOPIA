using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Planificador GOAP que encuentra la secuencia óptima de acciones
/// </summary>
public class GOAPPlanner
{
    /// <summary>
    /// Nodo del grafo de planificación
    /// </summary>
    private class Node
    {
        public Node parent;
        public float runningCost;
        public Dictionary<string, object> state;
        public GOAPAction action;

        public Node(Node parent, float runningCost, Dictionary<string, object> state, GOAPAction action)
        {
            this.parent = parent;
            this.runningCost = runningCost;
            this.state = state;
            this.action = action;
        }
    }

    /// <summary>
    /// Planifica una secuencia de acciones para alcanzar el objetivo
    /// </summary>
    public Queue<GOAPAction> Plan(BossAIBase agent, List<GOAPAction> availableActions, 
                                   Dictionary<string, object> worldState, 
                                   Dictionary<string, object> goal)
    {
        // Filtrar acciones que pueden ejecutarse
        List<GOAPAction> usableActions = availableActions.Where(a => a.CheckProceduralPrecondition(agent)).ToList();

        // Crear nodo inicial
        List<Node> leaves = new List<Node>();
        Node start = new Node(null, 0, worldState, null);

        // Construir el grafo
        bool success = BuildGraph(start, leaves, usableActions, goal);

        if (!success)
        {
            Debug.LogWarning("GOAP: No se encontró plan válido");
            return null;
        }

        // Encontrar el nodo hoja con menor costo
        Node cheapest = null;
        foreach (Node leaf in leaves)
        {
            if (cheapest == null || leaf.runningCost < cheapest.runningCost)
            {
                cheapest = leaf;
            }
        }

        // Reconstruir el plan desde el nodo más barato
        List<GOAPAction> result = new List<GOAPAction>();
        Node n = cheapest;
        while (n != null)
        {
            if (n.action != null)
            {
                result.Insert(0, n.action);
            }
            n = n.parent;
        }

        Queue<GOAPAction> queue = new Queue<GOAPAction>();
        foreach (GOAPAction action in result)
        {
            queue.Enqueue(action);
        }

        return queue;
    }

    /// <summary>
    /// Construye el grafo de acciones recursivamente
    /// </summary>
    private bool BuildGraph(Node parent, List<Node> leaves, List<GOAPAction> usableActions, 
                            Dictionary<string, object> goal)
    {
        bool foundOne = false;

        foreach (GOAPAction action in usableActions)
        {
            if (InState(action.Preconditions, parent.state))
            {
                Dictionary<string, object> currentState = PopulateState(parent.state, action.Effects);
                Node node = new Node(parent, parent.runningCost + action.cost, currentState, action);

                if (InState(goal, currentState))
                {
                    // Encontramos un camino al objetivo
                    leaves.Add(node);
                    foundOne = true;
                }
                else
                {
                    // Continuar buscando
                    List<GOAPAction> subset = ActionSubset(usableActions, action);
                    bool found = BuildGraph(node, leaves, subset, goal);
                    if (found)
                    {
                        foundOne = true;
                    }
                }
            }
        }

        return foundOne;
    }

    /// <summary>
    /// Verifica si todas las condiciones están en el estado
    /// </summary>
    private bool InState(Dictionary<string, object> test, Dictionary<string, object> state)
    {
        bool allMatch = true;
        foreach (KeyValuePair<string, object> kvp in test)
        {
            if (!state.ContainsKey(kvp.Key) || !state[kvp.Key].Equals(kvp.Value))
            {
                allMatch = false;
                break;
            }
        }
        return allMatch;
    }

    /// <summary>
    /// Aplica los efectos de una acción al estado
    /// </summary>
    private Dictionary<string, object> PopulateState(Dictionary<string, object> currentState, 
                                                     Dictionary<string, object> stateChange)
    {
        Dictionary<string, object> state = new Dictionary<string, object>(currentState);
        foreach (KeyValuePair<string, object> kvp in stateChange)
        {
            state[kvp.Key] = kvp.Value;
        }
        return state;
    }

    /// <summary>
    /// Crea un subconjunto de acciones excluyendo la acción actual
    /// </summary>
    private List<GOAPAction> ActionSubset(List<GOAPAction> actions, GOAPAction removeMe)
    {
        List<GOAPAction> subset = new List<GOAPAction>();
        foreach (GOAPAction action in actions)
        {
            if (!action.Equals(removeMe))
            {
                subset.Add(action);
            }
        }
        return subset;
    }
}
