using TMPro;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    private string currentObjective;
    private TextMeshProUGUI objectiveText;

    // Auto creación si no existe
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("MissionManager");
            go.AddComponent<MissionManager>();
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("MissionManager creado y persistente");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetObjective(string newObjective)
    {
        currentObjective = newObjective;
        UpdateUI();
    }

    public void BindUI(TextMeshProUGUI uiText)
    {
        objectiveText = uiText;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (objectiveText != null)
            objectiveText.text = currentObjective;
    }
}