using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeadManager : MonoBehaviour
{
    [Header("Player Settings")]
    public GameObject player;          // Asigna el jugador en el inspector
    public float maxFall = 10f;        // Altura máxima de caída para morir

    private float maxY;
    private bool playerAlive = true;
    private PlayerRespawn respawnSystem;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("DeadManager: No se ha asignado el jugador.");
            return;
        }

        maxY = player.transform.position.y;
        respawnSystem = player.GetComponent<PlayerRespawn>();
        if (respawnSystem == null)
        {
            Debug.LogWarning("DeadManager: No se encontró PlayerRespawn en el jugador.");
        }
    }

    void Update()
    {
        if (!playerAlive) return;

        // Actualizamos la altura máxima alcanzada
        if (player.transform.position.y > maxY)
        {
            maxY = player.transform.position.y;
        }

        // Comprobamos si el jugador ha caído demasiado
        CheckFallDeath();
    }

    // Comprueba si el jugador ha caído más de maxFall
    private void CheckFallDeath()
    {
        float fallDistance = maxY - player.transform.position.y;

        if (fallDistance > maxFall)
        {
            playerAlive = false;
            Debug.Log("Jugador muerto por caída.");
            OpenDeadScene();
        }
    }

    // Carga la escena Dead_Menu
    private void OpenDeadScene()
    {
        Debug.Log("Cargando escena Dead_Menu...");
        SceneManager.LoadScene("Dead_Menu");
    }

    // Método público para respawnear al jugador
    public void RespawnPlayer()
    {
        if (respawnSystem != null)
        {
            respawnSystem.Respawn();
            playerAlive = true;
            maxY = player.transform.position.y;
            Debug.Log("Jugador respawneado.");
        }
        else
        {
            Debug.LogWarning("DeadManager: No se puede respawnear, PlayerRespawn no asignado.");
        }
    }

    // Método público para salir al menú principal (rellenarás tú)
    public void ExitToMainMenu()
    {
        Debug.Log("Método para salir al menú principal aún no implementado.");
        // SceneManager.LoadScene("MainMenu"); // Descomenta cuando tengas la escena
    }
}
