using UnityEngine;

/// <summary>
/// Script de inicialización que desactiva automáticamente los sistemas antiguos de diálogo
/// para usar solo el nuevo sistema CharacterDialogue.
/// Asigna este script al personaje Anas (mismo GameObject que tiene CharacterDialogue).
/// 
/// Automáticamente detecta y desactiva:
/// - NPCInteractable
/// - BossManager
/// </summary>
public class DialogueSystemInitializer : MonoBehaviour
{
    [Tooltip("Si está activo, desactivará automáticamente los scripts conflictivos en Start")]
    [SerializeField] private bool autoDisableConflictingScripts = true;

    private void Start()
    {
        if (autoDisableConflictingScripts)
        {
            DisableConflictingScripts();
        }
    }

    private void DisableConflictingScripts()
    {
        // Buscar y desactivar NPCInteractable
        NPCInteractable npcInteractable = GetComponent<NPCInteractable>();
        if (npcInteractable != null)
        {
            npcInteractable.enabled = false;
            Debug.Log("[DialogueSystemInitializer] ✓ NPCInteractable desactivado.");
        }

        // Buscar y desactivar BossManager
        BossManager bossManager = GetComponent<BossManager>();
        if (bossManager != null)
        {
            bossManager.enabled = false;
            Debug.Log("[DialogueSystemInitializer] ✓ BossManager desactivado.");
        }

        Debug.Log("[DialogueSystemInitializer] Sistema configurado para usar CharacterDialogue.");
    }
}
