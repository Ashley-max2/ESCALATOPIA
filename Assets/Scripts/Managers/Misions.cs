using UnityEngine;
using DG.Tweening;

public class Misions : MonoBehaviour
{
    [Header("Panel de Misiones")]
    public GameObject missionPanel;
    public RectTransform missionPanelRect;

    [Header("Configuración de Animación")]
    public float animationDuration = 0.6f;
    public Vector2 hiddenPosition = new Vector2(-900f, 0f);   // Fuera de pantalla (izquierda)
    public Vector2 shownPosition = Vector2.zero;              // Posición visible

    [Header("Estado")]
    public bool objectCollected = false;

    private bool isPlayerNear = false;
    private bool isPanelOpen = false;
    private Tween currentTween;

    void Start()
    {
        if (missionPanel != null)
        {
            missionPanel.SetActive(false);
            missionPanelRect.anchoredPosition = hiddenPosition;
        }
    }

    void Update()
    {
        if (!isPlayerNear) return;

        // === RECOGER OBJETO con tecla E ===
        if (!objectCollected && Input.GetKeyDown(KeyCode.E))
        {
            CollectObject();
        }

        // === ABRIR/CERRAR PANEL con tecla J (solo si ya recogió el objeto) ===
        if (objectCollected && Input.GetKeyDown(KeyCode.J))
        {
            ToggleMissionPanel();
        }
    }

    private void CollectObject()
    {
        objectCollected = true;
        gameObject.SetActive(false);           // Desactiva el objeto (lo "recoge")

        Debug.Log("¡Objeto recogido! Ahora puedes abrir el menú de misiones pulsando J");
    }

    private void ToggleMissionPanel()
    {
        if (isPanelOpen)
            CloseMissionPanel();
        else
            OpenMissionPanel();
    }

    public void OpenMissionPanel()
    {
        if (missionPanel == null) return;

        missionPanel.SetActive(true);
        isPanelOpen = true;

        currentTween?.Kill();

        currentTween = missionPanelRect.DOAnchorPos(shownPosition, animationDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => Debug.Log("Menú de misiones abierto"));
    }

    public void CloseMissionPanel()
    {
        if (missionPanel == null) return;

        isPanelOpen = false;

        currentTween?.Kill();

        currentTween = missionPanelRect.DOAnchorPos(hiddenPosition, animationDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                missionPanel.SetActive(false);
                Debug.Log("Menú de misiones cerrado");
            });
    }

    // ====================== TRIGGER ======================
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;

            if (!objectCollected)
                Debug.Log("Pulsa E para recoger el objeto");
            else
                Debug.Log("Pulsa J para abrir el menú de misiones");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }

    // Opcional: método público por si quieres llamarlo desde otro script
    public void ForceCollectObject()
    {
        CollectObject();
    }
}