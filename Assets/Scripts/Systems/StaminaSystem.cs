using UnityEngine;

/// <summary>
/// Sistema de estamina para escalada y otras acciones.
/// La estamina se consume al escalar y se regenera cuando no se usa.
/// </summary>
public class StaminaSystem : MonoBehaviour
{
    [Header("=== STAMINA CONFIG ===")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina = 100f;
    [SerializeField] private float regenRate = 15f;
    [SerializeField] private float regenDelay = 1.5f;
    
    [Header("=== THRESHOLDS ===")]
    [SerializeField] private float warningThreshold = 25f;
    [SerializeField] private float criticalThreshold = 10f;
    
    // Events
    public event System.Action<float, float> OnStaminaChanged; // current, max
    public event System.Action OnStaminaDepleted;
    public event System.Action OnStaminaWarning;
    
    // Properties
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public float StaminaPercent => currentStamina / maxStamina;
    
    // Runtime
    private float _lastConsumeTime;
    private bool _warningTriggered;
    
    private void Update()
    {
        RegenerateStamina();
    }
    
    /// <summary>
    /// Consume una cantidad de estamina
    /// </summary>
    public void ConsumeStamina(float amount)
    {
        if (amount <= 0) return;
        
        float previousStamina = currentStamina;
        currentStamina = Mathf.Max(0, currentStamina - amount);
        _lastConsumeTime = Time.time;
        
        // Trigger events
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        
        // Warning
        if (currentStamina <= warningThreshold && previousStamina > warningThreshold && !_warningTriggered)
        {
            _warningTriggered = true;
            OnStaminaWarning?.Invoke();
        }
        
        // Depleted
        if (currentStamina <= 0 && previousStamina > 0)
        {
            OnStaminaDepleted?.Invoke();
        }
    }
    
    /// <summary>
    /// Verifica si hay suficiente estamina
    /// </summary>
    public bool HasStamina(float amount = 0.1f)
    {
        return currentStamina >= amount;
    }
    
    /// <summary>
    /// Verifica si hay estamina disponible (shortcut)
    /// </summary>
    public bool HasStamina()
    {
        return currentStamina > criticalThreshold;
    }
    
    /// <summary>
    /// Establece la estamina máxima (para upgrades)
    /// </summary>
    public void SetMaxStamina(float newMax)
    {
        maxStamina = newMax;
        currentStamina = Mathf.Min(currentStamina, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }
    
    /// <summary>
    /// Restaura toda la estamina
    /// </summary>
    public void RestoreStamina()
    {
        currentStamina = maxStamina;
        _warningTriggered = false;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }
    
    private void RegenerateStamina()
    {
        // Only regen after delay
        if (Time.time - _lastConsumeTime < regenDelay) return;
        
        if (currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + regenRate * Time.deltaTime);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            
            // Reset warning when above threshold
            if (currentStamina > warningThreshold)
            {
                _warningTriggered = false;
            }
        }
    }
    
    private void OnValidate()
    {
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        warningThreshold = Mathf.Clamp(warningThreshold, 0, maxStamina);
        criticalThreshold = Mathf.Clamp(criticalThreshold, 0, warningThreshold);
    }
}
