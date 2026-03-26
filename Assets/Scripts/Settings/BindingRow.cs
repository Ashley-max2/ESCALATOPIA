using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BindingRow : MonoBehaviour
{
    [SerializeField] private string actionName = "Adelante"; // �Pon aqu� el nombre de la acci�n MANUAL!
    [SerializeField] private Button keyButton;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private bool allowKeyboardSubmit = false;

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
        if (!IsValidRebindTrigger()) return;

        inputHandler.StartRebind(actionName);
        if (controlsMenu != null)
            controlsMenu.ShowRebindPanel();
    }

    bool IsValidRebindTrigger()
    {
        if (inputHandler == null)
            return false;

        bool keyboardScheme = inputHandler.CurrentInputScheme == PlayerInputHandler.InputScheme.KeyboardMouse;
        bool gamepadScheme = inputHandler.CurrentInputScheme == PlayerInputHandler.InputScheme.Gamepad;

        // Click de raton siempre permitido.
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
            return keyboardScheme;

        // Enter/Espacio permitidos para abrir remapeo desde teclado.
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            return keyboardScheme;

        // Confirmacion por mando (A / Cross) permitida cuando el esquema activo es mando.
        if (gamepadScheme)
        {
            if (Input.GetKeyDown(KeyCode.JoystickButton0))
                return true;
        }

        // Submit por teclado (espacio/enter) desactivado por defecto para evitar remapeos accidentales.
        return allowKeyboardSubmit;
    }
}