using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JournalPickUp : MonoBehaviour
{
    public MisionManager missionManager;

    private bool isPlayerNear = false;

    void Update()
    {
        if (isPlayerNear && InteractInput.PressedThisFrame())
        {

            Pickup();

        }
    }

    private void Pickup()
    {
        if (missionManager != null)
            missionManager.ObjectCollected();

        gameObject.SetActive(false);
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
