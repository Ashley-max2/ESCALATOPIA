using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Maneja la navegación por gamepad en el menú de pausa.
/// Detecta automáticamente todos los botones y permite navegar con Left Stick/D-Pad.
/// </summary>
public class PauseMenuGamepadNavigation : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private EventSystem eventSystem;

    [Header("Configuración")]
    [SerializeField] private float navigationCooldown = 0.2f;
    [SerializeField] private float stickDeadzone = 0.3f;

    private float navigationTimer = 0f;
    private InputAction navigateAction;
    private PlayerInput playerInput;
    private List<Button> allButtons = new List<Button>();
    private int currentButtonIndex = -1;

    private void Start()
    {
        // Buscar referencias automáticamente si no están asignadas
        if (pauseMenu == null)
            pauseMenu = GetComponent<PauseMenu>();

        if (eventSystem == null)
            eventSystem = EventSystem.current;

        // Obtener PlayerInput del jugador si está disponible
        playerInput = FindObjectOfType<PlayerInput>();

        // Si tenemos PlayerInput, obtener la acción de navegación
        if (playerInput != null)
        {
            var inputActions = playerInput.actions;
            if (inputActions != null)
            {
                navigateAction = inputActions.FindAction("UI/Navigate");
                if (navigateAction != null)
                {
                    navigateAction.Enable();
                }
            }
        }

        // Recolectar todos los botones de la escena
        CollectAllButtons();

        // Seleccionar el primer botón activo
        SelectFirstActiveButton();
    }

    private void CollectAllButtons()
    {
        allButtons.Clear();
        
        // Buscar todos los botones en la escena (incluyendo hijos inactivos temporalmente)
        Button[] allButtonsInScene = FindObjectsOfType<Button>(true);
        
        foreach (Button button in allButtonsInScene)
        {
            // Solo incluir botones que sean interactuables
            if (button.IsInteractable())
            {
                allButtons.Add(button);
            }
        }

        Debug.Log($"[PauseMenuGamepadNavigation] Se encontraron {allButtons.Count} botones interactuables");
    }

    private void SelectFirstActiveButton()
    {
        if (eventSystem == null || allButtons.Count == 0) return;

        // Buscar el primer botón que esté activo en la jerarquía
        for (int i = 0; i < allButtons.Count; i++)
        {
            if (allButtons[i].gameObject.activeSelf && allButtons[i].IsInteractable())
            {
                currentButtonIndex = i;
                eventSystem.SetSelectedGameObject(allButtons[i].gameObject);
                Debug.Log($"[PauseMenuGamepadNavigation] Botón seleccionado: {allButtons[i].name}");
                return;
            }
        }
    }

    private void Update()
    {
        if (pauseMenu == null || eventSystem == null || allButtons.Count == 0)
            return;

        // Solo procesar input si el menú de pausa está activo
        if (!pauseMenu.menuDePausa.activeSelf)
            return;

        navigationTimer -= Time.deltaTime;

        // Leer input de navegación
        Vector2 navigationInput = Vector2.zero;

        // Opción 1: Usar Input System UI Navigate (si está configurado)
        if (navigateAction != null && navigateAction.enabled)
        {
            navigationInput = navigateAction.ReadValue<Vector2>();
        }

        // Opción 2: Fallback a Gamepad directo
        if (navigationInput.sqrMagnitude < 0.01f && Gamepad.current != null)
        {
            navigationInput = Gamepad.current.leftStick.ReadValue();
        }

        // Procesar navegación si el cooldown pasó
        if (navigationInput.sqrMagnitude > stickDeadzone && navigationTimer <= 0f)
        {
            if (navigationInput.y > 0) // Arriba
            {
                NavigateUp();
                navigationTimer = navigationCooldown;
            }
            else if (navigationInput.y < 0) // Abajo
            {
                NavigateDown();
                navigationTimer = navigationCooldown;
            }
        }

        // Navegación horizontal para sliders
        if (navigationInput.sqrMagnitude > stickDeadzone)
        {
            if (navigationInput.x > 0.5f) // Derecha
            {
                AdjustSliderRight();
            }
            else if (navigationInput.x < -0.5f) // Izquierda
            {
                AdjustSliderLeft();
            }
        }
    }

    private void NavigateUp()
    {
        // Obtener el botón actualmente seleccionado
        GameObject currentSelected = eventSystem.currentSelectedGameObject;
        
        if (currentSelected == null)
        {
            SelectFirstActiveButton();
            return;
        }

        // Buscar el índice del botón actualmente seleccionado
        int selectedIndex = -1;
        for (int i = 0; i < allButtons.Count; i++)
        {
            if (allButtons[i].gameObject == currentSelected)
            {
                selectedIndex = i;
                break;
            }
        }

        if (selectedIndex >= 0)
        {
            // Buscar el botón anterior activo
            int newIndex = selectedIndex - 1;
            if (newIndex < 0) newIndex = allButtons.Count - 1; // Circular

            while (newIndex != selectedIndex && (!allButtons[newIndex].gameObject.activeSelf || !allButtons[newIndex].IsInteractable()))
            {
                newIndex--;
                if (newIndex < 0) newIndex = allButtons.Count - 1;
            }

            if (allButtons[newIndex].gameObject.activeSelf && allButtons[newIndex].IsInteractable())
            {
                currentButtonIndex = newIndex;
                eventSystem.SetSelectedGameObject(allButtons[newIndex].gameObject);
                PlayButtonSound();
            }
        }
    }

    private void NavigateDown()
    {
        // Obtener el botón actualmente seleccionado
        GameObject currentSelected = eventSystem.currentSelectedGameObject;
        
        if (currentSelected == null)
        {
            SelectFirstActiveButton();
            return;
        }

        // Buscar el índice del botón actualmente seleccionado
        int selectedIndex = -1;
        for (int i = 0; i < allButtons.Count; i++)
        {
            if (allButtons[i].gameObject == currentSelected)
            {
                selectedIndex = i;
                break;
            }
        }

        if (selectedIndex >= 0)
        {
            // Buscar el botón siguiente activo
            int newIndex = selectedIndex + 1;
            if (newIndex >= allButtons.Count) newIndex = 0; // Circular

            while (newIndex != selectedIndex && (!allButtons[newIndex].gameObject.activeSelf || !allButtons[newIndex].IsInteractable()))
            {
                newIndex++;
                if (newIndex >= allButtons.Count) newIndex = 0;
            }

            if (allButtons[newIndex].gameObject.activeSelf && allButtons[newIndex].IsInteractable())
            {
                currentButtonIndex = newIndex;
                eventSystem.SetSelectedGameObject(allButtons[newIndex].gameObject);
                PlayButtonSound();
            }
        }
    }

    private void AdjustSliderRight()
    {
        GameObject currentSelected = eventSystem.currentSelectedGameObject;
        if (currentSelected == null) return;

        Slider slider = currentSelected.GetComponent<Slider>();
        if (slider != null && !slider.IsInteractable()) return;

        if (slider != null)
        {
            float increment = slider.wholeNumbers ? 1f : 0.1f;
            slider.value = Mathf.Min(slider.value + increment, slider.maxValue);
        }
    }

    private void AdjustSliderLeft()
    {
        GameObject currentSelected = eventSystem.currentSelectedGameObject;
        if (currentSelected == null) return;

        Slider slider = currentSelected.GetComponent<Slider>();
        if (slider != null && !slider.IsInteractable()) return;

        if (slider != null)
        {
            float decrement = slider.wholeNumbers ? 1f : 0.1f;
            slider.value = Mathf.Max(slider.value - decrement, slider.minValue);
        }
    }

    private void PlayButtonSound()
    {
        // Reproducir sonido del botón si está disponible
        if (pauseMenu != null && pauseMenu.GetComponent<AudioSource>() != null)
        {
            pauseMenu.GetComponent<AudioSource>().Play();
        }
    }

    private void OnDestroy()
    {
        if (navigateAction != null)
        {
            navigateAction.Disable();
        }
    }
}
