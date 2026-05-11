using TMPro;
using UnityEngine;

public class MissionUIBinder : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI objectiveText;

    private void Start()
    {
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.BindUI(objectiveText);
        }
    }
}