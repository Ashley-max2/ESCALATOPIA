using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallDetector : MonoBehaviour
{
    public bool playerLive = true;
    public float startY;
    public float maxFall = 10f;

    private float maxY;
    private PlayerRespawn respawnSystem;

    void Start()
    {
        startY = transform.position.y;
        maxY = startY;
        respawnSystem = GetComponent<PlayerRespawn>();
    }

    void Update()
    {

        if (transform.position.y > maxY)
        {
            maxY = transform.position.y;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
      
        float fallDistance = maxY - transform.position.y;

        if (fallDistance > maxFall)
        {
            playerLive = false;
            Debug.Log("El jugador ha caído más de " + maxFall + " metros desde su altura máxima, muerte.");

            if (respawnSystem != null)
            {
                respawnSystem.Respawn();
            }
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
