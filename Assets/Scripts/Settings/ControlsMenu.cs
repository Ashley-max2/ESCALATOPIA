using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ControlsMenu : MonoBehaviour
{
    public static ControlsMenu Instance;

    [SerializeField] private GameObject rebindPanel; // Panel "PULSA UNA TECLA"
    [SerializeField] private TMP_Text rebindText;
    [SerializeField] private GameObject conflictPanel; // Panel "Tecla ya en uso"
    [SerializeField] private TMP_Text conflictText;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button closeButton;

    private PlayerInputHandler inputHandler;

    void Awake()
    {
        Instance = this;
        inputHandler = FindObjectOfType<PlayerInputHandler>();
    }

    void Start()
    {
        if (resetButton) resetButton.onClick.AddListener(() => { inputHandler.ResetDefaults(); });
        if (closeButton) closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        if (rebindText) rebindText.text = "PULSA UNA TECLA\n(ESC para cancelar)";
    }

    public void ShowRebindPanel()
    {
        if (rebindPanel) rebindPanel.SetActive(true);
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

    IEnumerator HideConflictAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideRebindPanel();
    }
}