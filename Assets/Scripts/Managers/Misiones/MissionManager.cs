using TMPro;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    private string currentObjective;
    private TextMeshProUGUI objectiveText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    public void SetObjective(string newObjective)
    {
        currentObjective = newObjective;
        UpdateUI();
    }

    public string GetCurrentObjective()
    {
        return currentObjective;
    }

    public void BindUI(TextMeshProUGUI uiText)
    {
        objectiveText = uiText;
        UpdateUI(); // clave para que funcione al cambiar escena
    }

    private void UpdateUI()
    {
        if (objectiveText != null)
            objectiveText.text = currentObjective;
    }
}