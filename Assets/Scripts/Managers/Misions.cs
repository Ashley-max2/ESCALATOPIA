using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Misions : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject panelUI;
    public float interactionDistance = 3f; 

    private bool isPlayerNear = false;
    private Transform player;

    void Start()
    {
        if (panelUI != null)
            panelUI.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    private void Interact()
    {
        gameObject.SetActive(false);

        if (panelUI != null)
            panelUI.SetActive(true);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            player = other.transform;
            Debug.Log("Pulsa E para interactuar");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
}
