using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

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

    [System.Serializable]
    public class SelectableVisualOverride
    {
        [Tooltip("Selectable al que aplica este override")]
        public Selectable selectable;
        [Tooltip("RectTransform donde se dibuja el sprite de seleccion (contenedor completo)")]
        public RectTransform highlightRectTarget;
        [Tooltip("TMP_Text exacto que se colorea al seleccionar (ejemplo: 'Adelante:')")]
        public TMP_Text textToColor;
        [Tooltip("Segundo TMP_Text opcional para colorear al seleccionar")]
        public TMP_Text textToColor2;
        [Tooltip("Text legacy exacto que se colorea al seleccionar")]
        public Text legacyTextToColor;
    }

    [System.Serializable]
    public class PermanentTextColorOverride
    {
        [Tooltip("TMP_Text al que se aplica color permanente")]
        public TMP_Text tmpText;
        [Tooltip("Text legacy al que se aplica color permanente")]
        public Text legacyText;
        [Tooltip("Color permanente para este texto")]
        public Color color = Color.white;
    }

    [Header("=== PANELES ===")]
    [Tooltip("Configura cada panel con su primer boton seleccionado")]
    [SerializeField] private PanelConfig[] panels;

    [Header("=== OVERRIDES VISUALES (OPCIONAL) ===")]
    [Tooltip("Configura por Selectable: contenedor del marco y texto exacto a colorear")]
    [SerializeField] private SelectableVisualOverride[] visualOverrides;

    [Header("=== CONTROLES: OVERRIDES VISUALES ===")]
    [Tooltip("Overrides solo para el panel de Controles. Se aplican antes que los generales")]
    [SerializeField] private SelectableVisualOverride[] controlsVisualOverrides;

    [Header("=== OVERRIDES TEXTO PERMANENTE ===")]
    [Tooltip("Color fijo para textos concretos, independiente de la seleccion")]
    [SerializeField] private PermanentTextColorOverride[] permanentTextColorOverrides;

    [Header("=== VISUAL HIGHLIGHT ===")]
    [Tooltip("Color del recuadro de seleccion")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.2f, 1f); // Amarillo dorado
    [Tooltip("Grosor del recuadro")]
    [SerializeField] private Vector2 highlightSize = new Vector2(4f, 4f);
    [Tooltip("Si esta activo, crea Outline en targets que no lo tengan (puede aumentar componentes en tiempo de ejecucion)")]
    [SerializeField] private bool autoAddOutlineOnTarget = false;
    [Tooltip("Frames que esperamos antes de quitar highlight cuando EventSystem devuelve null de forma temporal")]
    [SerializeField] private int nullSelectionGraceFrames = 6;

    [Header("=== INPUT SOURCE ===")]
    [Tooltip("Si esta activo, mover el mouse sobre un boton cambiara la seleccion UI")]
    [SerializeField] private bool syncMouseHoverSelection = true;

    [Header("=== BOTON SELECCIONADO ===")]
    [Tooltip("Sprite del recuadro que se mostrara SOLO en el boton seleccionado")]
    [SerializeField] private Sprite selectedButtonSprite;
    [Tooltip("Si esta activo, muestra selectedButtonSprite como overlay de seleccion")]
    [SerializeField] private bool useSelectedSprite = true;
    [Tooltip("Padding del recuadro (X=horizontal, Y=vertical)")]
    [SerializeField] private Vector2 selectedFramePadding = new Vector2(10f, 6f);
    [Tooltip("Evita dibujar recuadro/outline si el rect objetivo es demasiado grande")]
    [SerializeField] private bool skipHugeHighlightRects = true;
    [Tooltip("Ancho maximo del rect objetivo respecto a pantalla")]
    [SerializeField, Range(0.1f, 1f)] private float maxHighlightScreenWidthRatio = 0.65f;
    [Tooltip("Alto maximo del rect objetivo respecto a pantalla")]
    [SerializeField, Range(0.1f, 1f)] private float maxHighlightScreenHeightRatio = 0.5f;
    [Tooltip("Si esta activo, cambia el color del texto seleccionado")]
    [SerializeField] private bool applySelectedTextColor = true;
    [Tooltip("Color del texto del boton seleccionado")]
    [SerializeField] private Color selectedTextColor = new Color(1f, 0.95f, 0.55f, 1f);

    [Header("=== SLIDER SELECCIONADO ===")]
    [Tooltip("Si esta activo, colorea el Fill del slider seleccionado")]
    [SerializeField] private bool highlightSliderFill = true;
    [Tooltip("Color del Fill cuando el slider esta seleccionado")]
    [SerializeField] private Color selectedSliderFillColor = new Color(1f, 0.95f, 0.55f, 1f);
    [Tooltip("Padding extra del recuadro cuando el seleccionado es un Slider (X=horizontal, Y=vertical)")]
    [SerializeField] private Vector2 sliderFrameExtraPadding = new Vector2(6f, 3f);

    [Header("=== TOGGLE SELECCIONADO ===")]
    [Tooltip("Si esta activo, el toggle usa un highlight mas amplio")]
    [SerializeField] private bool highlightToggleWholeButton = true;
    [Tooltip("Padding extra del recuadro cuando el seleccionado es un Toggle (X=horizontal, Y=vertical)")]
    [SerializeField] private Vector2 toggleFrameExtraPadding = new Vector2(14f, 8f);

    // Runtime
    private GameObject _lastSelected;
    private Outline _currentOutline;
    private Selectable _currentSelectable;
    private Vector3 _lastMousePosition;
    private bool _mousePositionInitialized;
    private int _nullSelectionFrames;
    private PointerEventData _pointerEventData;
    private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>(16);
    private readonly Vector3[] _rectCorners = new Vector3[4];
    private bool _selectionSuspendedByRebindOverlay;

    // Recuadro visual reutilizable para el boton seleccionado
    private GameObject _selectionFrameObject;
    private RectTransform _selectionFrameRect;
    private Image _selectionFrameImage;

    // Estado visual temporal del texto del boton actualmente seleccionado
    private Text _styledLegacyText;
    private string _originalLegacyText;
    private Color _originalLegacyColor;
    private TMP_Text _styledTmpText;
    private string _originalTmpText;
    private Color _originalTmpColor;
    private TMP_Text _styledTmpText2;
    private string _originalTmpText2;
    private Color _originalTmpColor2;
    private Image _styledSliderFillImage;
    private Color _originalSliderFillColor;

    private void Update()
    {
        if (IsRebindOverlayBlockingUI())
        {
            SuspendSelectionVisualsForRebind();
            return;
        }

        if (_selectionSuspendedByRebindOverlay)
            _selectionSuspendedByRebindOverlay = false;

        ApplyPermanentTextColorOverrides();

        if (syncMouseHoverSelection)
            SelectHoveredButtonWithMouse();

        // Detectar panel activo y auto-seleccionar si no hay nada seleccionado
        GameObject currentSelected = EventSystem.current?.currentSelectedGameObject;

        // Si no hay nada seleccionado o el seleccionado esta inactivo, seleccionar el primero del panel activo
        if (currentSelected == null || !currentSelected.activeInHierarchy)
        {
            SelectFirstButtonOfActivePanel();
            currentSelected = EventSystem.current?.currentSelectedGameObject;
        }

        if (currentSelected == null && _lastSelected != null)
        {
            _nullSelectionFrames++;
            if (_nullSelectionFrames < Mathf.Max(1, nullSelectionGraceFrames))
                currentSelected = _lastSelected;
        }
        else
        {
            _nullSelectionFrames = 0;
        }

        // Actualizar visual highlight si cambio la seleccion
        if (currentSelected != _lastSelected)
        {
            UpdateHighlight(currentSelected);
            _lastSelected = currentSelected;
        }
    }

    private bool IsRebindOverlayBlockingUI()
    {
        return ControlsMenu.Instance != null && ControlsMenu.Instance.IsRebindPanelVisible;
    }

    private void SuspendSelectionVisualsForRebind()
    {
        if (_currentOutline != null)
            _currentOutline.enabled = false;

        if (_selectionFrameObject != null)
            _selectionFrameObject.SetActive(false);

        if (_selectionSuspendedByRebindOverlay)
            return;

        RestoreSelectedButtonVisuals();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        _currentSelectable = null;
        _lastSelected = null;
        _nullSelectionFrames = 0;
        _selectionSuspendedByRebindOverlay = true;
    }

    private void SelectHoveredButtonWithMouse()
    {
        if (EventSystem.current == null) return;

        Vector3 currentMousePosition = Input.mousePosition;
        if (!_mousePositionInitialized)
        {
            _lastMousePosition = currentMousePosition;
            _mousePositionInitialized = true;
            return;
        }

        if (currentMousePosition == _lastMousePosition)
            return;

        _lastMousePosition = currentMousePosition;

        if (_pointerEventData == null)
            _pointerEventData = new PointerEventData(EventSystem.current);

        _pointerEventData.Reset();
        _pointerEventData.position = currentMousePosition;

        _raycastResults.Clear();
        EventSystem.current.RaycastAll(_pointerEventData, _raycastResults);

        for (int i = 0; i < _raycastResults.Count; i++)
        {
            GameObject hoveredObject = _raycastResults[i].gameObject;
            if (hoveredObject == null) continue;

            Selectable hoveredSelectable = hoveredObject.GetComponentInParent<Selectable>();
            PanelConfig panelConfig = GetPanelConfigForSelectable(hoveredSelectable);
            if (hoveredSelectable != null && IsMouseHoverSelectableValid(hoveredSelectable, panelConfig, currentMousePosition))
            {
                if (EventSystem.current.currentSelectedGameObject != hoveredSelectable.gameObject)
                    EventSystem.current.SetSelectedGameObject(hoveredSelectable.gameObject);

                return;
            }
        }
    }

    private bool IsMouseHoverSelectableValid(Selectable selectable, PanelConfig panelConfig, Vector2 mousePosition)
    {
        if (selectable == null)
            return false;

        if (!selectable.IsInteractable() || !selectable.gameObject.activeInHierarchy)
            return false;

        RectTransform hoverRect = GetFrameTargetRect(selectable, panelConfig);
        if (hoverRect == null)
            return false;

        // Evita seleccionar contenedores enormes por error: solo selecciona si el puntero cae en el rect exacto del control.
        if (!RectTransformUtility.RectangleContainsScreenPoint(hoverRect, mousePosition, null))
            return false;

        return true;
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

    private PanelConfig GetPanelConfigForSelectable(Selectable selectable)
    {
        if (selectable == null || panels == null)
            return null;

        Transform selectableTransform = selectable.transform;
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i].panel == null || !panels[i].panel.activeInHierarchy)
                continue;

            if (selectableTransform.IsChildOf(panels[i].panel.transform))
                return panels[i];
        }

        return null;
    }

    private SelectableVisualOverride GetVisualOverride(Selectable selectable)
    {
        if (selectable == null)
            return null;

        SelectableVisualOverride controlsOverride = FindVisualOverride(selectable, controlsVisualOverrides);
        if (controlsOverride != null)
            return controlsOverride;

        return FindVisualOverride(selectable, visualOverrides);
    }

    private SelectableVisualOverride FindVisualOverride(Selectable selectable, SelectableVisualOverride[] source)
    {
        if (source == null)
            return null;

        for (int i = 0; i < source.Length; i++)
        {
            if (source[i] != null && source[i].selectable == selectable)
                return source[i];
        }

        return null;
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
        // Quitar outline del anterior sin destruir componente (evita parpadeo)
        if (_currentOutline != null)
            _currentOutline.enabled = false;

        RestoreSelectedButtonVisuals();

        _currentSelectable = null;

        // Poner outline en el nuevo
        if (newSelected != null)
        {
            // Solo poner outline si es un UI Selectable (boton, toggle, dropdown, etc.)
            Selectable selectable = newSelected.GetComponent<Selectable>();
            if (selectable != null)
            {
                _currentSelectable = selectable;
                PanelConfig panelConfig = GetPanelConfigForSelectable(selectable);
                GameObject highlightTarget = GetHighlightTargetObject(selectable, panelConfig);
                RectTransform highlightRect = GetFrameTargetRect(selectable, panelConfig);
                bool allowHighlightVisuals = !IsHighlightRectTooLarge(highlightRect);

                if (allowHighlightVisuals && highlightTarget != null)
                {
                    _currentOutline = highlightTarget.GetComponent<Outline>();
                    if (_currentOutline == null && autoAddOutlineOnTarget)
                        _currentOutline = highlightTarget.AddComponent<Outline>();

                    if (_currentOutline != null)
                    {
                        _currentOutline.enabled = true;
                        _currentOutline.effectColor = highlightColor;
                        _currentOutline.effectDistance = highlightSize;
                    }
                }

                ApplySelectedButtonVisuals(selectable, panelConfig);
            }
        }
    }

    private void ApplySelectedButtonVisuals(Selectable selectable, PanelConfig panelConfig)
    {
        SelectableVisualOverride visualOverride = GetVisualOverride(selectable);

        // Mostrar sprite si está activado
        if (useSelectedSprite && selectedButtonSprite != null)
        {
            EnsureSelectionFrameExists();
            RectTransform targetRect = GetOverrideRectTarget(visualOverride)
                ?? (selectable.transform as RectTransform);

            if (targetRect != null && !IsHighlightRectTooLarge(targetRect))
            {
                _selectionFrameObject.transform.SetParent(targetRect, false);
                _selectionFrameObject.transform.SetAsFirstSibling();
                _selectionFrameRect.anchorMin = Vector2.zero;
                _selectionFrameRect.anchorMax = Vector2.one;

                Vector2 appliedPadding = selectedFramePadding;
                _selectionFrameRect.offsetMin = new Vector2(-appliedPadding.x, -appliedPadding.y);
                _selectionFrameRect.offsetMax = new Vector2(appliedPadding.x, appliedPadding.y);
                _selectionFrameImage.sprite = selectedButtonSprite;
                _selectionFrameImage.type = Image.Type.Sliced;
                _selectionFrameImage.raycastTarget = false;
                _selectionFrameObject.SetActive(true);
            }
            else
            {
                _selectionFrameObject.SetActive(false);
            }
        }

        // Colorear texto si está activado
        if (applySelectedTextColor)
        {
            TMP_Text tmpText = visualOverride != null && visualOverride.textToColor != null
                ? visualOverride.textToColor
                : selectable.GetComponentInChildren<TMP_Text>(true);
            TMP_Text tmpText2 = visualOverride != null ? visualOverride.textToColor2 : null;
            Text legacyText = visualOverride != null && visualOverride.legacyTextToColor != null
                ? visualOverride.legacyTextToColor
                : selectable.GetComponentInChildren<Text>(true);

            if (tmpText != null)
            {
                _styledTmpText = tmpText;
                _originalTmpText = tmpText.text;
                _originalTmpColor = tmpText.color;
                _styledTmpText.color = selectedTextColor;
            }

            if (tmpText2 != null && tmpText2 != tmpText)
            {
                _styledTmpText2 = tmpText2;
                _originalTmpText2 = tmpText2.text;
                _originalTmpColor2 = tmpText2.color;
                _styledTmpText2.color = selectedTextColor;
            }

            if (legacyText != null)
            {
                _styledLegacyText = legacyText;
                _originalLegacyText = legacyText.text;
                _originalLegacyColor = legacyText.color;
                _styledLegacyText.color = selectedTextColor;
            }
        }

        // Colorear Fill si es slider
        if (highlightSliderFill && selectable is Slider slider)
        {
            Image fillImage = GetSliderFillImage(slider);
            if (fillImage != null)
            {
                _styledSliderFillImage = fillImage;
                _originalSliderFillColor = fillImage.color;
                _styledSliderFillImage.color = selectedSliderFillColor;
            }
        }
    }

    private void EnsureSelectionFrameExists()
    {
        if (_selectionFrameObject != null)
            return;

        _selectionFrameObject = new GameObject("__SelectionFrame", typeof(RectTransform), typeof(Image));
        _selectionFrameRect = _selectionFrameObject.GetComponent<RectTransform>();
        _selectionFrameImage = _selectionFrameObject.GetComponent<Image>();
        _selectionFrameObject.SetActive(false);
    }

    private GameObject GetHighlightTargetObject(Selectable selectable, PanelConfig panelConfig)
    {
        SelectableVisualOverride visualOverride = GetVisualOverride(selectable);
        GameObject overrideObject = GetOverrideObjectTarget(visualOverride);
        if (overrideObject != null)
            return overrideObject;

        if (selectable is TMP_Dropdown tmpDropdown)
        {
            if (tmpDropdown.targetGraphic != null)
                return tmpDropdown.targetGraphic.gameObject;
            if (tmpDropdown.captionText != null)
                return tmpDropdown.captionText.gameObject;
        }

        if (selectable is Slider sliderForHighlight)
        {
            Image background = GetSliderBackgroundImage(sliderForHighlight);
            if (background != null)
                return background.gameObject;

            Image fill = GetSliderFillImage(sliderForHighlight);
            if (fill != null)
                return fill.gameObject;

            if (sliderForHighlight.targetGraphic != null)
                return sliderForHighlight.targetGraphic.gameObject;
        }

        if (selectable is Toggle toggleForHighlight)
        {
            return toggleForHighlight.gameObject;
        }

        if (selectable is Dropdown dropdown)
        {
            if (dropdown.targetGraphic != null)
                return dropdown.targetGraphic.gameObject;
            if (dropdown.captionText != null)
                return dropdown.captionText.gameObject;
        }

        if (selectable != null && selectable.targetGraphic != null)
            return selectable.targetGraphic.gameObject;

        return selectable != null ? selectable.gameObject : null;
    }

    private RectTransform GetFrameTargetRect(Selectable selectable, PanelConfig panelConfig)
    {
        SelectableVisualOverride visualOverride = GetVisualOverride(selectable);
        RectTransform overrideRect = GetOverrideRectTarget(visualOverride);
        if (overrideRect != null)
            return overrideRect;

        if (selectable is TMP_Dropdown tmpDropdown)
        {
            if (tmpDropdown.targetGraphic != null)
                return tmpDropdown.targetGraphic.rectTransform;
            if (tmpDropdown.captionText != null)
                return tmpDropdown.captionText.rectTransform;
        }

        if (selectable is Slider sliderForFrame)
        {
            Image background = GetSliderBackgroundImage(sliderForFrame);
            if (background != null)
                return background.rectTransform;

            Image fill = GetSliderFillImage(sliderForFrame);
            if (fill != null)
                return fill.rectTransform;

            if (sliderForFrame.targetGraphic != null)
                return sliderForFrame.targetGraphic.rectTransform;
        }

        if (selectable is Toggle toggleForFrame)
        {
            return toggleForFrame.transform as RectTransform;
        }

        if (selectable is Dropdown dropdown)
        {
            if (dropdown.targetGraphic != null)
                return dropdown.targetGraphic.rectTransform;
            if (dropdown.captionText != null)
                return dropdown.captionText.rectTransform;
        }

        if (selectable != null && selectable.targetGraphic != null)
            return selectable.targetGraphic.rectTransform;

        return selectable != null ? selectable.transform as RectTransform : null;
    }

    private RectTransform GetOverrideRectTarget(SelectableVisualOverride visualOverride)
    {
        if (visualOverride == null)
            return null;

        if (visualOverride.highlightRectTarget != null)
            return visualOverride.highlightRectTarget;

        return null;
    }

    private GameObject GetOverrideObjectTarget(SelectableVisualOverride visualOverride)
    {
        if (visualOverride == null)
            return null;

        if (visualOverride.highlightRectTarget != null)
            return visualOverride.highlightRectTarget.gameObject;

        return null;
    }

    private bool IsHighlightRectTooLarge(RectTransform rect)
    {
        if (!skipHugeHighlightRects || rect == null || Screen.width <= 0 || Screen.height <= 0)
            return false;

        rect.GetWorldCorners(_rectCorners);

        float width = Mathf.Abs(_rectCorners[2].x - _rectCorners[0].x);
        float height = Mathf.Abs(_rectCorners[2].y - _rectCorners[0].y);

        float widthRatio = width / Screen.width;
        float heightRatio = height / Screen.height;

        return widthRatio > maxHighlightScreenWidthRatio || heightRatio > maxHighlightScreenHeightRatio;
    }

    private void ApplyPermanentTextColorOverrides()
    {
        if (permanentTextColorOverrides == null)
            return;

        for (int i = 0; i < permanentTextColorOverrides.Length; i++)
        {
            PermanentTextColorOverride item = permanentTextColorOverrides[i];
            if (item == null)
                continue;

            // No sobreescribir el color temporal del texto actualmente seleccionado.
            if (item.tmpText != null && item.tmpText != _styledTmpText && item.tmpText != _styledTmpText2 && item.tmpText.color != item.color)
                item.tmpText.color = item.color;

            if (item.legacyText != null && item.legacyText != _styledLegacyText && item.legacyText.color != item.color)
                item.legacyText.color = item.color;
        }
    }

    private void RestoreSelectedButtonVisuals()
    {
        if (_selectionFrameObject != null)
            _selectionFrameObject.SetActive(false);

        if (_styledSliderFillImage != null)
        {
            _styledSliderFillImage.color = _originalSliderFillColor;
            _styledSliderFillImage = null;
        }

        if (_styledTmpText != null)
        {
            _styledTmpText.text = _originalTmpText;
            _styledTmpText.color = _originalTmpColor;
            _styledTmpText = null;
            _originalTmpText = null;
        }

        if (_styledTmpText2 != null)
        {
            _styledTmpText2.text = _originalTmpText2;
            _styledTmpText2.color = _originalTmpColor2;
            _styledTmpText2 = null;
            _originalTmpText2 = null;
        }

        if (_styledLegacyText != null)
        {
            _styledLegacyText.text = _originalLegacyText;
            _styledLegacyText.color = _originalLegacyColor;
            _styledLegacyText = null;
            _originalLegacyText = null;
        }
    }

    private Image GetSliderFillImage(Slider slider)
    {
        if (slider == null)
            return null;

        if (slider.fillRect != null)
        {
            Image fill = slider.fillRect.GetComponent<Image>();
            if (fill != null)
                return fill;
        }

        Transform fillTransform = slider.transform.Find("Fill Area/Fill");
        if (fillTransform != null)
            return fillTransform.GetComponent<Image>();

        if (slider.targetGraphic is Image targetImage)
            return targetImage;

        return null;
    }

    private Image GetSliderBackgroundImage(Slider slider)
    {
        if (slider == null)
            return null;

        Transform backgroundTransform = slider.transform.Find("Background");
        if (backgroundTransform != null)
        {
            Image bg = backgroundTransform.GetComponent<Image>();
            if (bg != null)
                return bg;
        }

        return null;
    }

    /// <summary>
    /// Para llamar cuando un panel se activa (compatible con UnityEvent en el Inspector)
    /// </summary>
    public void OnPanelChanged()
    {
        _lastSelected = null;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void OnDisable()
    {
        if (_currentOutline != null)
            _currentOutline.enabled = false;

        RestoreSelectedButtonVisuals();
        _currentSelectable = null;
        _lastSelected = null;
        _nullSelectionFrames = 0;
    }
}
