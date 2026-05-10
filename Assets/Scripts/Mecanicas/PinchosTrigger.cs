using UnityEngine;

/// <summary>
/// Detecta la colisión del jugador con objetos etiquetados como "pinchos".
/// Al contacto, llama a Die() en la PlayerStateMachine, que activa el
/// PlayerDeadState y hace el respawn automático tras el delay configurado.
///
/// CONFIGURACIÓN:
///   - Añade este script al GameObject del jugador (el que tiene PlayerStateMachine).
///   - Los objetos de pinchos deben tener el tag "pinchos" en el Inspector.
///   - Los colliders de los pinchos pueden ser Trigger o sólidos (ambos funcionan).
///   - Si usas Trigger en los pinchos, activa "Is Trigger" en su Collider.
/// </summary>
[RequireComponent(typeof(PlayerStateMachine))]
public class PinchosTrigger : MonoBehaviour
{
    private const string PINCHOS_TAG = "pinchos";

    private PlayerStateMachine _player;

    private void Awake()
    {
        _player = GetComponent<PlayerStateMachine>();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Colisión física (Collider sólido en los pinchos, sin Is Trigger)
    // ──────────────────────────────────────────────────────────────────────────
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(PINCHOS_TAG))
        {
            HandlePinchosContact();
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Zona de trigger (Collider con Is Trigger activado en los pinchos)
    // ──────────────────────────────────────────────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(PINCHOS_TAG))
        {
            HandlePinchosContact();
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Lógica central
    // ──────────────────────────────────────────────────────────────────────────
    private void HandlePinchosContact()
    {
        // Evitar respawn múltiple si ya está reapareciendo
        if (_player.CurrentState is PlayerDeadState)
            return;

        Debug.Log("[PinchosTrigger] Jugador tocó pinchos → respawn instantáneo al checkpoint.");
        _player.Respawn();
    }
}
