using UnityEngine;

/// <summary>
/// Controla la reiniciación de la carrera después de que el jugador pierde.
/// Se activa cuando BossRaceManager detecta que el Boss gana.
/// </summary>
public class RaceRestartController : MonoBehaviour
{
    [Header("Referencias de Carrera")]
    [Tooltip("El manager que controla el estado de la carrera")]
    public BossManager bossManager;

    [Tooltip("El controlador de IA del Boss")]
    public BossAIController bossAIController;

    [Tooltip("El GameObject del diamante/objeto final de la carrera")]
    public GameObject raceFinishObject;

    [Tooltip("El diálogo del personaje")]
    public CharacterDialogue characterDialogue;

    /// <summary>
    /// Reinicia la carrera: desactiva la IA, resetea el estado y permite intentar de nuevo.
    /// Llamar después de que se haya mostrado el diálogo de derrota.
    /// </summary>
    public void RestartRace()
    {
        // Desactivar IA del Boss
        if (bossAIController != null)
        {
            bossAIController.enabled = false;
            Debug.Log("[RaceRestartController] BossAIController desactivado.");
        }

        // Resetear el collider del diamante si fue desactivado
        if (raceFinishObject != null)
        {
            Collider col = raceFinishObject.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            // Re-activar renderizadores si estaban ocultos
            foreach (Renderer r in raceFinishObject.GetComponentsInChildren<Renderer>())
                r.enabled = true;

            Debug.Log("[RaceRestartController] Objeto final reseteado.");
        }

        // Resetear el manager de carrera
        if (bossManager != null)
        {
            bossManager.RestartRace();
            Debug.Log("[RaceRestartController] BossManager reiniciado.");
        }

        // Permitir que se intente la carrera de nuevo
        if (characterDialogue != null)
        {
            characterDialogue.AllowRaceRetry();
            characterDialogue.SetInteractionEnabled(true);
            Debug.Log("[RaceRestartController] CharacterDialogue permitiendo reintentar la carrera.");
        }
    }
}
