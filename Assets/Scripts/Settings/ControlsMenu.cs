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

        if (rebindText)
            rebindText.text = "PULSA UNA TECLA\n(ESC para cancelar)";
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

        inputHandler.OnBindingsChanged.AddListener(RefreshAllRows);

        // Ocultar overlays
        if (rebindPanel) rebindPanel.SetActive(false);
        if (conflictPanel) conflictPanel.SetActive(false);
    }

    private void OnDisable()
    {
        if (inputHandler != null)
            inputHandler.OnBindingsChanged.RemoveListener(RefreshAllRows);
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

    // ==================== REBIND UI ====================

    public void ShowRebindPanel()
    {
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
