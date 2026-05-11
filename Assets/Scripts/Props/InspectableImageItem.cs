using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Permite al jugador interactuar con un objeto usando la tecla E para mostrar una imagen a pantalla completa.
/// </summary>
public class InspectableImageItem : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("El GameObject que contiene la 'E' que flota sobre el objeto. Debe estar asignado en la escena.")]
    [SerializeField] private GameObject promptE;

    [Tooltip("El panel de UI que cubrirá toda la pantalla.")]
    [SerializeField] private GameObject fullscreenImagePanel;

    [Tooltip("El componente Image de UI donde se mostrará la imagen del objeto.")]
    [SerializeField] private Image fullscreenImageUI;

    [Header("Item Settings")]
    [Tooltip("La imagen (Sprite) que se mostrará en pantalla completa al inspeccionar este objeto.")]
    [SerializeField] private Sprite imageToShow;

    [Tooltip("Si es true, pausa el tiempo del juego mientras se ve la imagen.")]
    [SerializeField] private bool pauseGameWhenViewing = true;

    private bool playerInsideTrigger = false;
    private bool isViewingImage = false;
    private TMP_Text promptText;
    private string lastPromptLabel = string.Empty;

    // Cooldown para evitar que el mismo frame en el que se abre la imagen se cierre por detectar una pulsación
    private bool frameCooldown = false;

    private void Start()
    {
        promptText = promptE != null ? promptE.GetComponentInChildren<TMP_Text>(true) : null;
        if (promptE != null) promptE.SetActive(false);
        if (fullscreenImagePanel != null) fullscreenImagePanel.SetActive(false);
    }

    private void LateUpdate()
    {
        UpdatePromptBillboard();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled) return;

        // Verifica que sea el jugador
        if (other.CompareTag("Player"))
        {
            playerInsideTrigger = true;
            // Mostrar la "E" si no estamos viendo ya la imagen
            if (!isViewingImage && promptE != null)
            {
                promptE.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!enabled) return;

        if (other.CompareTag("Player"))
        {
            playerInsideTrigger = false;

            if (promptE != null) promptE.SetActive(false);

            // Si por alguna razón el jugador sale del área mientras ve la imagen (por ej, si no pausas el juego), la cerramos
            if (isViewingImage && !pauseGameWhenViewing)
            {
                CloseImage();
            }
        }
    }

    private void Update()
    {
        UpdatePromptKeyLabel();

        // Consumimos el cooldown de 1 frame para no detectar input en el mismo frame que se abrió
        if (frameCooldown)
        {
            frameCooldown = false;
            return;
        }

        if (isViewingImage)
        {
            // Si está viendo la imagen, cualquier tecla (o clic) la cierra
            if (Input.anyKeyDown)
            {
                CloseImage();
            }
        }
        else if (playerInsideTrigger)
        {
                // Si está cerca del objeto y pulsa la acción de interacción, abrimos la imagen
                if (InteractInput.PressedThisFrame())
            {
                OpenImage();
            }
        }
    }

    private void UpdatePromptKeyLabel()
    {
        if (promptText == null) return;

        string label = InteractInput.GetBracketedDisplayKey();
        if (label != lastPromptLabel)
        {
            promptText.text = label;
            lastPromptLabel = label;
        }
    }

    private void UpdatePromptBillboard()
    {
        if (promptE == null || !promptE.activeSelf)
            return;

        Transform billboardTarget = null;

        if (Camera.main != null)
            billboardTarget = Camera.main.transform;
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                billboardTarget = player.transform;
        }

        if (billboardTarget == null)
            return;

        Vector3 direction = billboardTarget.position - promptE.transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        promptE.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    private void OpenImage()
    {
        isViewingImage = true;
        frameCooldown = true; // Evitar cierre en el frame inmediatamente posterior

        // Ocultar la E mientras se ve la imagen
        if (promptE != null) promptE.SetActive(false);

        // Configurar y mostrar la imagen en pantalla completa
        if (fullscreenImagePanel != null && fullscreenImageUI != null && imageToShow != null)
        {
            fullscreenImageUI.sprite = imageToShow;
            fullscreenImagePanel.SetActive(true);
        }

        // Pausar el juego si está configurado para hacerlo
        if (pauseGameWhenViewing)
        {
            Time.timeScale = 0f;
        }
    }

    private void CloseImage()
    {
        isViewingImage = false;

        // Ocultar el panel de la imagen
        if (fullscreenImagePanel != null)
        {
            fullscreenImagePanel.SetActive(false);
        }

        // Reanudar el tiempo
        if (pauseGameWhenViewing)
        {
            Time.timeScale = 1f;
        }

        // Si el jugador sigue dentro del trigger, volver a mostrar la E
        if (playerInsideTrigger && promptE != null)
        {
            promptE.SetActive(true);
        }
    }
}
