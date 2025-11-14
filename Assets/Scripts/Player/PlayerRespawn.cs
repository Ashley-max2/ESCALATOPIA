using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Transform lastRespawnPoint;

    void OnTriggerEnter(Collider other)
    {
        // Si el jugador entra en un collider con tag "respawn"
        if (other.CompareTag("respawn"))
        {
            lastRespawnPoint = other.transform;
            Debug.Log("Nuevo punto de respawn guardado: " + other.name);
        }
    }

    public void Respawn()
    {
        if (lastRespawnPoint == null)
        {
            Debug.LogWarning("No se ha tocado ningún respawn todavía.");
            return;
        }

        // Movemos al jugador al último respawn
        transform.position = lastRespawnPoint.position;

        // Revivimos al jugador
        GetComponent<PlayerFallDetector>().playerLive = true;

        // Reiniciamos la altura inicial
        GetComponent<PlayerFallDetector>().ResetStartY();

        Debug.Log("Jugador reapareció en: " + lastRespawnPoint.name);
    }
}
