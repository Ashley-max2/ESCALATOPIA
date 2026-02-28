using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Habilita la navegación de UI con mando (gamepad).
/// - Selecciona automaticamente el primer boton de cada panel
/// - Muestra un recuadro visual (Outline) en el boton seleccionado
/// - Detecta cambio de panel y re-selecciona el primer boton
/// - Funciona con el StandaloneInputModule de Unity (Submit=A, Cancel=B, Stick=navegar)
///
/// USO: Poner este script en el Canvas o en un GameObject vacio en la escena del menu.
///      Asignar los paneles y sus primeros botones en el Inspector.
/// </summary>
public class GamepadUINavigator : MonoBehaviour
{
    [System.Serializable]
    public class PanelConfig
    {
        [Tooltip("El panel (GameObject) que contiene los botones")]
        public GameObject panel;
        [Tooltip("El primer boton que se selecciona al abrir este panel")]
        public Selectable firstSelected;
    }

    [Header("=== PANELES ===")]
    [Tooltip("Configura cada panel con su primer boton seleccionado")]
    [SerializeField] private PanelConfig[] panels;

    [Header("=== VISUAL HIGHLIGHT ===")]
    [Tooltip("Color del recuadro de seleccion")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.2f, 1f); // Amarillo dorado
    [Tooltip("Grosor del recuadro")]
    [SerializeField] private Vector2 highlightSize = new Vector2(4f, 4f);

    // Runtime
    private GameObject _lastSelected;
    private Outline _currentOutline;

    private void Update()
    {
        // Detectar panel activo y auto-seleccionar si no hay nada seleccionado
        GameObject currentSelected = EventSystem.current?.currentSelectedGameObject;

        // Si no hay nada seleccionado o el seleccionado esta inactivo, seleccionar el primero del panel activo
        if (currentSelected == null || !currentSelected.activeInHierarchy)
        {
            SelectFirstButtonOfActivePanel();
        }

        // Actualizar visual highlight si cambio la seleccion
        if (currentSelected != _lastSelected)
        {
            UpdateHighlight(currentSelected);
            _lastSelected = currentSelected;
        }
    }

    /// <summary>
    /// Busca el panel activo y selecciona su primer boton configurado
    /// </summary>
    private void SelectFirstButtonOfActivePanel()
    {
        if (panels == null) return;

        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i].panel != null && panels[i].panel.activeInHierarchy && panels[i].firstSelected != null)
            {
                EventSystem.current?.SetSelectedGameObject(panels[i].firstSelected.gameObject);
                return;
            }
        }
    }

    /// <summary>
    /// Selecciona un boton especifico (para llamar desde otros scripts si es necesario)
    /// </summary>
    public void SelectButton(Selectable button)
    {
        if (button != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }

    /// <summary>
    /// Aplica/quita el recuadro visual de seleccion (Outline)
    /// </summary>
    private void UpdateHighlight(GameObject newSelected)
    {
        // Quitar outline del anterior
        if (_currentOutline != null)
        {
            Destroy(_currentOutline);
            _currentOutline = null;
        }

        // Poner outline en el nuevo
        if (newSelected != null)
        {
            // Solo poner outline si es un UI Selectable (boton, toggle, dropdown, etc.)
            Selectable selectable = newSelected.GetComponent<Selectable>();
            if (selectable != null)
            {
                _currentOutline = newSelected.AddComponent<Outline>();
                _currentOutline.effectColor = highlightColor;
                _currentOutline.effectDistance = highlightSize;
            }
        }
    }

    /// <summary>
    /// Para llamar cuando un panel se activa (compatible con UnityEvent en el Inspector)
    /// </summary>
    public void OnPanelChanged()
    {
        // Forzar re-seleccion en el siguiente frame
        _lastSelected = null;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}
