using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Panel de controles: gestiona los ControlBindingRow y el rebind de teclas.
/// No usa ScrollView ni prefabs — las filas se colocan manualmente en Unity.
/// </summary>
public class ControlsMenu : MonoBehaviour
{
    public static ControlsMenu Instance;
    public bool IsRebindPanelVisible => rebindPanel != null && rebindPanel.activeInHierarchy;
    public bool IsConflictPanelVisible => conflictPanel != null && conflictPanel.activeInHierarchy;

    [Header("Filas de Controles")]
    [Tooltip("Arrastra aqui el panel donde estan los botones de rebind (ControlsPanel).\nSi se deja vacio, busca las filas como hijos de este objeto.")]
    [SerializeField] private Transform rowsParent;

    [Header("Overlay de Rebind")]
    [SerializeField] private GameObject rebindPanel;
    [SerializeField] private TMP_Text rebindText;

    [Header("Aviso de Conflicto")]
    [SerializeField] private GameObject conflictPanel;
    [SerializeField] private TMP_Text conflictText;

    [Header("Botones")]
    [SerializeField] private Button resetButton;
    [SerializeField] private Button backButton;

    private PlayerInputHandler inputHandler;
    private ControlBindingRow[] rows;
    private bool initialized;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (resetButton)
            resetButton.onClick.AddListener(OnResetClicked);

        if (backButton)
            backButton.onClick.AddListener(OnBackClicked);

        UpdateRebindHint();
    }

    private void OnEnable()
    {
        // Buscar PlayerInputHandler en toda la escena
        if (inputHandler == null)
            inputHandler = FindObjectOfType<PlayerInputHandler>();

        if (inputHandler == null)
        {
            Debug.LogError("ControlsMenu: No hay PlayerInputHandler en la escena. Anade un GameObject con PlayerInputHandler.");
            return;
        }

        InitRows();

        inputHandler.OnBindingsChanged.AddListener(RefreshControlsUI);
        RefreshControlsUI();

        // Ocultar overlays
        if (rebindPanel) rebindPanel.SetActive(false);
        if (conflictPanel) conflictPanel.SetActive(false);
    }

    private void OnDisable()
    {
        if (inputHandler != null)
            inputHandler.OnBindingsChanged.RemoveListener(RefreshControlsUI);
    }

    // ==================== INIT ROWS ====================

    private void InitRows()
    {
        // Buscar filas en el parent indicado, o en toda la escena si no hay parent
        if (rowsParent != null)
            rows = rowsParent.GetComponentsInChildren<ControlBindingRow>(true);
        else
            rows = FindObjectsOfType<ControlBindingRow>();

        if (rows.Length == 0)
        {
            Debug.LogWarning("ControlsMenu: No se encontraron ControlBindingRow. Asegurate de que estan en la escena y tienen el componente.");
            return;
        }

        foreach (var row in rows)
        {
            row.Init(this, inputHandler);
        }

        // Configurar navegación explícita entre filas para evitar saltos inesperados
        // Usamos el orden devuelto por GetComponentsInChildren (o FindObjectsOfType) como orden visual.
        for (int i = 0; i < rows.Length; i++)
        {
            Selectable current = rows[i].RebindSelectable;
            if (current == null) continue;

            Navigation nav = new Navigation { mode = Navigation.Mode.Explicit };

            // Arriba -> Volver (backButton) para la primera fila, o la fila anterior
            if (i == 0)
            {
                if (backButton != null) nav.selectOnUp = backButton;
            }
            else
            {
                nav.selectOnUp = rows[i - 1].RebindSelectable;
            }

            // Abajo -> siguiente fila
            if (i < rows.Length - 1)
                nav.selectOnDown = rows[i + 1].RebindSelectable;

            current.navigation = nav;
        }

        // Asegurar que el boton Volver baje a la primera fila
        if (backButton != null && rows.Length > 0)
        {
            Navigation backNav = backButton.navigation;
            backNav.mode = Navigation.Mode.Explicit;
            backNav.selectOnDown = rows[0].RebindSelectable;
            backButton.navigation = backNav;
        }

        initialized = true;
    }

    private void RefreshAllRows()
    {
        if (rows == null) return;
        foreach (var row in rows)
        {
            if (row != null) row.RefreshKeyDisplay();
        }
    }

    private void RefreshControlsUI()
    {
        RefreshAllRows();
        UpdateRebindHint();
    }

    private void UpdateRebindHint()
    {
        if (rebindText == null)
            return;

        if (inputHandler == null)
        {
            rebindText.text = "PULSA UNA TECLA\n(ESC para cancelar)";
            return;
        }

        if (inputHandler.CurrentInputScheme == PlayerInputHandler.InputScheme.KeyboardMouse)
        {
            rebindText.text = "PULSA UNA TECLA\n(ESC para cancelar)";
            return;
        }

        switch (inputHandler.DetectedGamepad)
        {
            case PlayerInputHandler.GamepadType.Xbox:
                rebindText.text = "PULSA UN BOTON DE MANDO\n(START para cancelar)";
                break;

            case PlayerInputHandler.GamepadType.PlayStation:
                rebindText.text = "PULSA UN BOTON DE MANDO\n(OPTIONS para cancelar)";
                break;

            default:
                rebindText.text = "PULSA UN BOTON DE MANDO\n(BOTON MENU para cancelar)";
                break;
        }
    }

    // ==================== REBIND UI ====================

    public void ShowRebindPanel()
    {
        UpdateRebindHint();
        if (rebindPanel) rebindPanel.SetActive(true);
        if (conflictPanel) conflictPanel.SetActive(false);
    }

    public void HideRebindPanel()
    {
        if (rebindPanel) rebindPanel.SetActive(false);
        if (conflictPanel) conflictPanel.SetActive(false);
    }

    public void ShowConflict(string otherAction)
    {
        if (conflictText)
            conflictText.text = $"Tecla ya en uso por\n\"{otherAction}\"";
        if (conflictPanel) conflictPanel.SetActive(true);

        StartCoroutine(HideConflictAfter(2f));
    }

    private IEnumerator HideConflictAfter(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (conflictPanel) conflictPanel.SetActive(false);
    }

    // ==================== BUTTONS ====================

    private void OnResetClicked()
    {
        MusicManager.PlayButton();
        if (inputHandler != null)
            inputHandler.ResetDefaults();
    }

    private void OnBackClicked()
    {
        MusicManager.PlayButton();
        if (inputHandler != null && inputHandler.IsRebinding)
        {
            inputHandler.EndRebind();
            HideRebindPanel();
        }

        // Intentar volver al config del Main Menu
        var mainMenu = FindObjectOfType<MainMenu>();
        if (mainMenu != null)
        {
            mainMenu.ReturnToConfig();
            return;
        }

        // Intentar volver al config del Pause Menu
        var pauseMenu = FindObjectOfType<PauseMenuManager>();
        if (pauseMenu != null)
        {
            pauseMenu.ReturnToConfig();
            return;
        }
    }
}
