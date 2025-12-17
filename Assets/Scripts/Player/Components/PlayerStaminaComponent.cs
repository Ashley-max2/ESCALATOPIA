using UnityEngine;

/// <summary>
/// Wrapper around ResistenceController implementing IStaminaUser interface.
/// Single Responsibility: Stamina management interface.
/// Follows Adapter pattern to make existing ResistenceController work with new interfaces.
/// </summary>
[RequireComponent(typeof(ResistenceController))]
public class PlayerStaminaComponent : MonoBehaviour, IStaminaUser
{
    private ResistenceController resistenceController;

    // IStaminaUser implementation
    public float CurrentStamina => resistenceController != null ? resistenceController.GetResistenciaActual() : 0f;
    public float MaxStamina => resistenceController != null ? resistenceController.resistenciaMaxima : 100f;
    public bool IsRegenerating => resistenceController != null && resistenceController.GetResistenciaActual() < MaxStamina;
    public bool IsDepleted => CurrentStamina <= 0f;

    private void Awake()
    {
        resistenceController = GetComponent<ResistenceController>();
        
        if (resistenceController == null)
        {
            Debug.LogError("[PlayerStaminaComponent] ResistenceController not found!");
        }
    }

    /// <summary>
    /// Check if there is enough stamina.
    /// </summary>
    public bool HasStamina(float amount)
    {
        if (resistenceController == null)
            return false;

        return resistenceController.TieneResistencia(amount);
    }

    /// <summary>
    /// Consume stamina.
    /// </summary>
    public bool ConsumeStamina(float amount)
    {
        if (resistenceController == null)
            return false;

        if (!HasStamina(amount))
            return false;

        resistenceController.ConsumirResistencia(amount);
        return true;
    }

    /// <summary>
    /// Regenerate stamina.
    /// </summary>
    public void RegenerateStamina(float amount)
    {
        if (resistenceController == null)
            return;

        resistenceController.RegenerarResistencia(amount);
    }

    /// <summary>
    /// Get stamina percentage (0-1).
    /// </summary>
    public float GetStaminaPercentage()
    {
        if (MaxStamina <= 0)
            return 0f;

        return CurrentStamina / MaxStamina;
    }

    /// <summary>
    /// Debug stamina state.
    /// </summary>
    public void DebugStaminaState()
    {
        if (resistenceController != null)
        {
            resistenceController.DebugEstadoResistencia();
        }
    }
}
