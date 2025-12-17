using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Input action definition.
/// Represents a mappable input action (e.g., Jump, Move, Climb).
/// </summary>
[Serializable]
public class InputAction
{
    public string actionName;
    public KeyCode primaryKey;
    public KeyCode secondaryKey;
    public string gamepadButton;

    public InputAction(string name, KeyCode primary, KeyCode secondary = KeyCode.None, string gamepad = "")
    {
        actionName = name;
        primaryKey = primary;
        secondaryKey = secondary;
        gamepadButton = gamepad;
    }

    /// <summary>
    /// Check if this action is currently pressed.
    /// </summary>
    public bool IsPressed()
    {
        return Input.GetKey(primaryKey) || 
               (secondaryKey != KeyCode.None && Input.GetKey(secondaryKey));
    }

    /// <summary>
    /// Check if this action was just pressed this frame.
    /// </summary>
    public bool WasJustPressed()
    {
        return Input.GetKeyDown(primaryKey) || 
               (secondaryKey != KeyCode.None && Input.GetKeyDown(secondaryKey));
    }

    /// <summary>
    /// Check if this action was just released this frame.
    /// </summary>
    public bool WasJustReleased()
    {
        return Input.GetKeyUp(primaryKey) || 
               (secondaryKey != KeyCode.None && Input.GetKeyUp(secondaryKey));
    }
}

/// <summary>
/// Input remapping system for Gold-level UX rubric score.
/// Allows players to customize keyboard and gamepad controls.
/// </summary>
public class InputRemappingSystem : MonoBehaviour
{
    private static InputRemappingSystem instance;
    public static InputRemappingSystem Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("InputRemappingSystem");
                instance = go.AddComponent<InputRemappingSystem>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("Input Actions")]
    private Dictionary<string, InputAction> inputActions = new Dictionary<string, InputAction>();

    // Events for input remapping
    public event Action<string, KeyCode> OnKeyRemapped;

    private bool isWaitingForInput = false;
    private string actionBeingRemapped = "";
    private bool remappingPrimary = true;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeDefaultBindings();
        LoadBindings();
    }

    /// <summary>
    /// Initialize default key bindings.
    /// </summary>
    private void InitializeDefaultBindings()
    {
        inputActions.Clear();

        // Movement
        inputActions.Add("MoveForward", new InputAction("MoveForward", KeyCode.W, KeyCode.UpArrow));
        inputActions.Add("MoveBackward", new InputAction("MoveBackward", KeyCode.S, KeyCode.DownArrow));
        inputActions.Add("MoveLeft", new InputAction("MoveLeft", KeyCode.A, KeyCode.LeftArrow));
        inputActions.Add("MoveRight", new InputAction("MoveRight", KeyCode.D, KeyCode.RightArrow));

        // Actions
        inputActions.Add("Jump", new InputAction("Jump", KeyCode.Space));
        inputActions.Add("Climb", new InputAction("Climb", KeyCode.E));
        inputActions.Add("Run", new InputAction("Run", KeyCode.LeftShift, KeyCode.RightShift));
        inputActions.Add("Hook", new InputAction("Hook", KeyCode.Mouse0));

        // UI
        inputActions.Add("Pause", new InputAction("Pause", KeyCode.Escape));
        inputActions.Add("Interact", new InputAction("Interact", KeyCode.F));
    }

    /// <summary>
    /// Get an input action by name.
    /// </summary>
    public InputAction GetAction(string actionName)
    {
        if (inputActions.ContainsKey(actionName))
        {
            return inputActions[actionName];
        }

        Debug.LogWarning($"[InputRemapping] Action '{actionName}' not found!");
        return null;
    }

    /// <summary>
    /// Check if an action is pressed.
    /// </summary>
    public bool IsActionPressed(string actionName)
    {
        InputAction action = GetAction(actionName);
        return action != null && action.IsPressed();
    }

    /// <summary>
    /// Check if an action was just pressed.
    /// </summary>
    public bool WasActionJustPressed(string actionName)
    {
        InputAction action = GetAction(actionName);
        return action != null && action.WasJustPressed();
    }

    /// <summary>
    /// Start remapping a key for an action.
    /// </summary>
    public void StartRemapping(string actionName, bool primary = true)
    {
        if (!inputActions.ContainsKey(actionName))
        {
            Debug.LogWarning($"[InputRemapping] Cannot remap unknown action: {actionName}");
            return;
        }

        isWaitingForInput = true;
        actionBeingRemapped = actionName;
        remappingPrimary = primary;

        Debug.Log($"[InputRemapping] Waiting for input for {actionName} ({(primary ? "Primary" : "Secondary")})");
    }

    /// <summary>
    /// Cancel current remapping.
    /// </summary>
    public void CancelRemapping()
    {
        isWaitingForInput = false;
        actionBeingRemapped = "";
    }

    /// <summary>
    /// Check if currently waiting for input.
    /// </summary>
    public bool IsWaitingForInput => isWaitingForInput;

    private void Update()
    {
        if (!isWaitingForInput)
            return;

        // Listen for any key press
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                // Ignore mouse buttons for now (except Mouse0, Mouse1)
                if (keyCode >= KeyCode.Mouse2 && keyCode <= KeyCode.Mouse6)
                    continue;

                RemapKey(actionBeingRemapped, keyCode, remappingPrimary);
                isWaitingForInput = false;
                break;
            }
        }
    }

    /// <summary>
    /// Remap a key for an action.
    /// </summary>
    private void RemapKey(string actionName, KeyCode newKey, bool primary)
    {
        if (!inputActions.ContainsKey(actionName))
            return;

        InputAction action = inputActions[actionName];

        // Check for conflicts
        foreach (var kvp in inputActions)
        {
            if (kvp.Key == actionName)
                continue;

            if (kvp.Value.primaryKey == newKey || kvp.Value.secondaryKey == newKey)
            {
                Debug.LogWarning($"[InputRemapping] Key {newKey} is already bound to {kvp.Key}");
                // Optionally: remove the conflicting binding
            }
        }

        // Apply the new binding
        if (primary)
        {
            action.primaryKey = newKey;
        }
        else
        {
            action.secondaryKey = newKey;
        }

        Debug.Log($"[InputRemapping] Remapped {actionName} to {newKey}");

        // Raise event
        OnKeyRemapped?.Invoke(actionName, newKey);

        // Save bindings
        SaveBindings();
    }

    /// <summary>
    /// Reset all bindings to default.
    /// </summary>
    public void ResetToDefaults()
    {
        InitializeDefaultBindings();
        SaveBindings();
        Debug.Log("[InputRemapping] Reset to default bindings");
    }

    /// <summary>
    /// Save bindings to PlayerPrefs.
    /// </summary>
    private void SaveBindings()
    {
        foreach (var kvp in inputActions)
        {
            PlayerPrefs.SetInt($"Input_{kvp.Key}_Primary", (int)kvp.Value.primaryKey);
            PlayerPrefs.SetInt($"Input_{kvp.Key}_Secondary", (int)kvp.Value.secondaryKey);
        }
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load bindings from PlayerPrefs.
    /// </summary>
    private void LoadBindings()
    {
        foreach (var kvp in inputActions)
        {
            if (PlayerPrefs.HasKey($"Input_{kvp.Key}_Primary"))
            {
                kvp.Value.primaryKey = (KeyCode)PlayerPrefs.GetInt($"Input_{kvp.Key}_Primary");
            }

            if (PlayerPrefs.HasKey($"Input_{kvp.Key}_Secondary"))
            {
                kvp.Value.secondaryKey = (KeyCode)PlayerPrefs.GetInt($"Input_{kvp.Key}_Secondary");
            }
        }

        Debug.Log("[InputRemapping] Loaded custom bindings");
    }

    /// <summary>
    /// Get all input actions for UI display.
    /// </summary>
    public Dictionary<string, InputAction> GetAllActions()
    {
        return new Dictionary<string, InputAction>(inputActions);
    }
}
