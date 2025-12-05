using UnityEngine;

/// <summary>
/// Controla la máquina de estados Hooked/Free (2 estados)
/// Para animaciones de gancho/impulso
/// </summary>
[RequireComponent(typeof(Animator))]
public class HookedFreeAnimController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;

    [Header("Estado Actual")]
    [SerializeField] private bool isHooked = false;

    [Header("Configuración Opcional")]
    [Tooltip("Tiempo mínimo en estado Hooked antes de poder volver a Free")]
    [SerializeField] private float minHookDuration = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool showDebug = false;

    private float hookStartTime = 0f;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Llamar cuando el gancho se conecta a un punto
    /// </summary>
    public void OnHookConnected()
    {
        isHooked = true;
        hookStartTime = Time.time;

        if (animator != null)
        {
            animator.SetBool("IsHooked", true);
        }

        if (showDebug)
        {
            Debug.Log($"[HookedFree] Hook CONNECTED at {Time.time:F2}");
        }
    }

    /// <summary>
    /// Llamar cuando el gancho se suelta o se completa el impulso
    /// </summary>
    public void OnHookReleased()
    {
        // Verificar duración mínima para evitar transiciones muy rápidas
        float timeSinceHook = Time.time - hookStartTime;
        
        if (timeSinceHook < minHookDuration && isHooked)
        {
            // Esperar un frame más
            Invoke(nameof(CompleteRelease), minHookDuration - timeSinceHook);
            return;
        }

        CompleteRelease();
    }

    /// <summary>
    /// Completa la liberación del gancho
    /// </summary>
    private void CompleteRelease()
    {
        isHooked = false;

        if (animator != null)
        {
            animator.SetBool("IsHooked", false);
        }

        if (showDebug)
        {
            Debug.Log($"[HookedFree] Hook RELEASED at {Time.time:F2}");
        }
    }

    /// <summary>
    /// Forzar estado Free (útil para reset o debugging)
    /// </summary>
    public void ForceFreeState()
    {
        isHooked = false;

        if (animator != null)
        {
            animator.SetBool("IsHooked", false);
        }

        // Cancelar cualquier invoke pendiente
        CancelInvoke(nameof(CompleteRelease));
    }

    /// <summary>
    /// Verificar si está actualmente enganchado
    /// </summary>
    public bool IsHooked()
    {
        return isHooked;
    }

    /// <summary>
    /// Integración con sistema de gancho existente (ejemplo)
    /// Conecta este método al evento de tu HookSystem
    /// </summary>
    public void OnHookStateChanged(bool hooked)
    {
        if (hooked)
        {
            OnHookConnected();
        }
        else
        {
            OnHookReleased();
        }
    }

    private void OnValidate()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void OnDisable()
    {
        // Limpiar invokes al desactivar
        CancelInvoke();
    }
}
