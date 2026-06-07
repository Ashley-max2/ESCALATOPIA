using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controlador del boss levitante. Ahora delega el movimiento en BossGroundAI (GOAP).
/// Se activa desde BossManager cuando empieza la carrera.
/// </summary>
public class BossLevitante : MonoBehaviour
{
    [Header("── Referencias ──")]
    [Tooltip("IA GOAP que controla el movimiento del boss (correr, saltar, escalar).")]
    public BossGroundAI bossGroundAI;

    [Tooltip("Lanzador de rocas opcional que se activa junto al boss.")]
    public BossRockThrower bossRockThrower;

    [Header("── Eventos ──")]
    public UnityEvent onActivated;
    public UnityEvent onDeactivated;

    private bool      _isActive       = false;
    private Vector3   _initialPosition;

    private void Awake()
    {
        _initialPosition = transform.position;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  API PÚBLICA
    // ─────────────────────────────────────────────────────────────────────────

    public void Activate()
    {
        if (_isActive) return;
        _isActive = true;

        if (bossGroundAI != null)
            bossGroundAI.Activate();

        if (bossRockThrower != null)
            bossRockThrower.Activate();

        onActivated?.Invoke();
        Debug.Log("[BossLevitante] Activado → BossGroundAI corriendo.");
    }

    public void Deactivate()
    {
        _isActive = false;

        if (bossGroundAI != null)
            bossGroundAI.Deactivate();

        if (bossRockThrower != null)
            bossRockThrower.Deactivate();

        onDeactivated?.Invoke();
    }

    public void ResetBoss()
    {
        Deactivate();

        if (bossGroundAI != null)
            bossGroundAI.ResetToPosition(_initialPosition);
        else
            transform.position = _initialPosition;

        Debug.Log("[BossLevitante] Reseteado a posición inicial.");
    }
}
