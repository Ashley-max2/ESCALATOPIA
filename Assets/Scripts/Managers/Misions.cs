using UnityEngine;
using DG.Tweening;

public class Misions : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject missionPanel;           // ← Panel de misiones
    public RectTransform missionPanelRect;    // ← El mismo panel

    [Header("Configuración")]
    public float hiddenX = -900f;             // Cambia si no entra bien
    public float animationTime = 0.5f;

    private bool objectCollected = false;
    private bool isPlayerNear = false;

    void Start()
    {
        if (missionPanelRect != null)
        {
            missionPanel.SetActive(false);
            missionPanelRect.anchoredPosition = new Vector2(hiddenX, 0f);
        }
    }

    void Update()
    {
        if (!isPlayerNear) return;

        // Recoger el objeto con E
        if (!objectCollected && Input.GetKeyDown(KeyCode.E))
        {
            CollectObject();
        }

        // Abrir/Cerrar misiones con J (solo después de recoger)
        if (objectCollected && Input.GetKeyDown(KeyCode.J))
        {
            ToggleMissionPanel();
        }
    }

    private void CollectObject()
    {
        objectCollected = true;
        Debug.Log("Objeto recogido. Ahora pulsa J para abrir las misiones");

        // IMPORTANTE: NO desactivamos este GameObject
        // gameObject.SetActive(false);   ← Comentado para que el script siga vivo
    }

    private void ToggleMissionPanel()
    {
        if (missionPanel.activeSelf)
            ClosePanel();
        else
            OpenPanel();
    }

    private void OpenPanel()
    {
        missionPanel.SetActive(true);
        missionPanelRect.anchoredPosition = new Vector2(hiddenX, 0f);

        missionPanelRect.DOAnchorPos(Vector2.zero, animationTime)
            .SetEase(Ease.OutBack);
    }

    private void ClosePanel()
    {
        missionPanelRect.DOAnchorPos(new Vector2(hiddenX, 0f), animationTime)
            .SetEase(Ease.InBack)
            .OnComplete(() => missionPanel.SetActive(false));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerNear = false;
    }
}