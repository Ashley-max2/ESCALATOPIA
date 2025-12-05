/*using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class PlayerFallDetector : MonoBehaviour
{
    public bool playerLive = true;
    public float startY;
    public float maxFall = 10f;

    private float maxY;
    private PlayerRespawn respawnSystem;

    [Header("UI")]
    public GameObject deadMenu;

    void Start()
    {
        startY = transform.position.y;
        maxY = startY;
        respawnSystem = GetComponent<PlayerRespawn>();

        if (deadMenu != null)
            deadMenu.SetActive(false);
    }

    void Update()
    {

        if (transform.position.y > maxY)
        {
            maxY = transform.position.y;
        }

        if (playerLive == true)
            deadMenu.SetActive(false);
    }

    void OnCollisionEnter(Collision collision)
    {
      
        float fallDistance = maxY - transform.position.y;

        if (fallDistance > maxFall)
        {
            playerLive = false;
            Debug.Log("El jugador ha caído más de " + maxFall + " metros desde su altura máxima, muerte.");

            if (deadMenu != null)
                deadMenu.SetActive(true);
        }
        else
        {
            // Si la caída fue segura, actualizamos referencia
            startY = transform.position.y;
            maxY = startY;
            Debug.Log("Caída segura, jugador sigue vivo.");
        }
    }

    public void ResetStartY()
    {
        startY = transform.position.y;
        maxY = startY;
    }

}
*/