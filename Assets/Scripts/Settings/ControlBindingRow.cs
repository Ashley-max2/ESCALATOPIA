using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Componente para cada fila del panel de controles.
/// Se coloca manualmente en la escena (sin prefab ni ScrollView).
/// Muestra la tecla actual en el boton y permite rebindear al pulsarlo.
/// </summary>
public class ControlBindingRow : MonoBehaviour
{
    [Header("Nombre interno de la accion (debe coincidir con PlayerInputHandler)")]
    [Tooltip("Valores validos: Adelante, Atras, Izquierda, Derecha, Saltar, Correr, Gancho, LiberarGancho")]
    [SerializeField] private string actionName;

    [Header("Referencias UI")]
    [SerializeField] private TMP_Text keyLabel;       // Texto dentro del boton que muestra la tecla
    [SerializeField] private Button rebindButton;     // Boton que el jugador pulsa para rebindear

    private ControlsMenu menu;
    private PlayerInputHandler inputHandler;
    private bool initialized;

    public string ActionName => actionName;

    /// <summary>
    /// Llamado por ControlsMenu al inicializar.
    /// </summary>
    public void Init(ControlsMenu menu, PlayerInputHandler inputHandler)
    {
        this.menu = menu;
        this.inputHandler = inputHandler;

        if (!initialized)
        {
            rebindButton.onClick.AddListener(OnRebindClicked);
            initialized = true;
        }

        RefreshKeyDisplay();
    }

    /// <summary>
    /// Actualiza el texto del boton con la tecla actual asignada.
    /// </summary>
    public void RefreshKeyDisplay()
    {
        if (keyLabel != null && inputHandler != null)
        {
            keyLabel.text = inputHandler.GetDisplayName(actionName);
        }
    }

    private void OnRebindClicked()
    {
        if (inputHandler == null || inputHandler.IsRebinding) return;

        inputHandler.StartRebind(actionName);
        if (menu != null) menu.ShowRebindPanel();
    }
}
