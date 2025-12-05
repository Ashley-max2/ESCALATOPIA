using UnityEngine;

/// <summary>
/// Controla la máquina de estados Idle/Walk (2 estados)
/// Detecta automáticamente si el personaje se está moviendo
/// </summary>
[RequireComponent(typeof(Animator))]
public class IdleWalkAnimController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rb;

    [Header("Configuración")]
    [Tooltip("Velocidad mínima para considerar que está caminando")]
    [SerializeField] private float movementThreshold = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool showDebug = false;

    private void Awake()
    {
        // Obtener componentes si no están asignados
        if (animator == null)
            animator = GetComponent<Animator>();

        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        UpdateMovementState();
    }

    /// <summary>
    /// Actualiza el parámetro IsMoving según la velocidad del Rigidbody
    /// </summary>
    private void UpdateMovementState()
    {
        if (rb == null || animator == null) return;

        // Calcular velocidad horizontal (ignorar Y para evitar que saltos afecten)
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        float speed = horizontalVelocity.magnitude;

        // Determinar si está moviéndose
        bool isMoving = speed > movementThreshold;

        // Actualizar animator
        animator.SetBool("IsMoving", isMoving);

        // Opcional: también actualizar Speed como float para blend trees futuros
        animator.SetFloat("Speed", speed);

        if (showDebug)
        {
            Debug.Log($"[IdleWalk] Speed: {speed:F2} | IsMoving: {isMoving}");
        }
    }

    /// <summary>
    /// Método público para forzar estado (útil para debugging o eventos especiales)
    /// </summary>
    public void ForceIdleState()
    {
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetFloat("Speed", 0f);
        }
    }

    /// <summary>
    /// Método público para forzar estado walk
    /// </summary>
    public void ForceWalkState()
    {
        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
        }
    }

    private void OnValidate()
    {
        // Auto-asignar referencias en el editor
        if (animator == null)
            animator = GetComponent<Animator>();

        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }
}
