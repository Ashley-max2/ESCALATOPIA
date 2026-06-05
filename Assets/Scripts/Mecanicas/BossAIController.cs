using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlador IA del escalador enemigo. Dos modos:
///
///   Scripted  — Sigue waypoints en orden usando la acción adecuada en cada tramo.
///   Autonomous — Va de su posición al Destination evaluando cada frame si andar,
///               escalar o usar gancho. No puede cruzar paredes AntiJefe.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BossAIController : MonoBehaviour
{
    public enum AIMode    { Scripted, Autonomous }
    public enum MoveState { Idle, Walking, Climbing, Hooking }

    // ──────────────────────────────────────────────
    [Header("=== MODO ===")]
    public AIMode mode = AIMode.Autonomous;

    [Header("=== SCRIPTED — waypoints en orden ===")]
    [Tooltip("Lista de puntos que el escalador recorre en orden")]
    public List<Transform> waypoints;
    [Tooltip("¿Vuelve al punto 0 al terminar la lista?")]
    public bool loopWaypoints;

    [Header("=== AUTONOMOUS — de aquí al destino ===")]
    [Tooltip("Punto B al que la IA debe llegar")]
    public Transform destination;

    [Header("=== VELOCIDADES ===")]
    public float walkSpeed  = 4f;
    public float climbSpeed = 2.5f;
    public float hookSpeed  = 12f;

    [Header("=== DETECCIÓN ===")]
    [Tooltip("Distancia a la que detecta paredes escalables delante")]
    public float wallDetectDist    = 1.5f;
    [Tooltip("Radio del SphereCast para detectar paredes")]
    public float wallDetectRadius  = 0.3f;
    [Tooltip("Pendiente máxima caminable (igual que el player: 45°). Por encima → escala.")]
    public float maxWalkableSlope  = 45f;
    [Tooltip("Rango máximo para buscar puntos de gancho (igual que el player: 30m)")]
    public float hookRange        = 30f;
    [Tooltip("Distancia para considerar que llegó a un punto")]
    public float arrivalRadius    = 0.7f;

    [Header("=== CAPAS ===")]
    public LayerMask climbableMask;
    public LayerMask groundMask;
    [Tooltip("Rocas, terreno y muros sólidos que el boss NO puede atravesar al caminar")]
    public LayerMask obstacleMask;
    [Tooltip("Paredes AntiJefe: el boss NO puede atravesarlas (delimitan la carrera)")]
    public LayerMask antiBossWallMask;

    // ──────────────────────────────────────────────
    public MoveState CurrentState { get; private set; }

    private Rigidbody _rb;
    private Animator  _animator;

    // Scripted
    private int _wpIndex;

    // Objetivo de movimiento vigente
    private Transform _moveTarget;

    // Climbing
    private Vector3 _wallNormal;
    private float   _lostWallTimer;

    // Hook
    private Vector3   _hookPoint;
    private Collider  _currentHookCollider;
    private HashSet<Collider> _usedHookColliders = new HashSet<Collider>();

    // Tras usar un hook, ignoramos AntiBoss durante esta gracia para no quedarnos en bucle
    private float _postHookGraceTimer;

    // ──────────────────────────────────────────────
    void Awake()
    {
        _rb       = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();

        // Movimiento manual vía transform.position; el Rigidbody solo se usa para colisiones pasivas
        _rb.isKinematic    = true;
        _rb.freezeRotation = true;
    }

    void OnEnable()
    {
        _wpIndex             = 0;
        _postHookGraceTimer  = 0f;
        _usedHookColliders.Clear();
        _currentHookCollider = null;
        // Forzar re-evaluación del estado aunque ya sea Idle
        CurrentState = MoveState.Hooking; // valor temporal distinto
        SetState(MoveState.Idle);
    }

    void OnDisable() { }

    // ══════════════════════════════════════════════
    // LÓGICA DE IA  (Update — decisiones)
    // ══════════════════════════════════════════════
    void Update()
    {
        if (_postHookGraceTimer > 0f)
            _postHookGraceTimer -= Time.deltaTime;

        if (mode == AIMode.Scripted)
            UpdateScripted();
        else
            UpdateAutonomous();
    }

    // ══════════════════════════════════════════════
    // FÍSICA  (FixedUpdate — movimiento)
    // ══════════════════════════════════════════════
    void FixedUpdate()
    {
        ApplyMovement();
    }

    // ══════════════════════════════════════════════
    // MODO SCRIPTED
    // ══════════════════════════════════════════════
    private void UpdateScripted()
    {
        if (waypoints == null || waypoints.Count == 0) return;
        if (_wpIndex >= waypoints.Count) { SetState(MoveState.Idle); return; }

        _moveTarget = waypoints[_wpIndex];

        // Acaba de activarse o terminar una acción: elige qué hacer para este punto
        if (CurrentState == MoveState.Idle)
        {
            ChooseActionToward(_moveTarget);
            return;
        }

        // Llegó al waypoint → avanzar al siguiente
        if (HasArrived(_moveTarget))
        {
            _wpIndex++;
            if (_wpIndex >= waypoints.Count)
            {
                if (loopWaypoints) _wpIndex = 0;
                else { SetState(MoveState.Idle); return; }
            }
            _moveTarget = waypoints[_wpIndex];
            ChooseActionToward(_moveTarget);
            return;
        }

        // Escalada: actualiza si la pared sigue presente
        HandleClimbingWallLoss();

        // Hook: cuando llega al punto de gancho, intenta escalar de inmediato
        if (CurrentState == MoveState.Hooking && HasArrivedPos(_hookPoint))
            TryStartClimbOrWalk();
    }

    // ══════════════════════════════════════════════
    // MODO AUTONOMOUS
    // ══════════════════════════════════════════════
    private void UpdateAutonomous()
    {
        if (destination == null) return;
        _moveTarget = destination;

        if (HasArrived(destination)) { SetState(MoveState.Idle); return; }

        // Hooking: espera a llegar al punto; al llegar intenta escalar de inmediato
        if (CurrentState == MoveState.Hooking)
        {
            if (HasArrivedPos(_hookPoint)) TryStartClimbOrWalk();
            return;
        }

        // Climbing: solo comprueba pérdida de pared
        if (CurrentState == MoveState.Climbing)
        {
            HandleClimbingWallLoss();
            if (CurrentState == MoveState.Climbing) return;
        }

        // ─ Toma de decisión ─

        Vector3 toTarget = _moveTarget.position - transform.position;
        toTarget.y = 0f;

        // 1. ¿Pared AntiJefe (agua) bloquea el camino directo?
        //    → intentar usar el gancho para sortearla; solo parar si no hay hook accesible.
        //    Ignorado durante gracia post-hook para evitar bucle Idle.
        if (_postHookGraceTimer <= 0f && toTarget.sqrMagnitude > 0.01f && AntiBossBlocks(toTarget.normalized))
        {
            Vector3 hookP;
            if (TryFindHookPoint(_moveTarget.position, out hookP))
            {
                _hookPoint = hookP;
                SetState(MoveState.Hooking);
                return;
            }
            SetState(MoveState.Idle); // sin hook disponible: esperar
            return;
        }

        // 2. ¿Hay pared escalable delante?
        {
            RaycastHit wallHit;
            if (DetectWall(out wallHit))
            {
                _wallNormal    = wallHit.normal;
                _lostWallTimer = 0f;
                SetState(MoveState.Climbing);
                return;
            }
        }

        // 3. ¿Hay un punto de gancho útil?
        {
            Vector3 hookP;
            if (TryFindHookPoint(_moveTarget.position, out hookP))
            {
                _hookPoint = hookP;
                SetState(MoveState.Hooking);
                return;
            }
        }

        // 4. Caminar
        SetState(MoveState.Walking);
    }

    // ──────────────────────────────────────────────
    // Elige entre Walking, Climbing o Hooking para llegar a un punto (Scripted)
    // ──────────────────────────────────────────────
    private void ChooseActionToward(Transform target)
    {
        if (target == null) return;

        float heightDiff = target.position.y - transform.position.y;

        if (heightDiff > 1.5f)
        {
            RaycastHit hit;
            if (DetectWall(out hit))
            {
                _wallNormal    = hit.normal;
                _lostWallTimer = 0f;
                SetState(MoveState.Climbing);
                return;
            }
        }

        Vector3 hookP;
        if (TryFindHookPoint(target.position, out hookP))
        {
            _hookPoint = hookP;
            SetState(MoveState.Hooking);
            return;
        }

        SetState(MoveState.Walking);
    }

    // ══════════════════════════════════════════════
    // MOVIMIENTO FÍSICO  (FixedUpdate)
    // ══════════════════════════════════════════════
    private void ApplyMovement()
    {
        switch (CurrentState)
        {
            case MoveState.Walking:  ApplyWalk();  break;
            case MoveState.Climbing: ApplyClimb(); break;
            case MoveState.Hooking:  ApplyHook();  break;
        }
    }

    private void ApplyWalk()
    {
        if (_moveTarget == null) return;

        Vector3 dir = _moveTarget.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;
        dir.Normalize();

        RotateToward(dir);

        float step = walkSpeed * Time.fixedDeltaTime;

        // No atravesar rocas ni muros sólidos
        if (obstacleMask != 0)
        {
            RaycastHit obstHit;
            if (Physics.SphereCast(transform.position + Vector3.up * 0.5f,
                                   0.3f, dir, out obstHit, step + 0.15f, obstacleMask))
                return;
        }

        transform.position += dir * step;

        // Snap al suelo: busca en groundMask Y en climbableMask (pendientes caminables)
        RaycastHit groundHit;
        LayerMask snapMask = groundMask | climbableMask;
        if (Physics.Raycast(transform.position + Vector3.up * 0.3f,
                            Vector3.down, out groundHit, 4f, snapMask))
        {
            // Solo hacer snap si la pendiente es caminable
            float slopeAngle = Vector3.Angle(Vector3.up, groundHit.normal);
            if (slopeAngle <= maxWalkableSlope)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    Mathf.MoveTowards(transform.position.y, groundHit.point.y, 8f * Time.fixedDeltaTime),
                    transform.position.z);
            }
        }
    }

    private void ApplyClimb()
    {
        Vector3 wallRight = Vector3.Cross(_wallNormal, Vector3.up).normalized;
        Vector3 wallUp    = Vector3.Cross(wallRight, _wallNormal).normalized;

        // Sube siempre recto por la pared; pequeño empuje hacia la pared para mantenerse pegado
        Vector3 move = wallUp * climbSpeed * Time.fixedDeltaTime;
        move -= _wallNormal * 0.05f;
        transform.position += move;

        Vector3 face = -_wallNormal;
        face.y = 0f;
        if (face.sqrMagnitude > 0.01f) RotateToward(face);
    }

    private void ApplyHook()
    {
        transform.position = Vector3.MoveTowards(
            transform.position, _hookPoint, hookSpeed * Time.fixedDeltaTime);

        Vector3 dir     = (_hookPoint - transform.position).normalized;
        Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);
        if (flatDir.sqrMagnitude > 0.01f) RotateToward(flatDir);
    }

    // ══════════════════════════════════════════════
    // DETECCIÓN DE ENTORNO
    // ══════════════════════════════════════════════

    /// <summary>
    /// Detecta una pared escalable en la dirección de movimiento.
    /// Usa el origen retrocedido para no fallar cuando el boss está pegado al collider.
    /// </summary>
    private bool DetectWall(out RaycastHit hit)
    {
        Vector3 baseOrigin = transform.position + Vector3.up * 0.8f;

        Vector3 toTarget = _moveTarget != null
            ? (_moveTarget.position - transform.position)
            : transform.forward;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude > 0.01f) toTarget.Normalize();
        else toTarget = transform.forward;

        // Retroceder el origen 0.5 m en la dirección opuesta para salir del collider
        Vector3[] dirs = { toTarget, transform.forward };
        foreach (Vector3 dir in dirs)
        {
            Vector3 origin = baseOrigin - dir * 0.5f;
            float   dist   = wallDetectDist + 0.5f;

            if (Physics.Raycast(origin, dir, out hit, dist, climbableMask))
            {
                float angle = Vector3.Angle(Vector3.up, hit.normal);
                // Solo es pared si supera la pendiente máxima caminable (igual que el player)
                if (angle > maxWalkableSlope && angle <= 135f) return true;
            }
        }

        // Fallback: OverlapSphere cerca del boss (detecta paredes que ya toca)
        Collider[] cols = Physics.OverlapSphere(baseOrigin, wallDetectDist * 0.6f, climbableMask);
        foreach (Collider col in cols)
        {
            Vector3 toUs    = (baseOrigin - col.bounds.center).normalized;
            Vector3 fromCol = col.bounds.center + toUs * 0.1f;
            if (Physics.Raycast(fromCol, -toUs, out hit, col.bounds.size.magnitude + 1f, climbableMask))
            {
                float angle = Vector3.Angle(Vector3.up, hit.normal);
                if (angle > maxWalkableSlope && angle <= 135f) return true;
            }
        }

        hit = default;
        return false;
    }

    private bool TryFindHookPoint(Vector3 targetPos, out Vector3 hookPoint)
    {
        hookPoint = Vector3.zero;
        Collider[] nearby = Physics.OverlapSphere(transform.position, hookRange);

        float    bestGain   = 0f;
        bool     found      = false;
        Collider bestCollider = null;

        foreach (Collider col in nearby)
        {
            if (!col.CompareTag("HookPoint")) continue;

            // No reutilizar hook points ya visitados
            if (_usedHookColliders.Contains(col)) continue;

            Vector3 hp       = col.transform.position;
            float   distNow  = Vector3.Distance(transform.position, targetPos);
            float   distPost = Vector3.Distance(hp, targetPos);
            float   gain     = distNow - distPost;

            if (gain < 2f) continue;

            // No disparar el gancho a través del agua (AntiJefe)
            Vector3 dir = (hp - transform.position).normalized;
            float   len = Vector3.Distance(transform.position, hp);
            if (antiBossWallMask != 0 &&
                Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, len, antiBossWallMask)) continue;

            if (gain > bestGain)
            {
                bestGain      = gain;
                hookPoint     = hp;
                bestCollider  = col;
                found         = true;
            }
        }

        if (found)
            _currentHookCollider = bestCollider;

        return found;
    }

    private bool AntiBossBlocks(Vector3 direction)
    {
        if (antiBossWallMask == 0) return false;
        return Physics.Raycast(
            transform.position + Vector3.up * 0.5f,
            direction,
            wallDetectDist * 1.5f,
            antiBossWallMask);
    }

    private void HandleClimbingWallLoss()
    {
        if (CurrentState != MoveState.Climbing) return;

        // Al escalar, miramos hacia la pared (forward ≈ -wallNormal).
        // Retrocedemos 0.6 m para no empezar dentro del collider.
        Vector3 origin = transform.position + Vector3.up * 0.8f - transform.forward * 0.6f;

        RaycastHit hit;
        bool wallPresent = Physics.Raycast(origin, transform.forward, out hit, wallDetectDist + 0.6f, climbableMask)
                           && Vector3.Angle(Vector3.up, hit.normal) > maxWalkableSlope
                           && Vector3.Angle(Vector3.up, hit.normal) <= 135f;

        if (!wallPresent)
        {
            _lostWallTimer += Time.deltaTime;
            if (_lostWallTimer > 0.5f)
                SetState(MoveState.Walking);
        }
        else
        {
            _wallNormal    = hit.normal;
            _lostWallTimer = 0f;
        }
    }

    // ══════════════════════════════════════════════
    // UTILIDADES
    // ══════════════════════════════════════════════

    /// <summary>
    /// Tras llegar al destino del hook: marca el hook como usado, detiene el Rigidbody
    /// para desenganchar limpiamente y decide si escalar o caminar.
    /// </summary>
    private void TryStartClimbOrWalk()
    {
        // Marcar el hook point como ya usado (no se reutilizará en esta carrera)
        if (_currentHookCollider != null)
        {
            _usedHookColliders.Add(_currentHookCollider);
            _currentHookCollider = null;
        }

        // Gracia: ignorar AntiBoss 2 s para que el boss se aleje del agua antes de re-evaluar
        _postHookGraceTimer = 2f;

        RaycastHit hit;
        if (DetectWall(out hit))
        {
            _wallNormal    = hit.normal;
            _lostWallTimer = 0f;
            SetState(MoveState.Climbing);
        }
        else
        {
            SetState(MoveState.Walking);
        }
    }

    private bool HasArrived(Transform t) =>
        t != null && Vector3.Distance(transform.position, t.position) <= arrivalRadius;

    private bool HasArrivedPos(Vector3 pos) =>
        Vector3.Distance(transform.position, pos) <= arrivalRadius;

    private void RotateToward(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.01f) return;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            Time.deltaTime * 12f);
    }

    private void SetState(MoveState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
        Debug.Log($"[BossAI] {gameObject.name}: {newState}");
    }

    // ══════════════════════════════════════════════
    // GIZMOS
    // ══════════════════════════════════════════════
    void OnDrawGizmosSelected()
    {
        if (_moveTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _moveTarget.position);
            Gizmos.DrawWireSphere(_moveTarget.position, arrivalRadius);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, wallDetectRadius);

        if (CurrentState == MoveState.Hooking)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, _hookPoint);
            Gizmos.DrawWireSphere(_hookPoint, 0.4f);
        }

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, hookRange);
    }
}
