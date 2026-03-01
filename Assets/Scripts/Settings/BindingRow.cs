using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BindingRow : MonoBehaviour
{
    [SerializeField] private string actionName = "Adelante"; // ¡Pon aquí el nombre de la acción MANUAL!
    [SerializeField] private Button keyButton;
    [SerializeField] private TMP_Text keyText;

    private PlayerInputHandler inputHandler;
    private static ControlsMenu controlsMenu;

    void Awake()
    {
        inputHandler = FindObjectOfType<PlayerInputHandler>();
        controlsMenu = FindObjectOfType<ControlsMenu>();
        if (keyButton == null) keyButton = GetComponentInChildren<Button>();
        if (keyText == null) keyText = keyButton.GetComponentInChildren<TMP_Text>();
    }

    void OnEnable()
    {
        if (inputHandler != null)
            inputHandler.OnBindingsChanged.AddListener(UpdateText);
        UpdateText();
    }

    void OnDisable()
    {
        if (inputHandler != null)
            inputHandler.OnBindingsChanged.RemoveListener(UpdateText);
    }

    void Start()
    {
        keyButton.onClick.AddListener(Rebind);
    }

    void UpdateText()
    {
        keyText.text = inputHandler.GetDisplayName(actionName);
    }

    void Rebind()
    {
        if (inputHandler.IsRebinding) return;
        inputHandler.StartRebind(actionName);
        controlsMenu.ShowRebindPanel();
    }
}