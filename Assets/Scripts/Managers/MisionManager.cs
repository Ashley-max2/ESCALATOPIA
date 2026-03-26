using UnityEngine;
using DG.Tweening;

public class MisionManager : MonoBehaviour
{
    [Header("Panel de Misiones")]
    public GameObject missionPanel;

    private bool openClosePanel;

    void Start()
    {
        if (missionPanel != null)
            missionPanel.SetActive(false);
    }

    /*
    public void ObjectCollected()
    {
        if (missionPanel != null)
        {
            missionPanel.SetActive(true);
            Debug.Log("Objeto recogido → Panel de misiones abierto");
        }
    }
    */

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.J))
        {
            if (missionPanel != null)
            {
                missionPanel.SetActive(!missionPanel.activeSelf);

                Debug.Log(missionPanel.activeSelf ? "Panel ABIERTO" : "Panel CERRADO");
            }
        }


    }

}