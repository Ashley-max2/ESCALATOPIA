using UnityEngine;

/// <summary>
/// Controla la máquina de estados Climbing/Hanging (2 estados)
/// Para personajes que escalan paredes
/// </summary>
[RequireComponent(typeof(Animator))]
public class ClimbingHangingAnimController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;

    [Header("Configuración")]
    [Tooltip("Velocidad de escalada para considerar que está subiendo")]
    [SerializeField] private float climbingSpeedThreshold = 0.1f;

    [Header("Estado Actual")]
    [SerializeField] private bool isOnWall = false;
    [SerializeField] private float currentClimbSpeed = 0f;

    [Header("Debug")]
    [SerializeField] private bool showDebug = false;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Update()
    {
        UpdateClimbingState();
    }

    /// <summary>
    /// Actualiza los parámetros de la máquina de estados
    /// </summary>
    private void UpdateClimbingState()
    {
        if (animator == null) return;

        // IsClimbing: true si está escalando activamente, false si está colgado sin moverse
        bool isClimbing = isOnWall && Mathf.Abs(currentClimbSpeed) > climbingSpeedThreshold;

        animator.SetBool("IsClimbing", isClimbing);
        animator.SetFloat("ClimbSpeed", currentClimbSpeed);

        if (showDebug)
        {
            Debug.Log($"[ClimbingHanging] IsOnWall: {isOnWall} | IsClimbing: {isClimbing} | Speed: {currentClimbSpeed:F2}");
        }
    }

    /// <summary>
    /// Llamar cuando el personaje toca una pared escalable
    /// </summary>
    public void StartClimbing()
    {
        isOnWall = true;
        if (showDebug) Debug.Log("[ClimbingHanging] Started climbing");
    }

    /// <summary>
    /// Llamar cuando el personaje se suelta de la pared
    /// </summary>
    public void StopClimbing()
    {
        isOnWall = false;
        currentClimbSpeed = 0f;
        
        if (animator != null)
        {
            animator.SetBool("IsClimbing", false);
            animator.SetFloat("ClimbSpeed", 0f);
        }

        if (showDebug) Debug.Log("[ClimbingHanging] Stopped climbing");
    }

    /// <summary>
    /// Actualizar la velocidad de escalada (desde input o física)
    /// </summary>
    /// <param name="speed">Velocidad vertical (positiva = subir, negativa = bajar)</param>
    public void SetClimbingSpeed(float speed)
    {
        currentClimbSpeed = speed;
    }

    /// <summary>
    /// Forzar estado Hanging (colgado sin moverse)
    /// </summary>
    public void ForceHangingState()
    {
        isOnWall = true;
        currentClimbSpeed = 0f;

        if (animator != null)
        {
            animator.SetBool("IsClimbing", false);
            animator.SetFloat("ClimbSpeed", 0f);
        }
    }

    /// <summary>
    /// Verificar si está actualmente en pared
    /// </summary>
    public bool IsOnWall()
    {
        return isOnWall;
    }

    private void OnValidate()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }
}
