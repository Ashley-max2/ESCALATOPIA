using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// UI for input remapping.
/// Displays current bindings and allows remapping.
/// </summary>
public class InputRemappingUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject bindingItemPrefab;
    [SerializeField] private Transform bindingsContainer;
    [SerializeField] private GameObject waitingForInputPanel;
    [SerializeField] private Text waitingForInputText;
    [SerializeField] private Button resetButton;

    private Dictionary<string, GameObject> bindingItems = new Dictionary<string, GameObject>();

    private void Start()
    {
        PopulateBindings();

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetClicked);
        }

        // Subscribe to remapping events
        InputRemappingSystem.Instance.OnKeyRemapped += OnKeyRemapped;
    }

    private void OnDestroy()
    {
        if (InputRemappingSystem.Instance != null)
        {
            InputRemappingSystem.Instance.OnKeyRemapped -= OnKeyRemapped;
        }
    }

    private void Update()
    {
        // Update waiting for input panel
        if (waitingForInputPanel != null)
        {
            waitingForInputPanel.SetActive(InputRemappingSystem.Instance.IsWaitingForInput);
        }

        // Cancel remapping with Escape
        if (InputRemappingSystem.Instance.IsWaitingForInput && Input.GetKeyDown(KeyCode.Escape))
        {
            InputRemappingSystem.Instance.CancelRemapping();
        }
    }

    /// <summary>
    /// Populate the bindings list.
    /// </summary>
    private void PopulateBindings()
    {
        if (bindingsContainer == null || bindingItemPrefab == null)
        {
            Debug.LogWarning("[InputRemappingUI] Missing references!");
            return;
        }

        // Clear existing items
        foreach (Transform child in bindingsContainer)
        {
            Destroy(child.gameObject);
        }
        bindingItems.Clear();

        // Create binding items
        var actions = InputRemappingSystem.Instance.GetAllActions();
        foreach (var kvp in actions)
        {
            CreateBindingItem(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Create a binding item in the UI.
    /// </summary>
    private void CreateBindingItem(string actionName, InputAction action)
    {
        GameObject item = Instantiate(bindingItemPrefab, bindingsContainer);
        bindingItems.Add(actionName, item);

        // Set action name
        Text nameText = item.transform.Find("ActionName")?.GetComponent<Text>();
        if (nameText != null)
        {
            nameText.text = FormatActionName(actionName);
        }

        // Set primary key button
        Button primaryButton = item.transform.Find("PrimaryKeyButton")?.GetComponent<Button>();
        if (primaryButton != null)
        {
            Text buttonText = primaryButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = action.primaryKey.ToString();
            }

            primaryButton.onClick.AddListener(() => OnRemapClicked(actionName, true));
        }

        // Set secondary key button
        Button secondaryButton = item.transform.Find("SecondaryKeyButton")?.GetComponent<Button>();
        if (secondaryButton != null)
        {
            Text buttonText = secondaryButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = action.secondaryKey != KeyCode.None ? action.secondaryKey.ToString() : "None";
            }

            secondaryButton.onClick.AddListener(() => OnRemapClicked(actionName, false));
        }
    }

    /// <summary>
    /// Handle remap button click.
    /// </summary>
    private void OnRemapClicked(string actionName, bool primary)
    {
        InputRemappingSystem.Instance.StartRemapping(actionName, primary);

        if (waitingForInputText != null)
        {
            waitingForInputText.text = $"Press a key for {FormatActionName(actionName)}\n(ESC to cancel)";
        }
    }

    /// <summary>
    /// Handle key remapped event.
    /// </summary>
    private void OnKeyRemapped(string actionName, KeyCode newKey)
    {
        // Refresh the UI
        PopulateBindings();
    }

    /// <summary>
    /// Handle reset button click.
    /// </summary>
    private void OnResetClicked()
    {
        InputRemappingSystem.Instance.ResetToDefaults();
        PopulateBindings();
    }

    /// <summary>
    /// Format action name for display.
    /// </summary>
    private string FormatActionName(string actionName)
    {
        // Convert camelCase to Title Case with spaces
        string result = "";
        for (int i = 0; i < actionName.Length; i++)
        {
            if (i > 0 && char.IsUpper(actionName[i]))
            {
                result += " ";
            }
            result += actionName[i];
        }
        return result;
    }
}
