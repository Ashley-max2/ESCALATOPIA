using UnityEngine;
using DG.Tweening;

public class MisionManager : MonoBehaviour
{
    [Header("Panel de Misiones")]
    public GameObject missionPanel;

    private bool canOpenPanel = false;   // Se activa cuando se recoge el objeto

    void Start()
    {
        if (missionPanel != null)
            missionPanel.SetActive(false);
    }

    // Este método lo llamaremos desde el objeto que se recoge
    public void ObjectCollected()
    {
        canOpenPanel = true;
        Debug.Log("Objeto recogido → Ahora puedes pulsar J para abrir/cerrar el panel");
    }

    // Abrir o cerrar el panel con J
    public void ToggleMissionPanel()
    {
        if (!canOpenPanel || missionPanel == null) return;

        missionPanel.SetActive(!missionPanel.activeSelf);

        Debug.Log(missionPanel.activeSelf ? "Panel de misiones ABIERTO" : "Panel de misiones CERRADO");
    }
}