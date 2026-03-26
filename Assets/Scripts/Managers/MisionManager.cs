using UnityEngine;
using DG.Tweening;

public class MisionManager : MonoBehaviour
{
    [Header("Panel de Misiones")]
    public GameObject missionPanel;

    void Start()
    {
        if (missionPanel != null)
            missionPanel.SetActive(false);
    }

    // Este método se llama cuando se recoge el objeto
    public void ObjectCollected()
    {
        if (missionPanel != null)
        {
            missionPanel.SetActive(true);           // Abre el panel automáticamente
            Debug.Log("Objeto recogido → Panel de misiones abierto");
        }
    }
}