using UnityEngine;

/// <summary>
/// Física de pista de esquí.
/// Añade este componente al jugador (personajePrincipal).
/// Cuando el jugador está sobre la capa "SkiSlope", se activa el modo esquí:
///   - Baja deslizándose por la pendiente con aceleración y velocidad máxima.
///   - El jugador puede esterear izquierda/derecha con el eje horizontal de input.
///   - Se desactiva al salir del terreno de esquí.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SkiSlope : MonoBehaviour
{
    [Header("=== SKI PHYSICS ===")]
    [Tooltip("Fuerza de deslizamiento a lo largo de la pendiente")]
    [SerializeField] private float slideForce = 18f;

    [Tooltip("Velocidad máxima al deslizarse")]
    [SerializeField] private float maxSlideSpeed = 22f;

    [Tooltip("Control lateral del jugador al esquiar")]
    [SerializeField] private float steerForce = 8f;

    [Tooltip("Fricción al esquiar (0 = hielo puro)")]
    [SerializeField] private float skiFriction = 0.05f;

    [Tooltip("Tag del terreno de esquí")]
    [SerializeField] private string skiSlopeTag = "SkiSlope";

    [Tooltip("Distancia del raycast hacia abajo para detectar la pendiente")]
    [SerializeField] private float groundRayDist = 1.5f;

    [Header("=== DEBUG ===")]
    [SerializeField] private bool showDebugInfo = true;

    // ─── Estado interno ───────────────────────────────────────────────
    private Rigidbody _rb;
    private bool _isSki;
    private Vector3 _slopeNormal = Vector3.up;
    private int _skiContactCount;

    // Referencia al PlayerStateMachine (opcional, para deshabilitar su control)
    private PlayerStateMachine _psm;

    // ─── Lifecycle ────────────────────────────────────────────────────
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _psm = GetComponent<PlayerStateMachine>();
    }

    private void FixedUpdate()
    {
        if (!_isSki) return;

        UpdateSlopeNormal();
        ApplySlide();
        ApplySteer();
        ClampSpeed();
    }

    // ─── Detección de contacto ────────────────────────────────────────
    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag(skiSlopeTag))
        {
            _skiContactCount++;
            ActivateSki();
        }
    }

    private void OnCollisionExit(Collision col)
    {
        if (col.gameObject.CompareTag(skiSlopeTag))
        {
            _skiContactCount = Mathf.Max(0, _skiContactCount - 1);
            if (_skiContactCount == 0) DeactivateSki();
        }
    }

    // ─── Activar / Desactivar modo esquí ─────────────────────────────
    private void ActivateSki()
    {
        if (_isSki) return;
        _isSki = true;

        // Reducir la fricción del rigidbody
        _rb.drag = skiFriction;
        _rb.angularDrag = 0.05f;

        if (showDebugInfo) Debug.Log("[SkiSlope] Modo esquí ACTIVADO");
    }

    private void DeactivateSki()
    {
        if (!_isSki) return;
        _isSki = false;

        // Restaurar físicas normales
        _rb.drag = 0f;
        _rb.angularDrag = 0.05f;

        if (showDebugInfo) Debug.Log("[SkiSlope] Modo esquí DESACTIVADO");
    }

    // ─── Física de pendiente ──────────────────────────────────────────

    /// <summary>
    /// Raycast hacia el suelo para obtener la normal de la pendiente actual.
    /// </summary>
    private void UpdateSlopeNormal()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f,
                            Vector3.down, out RaycastHit hit, groundRayDist))
        {
            _slopeNormal = hit.normal;
        }
        else
        {
            _slopeNormal = Vector3.up;
        }
    }

    /// <summary>
    /// Aplica fuerza a lo largo de la dirección de máxima pendiente (hacia abajo).
    /// </summary>
    private void ApplySlide()
    {
        // Proyecta la gravedad sobre el plano de la pendiente
        Vector3 gravity = Physics.gravity;
        Vector3 slideDir = Vector3.ProjectOnPlane(gravity, _slopeNormal).normalized;

        if (slideDir.sqrMagnitude < 0.001f) return; // terreno plano, no empujar

        _rb.AddForce(slideDir * slideForce, ForceMode.Acceleration);
    }

    /// <summary>
    /// Permite al jugador girar izquierda/derecha mientras baja.
    /// </summary>
    private void ApplySteer()
    {
        float horizontal = Input.GetAxis("Horizontal");
        if (Mathf.Abs(horizontal) < 0.01f) return;

        // El eje de dirección lateral es perpendicular a la pendiente y al "abajo"
        Vector3 downSlope = Vector3.ProjectOnPlane(Vector3.down, _slopeNormal).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, downSlope).normalized;

        _rb.AddForce(right * horizontal * steerForce, ForceMode.Acceleration);

        // Rotar el modelo hacia la dirección de movimiento
        Vector3 vel = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        if (vel.sqrMagnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(vel.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 8f * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// Limita la velocidad horizontal máxima al esquiar.
    /// </summary>
    private void ClampSpeed()
    {
        Vector3 flatVel = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        if (flatVel.magnitude > maxSlideSpeed)
        {
            Vector3 clamped = flatVel.normalized * maxSlideSpeed;
            _rb.velocity = new Vector3(clamped.x, _rb.velocity.y, clamped.z);
        }
    }

    // ─── Gizmos ───────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (!_isSki) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, Vector3.ProjectOnPlane(Vector3.down, _slopeNormal) * 2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, _slopeNormal);
    }

    private void OnGUI()
    {
        if (!showDebugInfo || !_isSki) return;
        GUILayout.BeginArea(new Rect(10, 220, 280, 60));
        GUILayout.Label($"[SKI] Velocidad: {_rb.velocity.magnitude:F1} m/s");
        GUILayout.Label($"[SKI] Normal pendiente: {_slopeNormal:F2}");
        GUILayout.EndArea();
    }
}
