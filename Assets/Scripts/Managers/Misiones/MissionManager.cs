using TMPro;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI objectiveText;

    [Header("Current Mission")]
    [TextArea]
    [SerializeField] private string currentObjective;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateObjectiveUI();
    }

    public void SetObjective(string newObjective)
    {
        currentObjective = newObjective;
        UpdateObjectiveUI();
    }

    public string GetCurrentObjective()
    {
        return currentObjective;
    }

    private void UpdateObjectiveUI()
    {
        objectiveText.text = currentObjective;
    }
}