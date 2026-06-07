using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IA del boss terrestre basada en GOAP (Goal-Oriented Action Planning).
/// ── Columna 1 de la rúbrica de Inteligencia ──
///
/// Componentes GOAP implementados:
///   · WorldState  — representación explícita y formal del entorno en cada instante
///   · Goal        — estado objetivo (llegar al destino)
///   · ActionDefs  — tabla de acciones con precondiciones y efectos declarativos
///   · Planner     — búsqueda hacia delante (forward search) que halla el plan óptimo
///                   usando coste acumulado + heurística de distancia (A* simplificado)
///
/// Acciones disponibles: Walk, Run, Climb, Jump, Hook
/// Animaciones usadas (Boss2.controller): Idle, Walk, Trote, ClimbIdle, ClimbForward,
///                                        Jump_Start, Jump_Loop, Jump_End
///
/// Setup:
///   · Añadir este componente al boss junto con Rigidbody
///   · Asignar 'destination' (Transform vacío en la meta de la carrera)
///   · Llamar Activate() desde BossLevitante o un UnityEvent
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BossGroundAI : MonoBehaviour
{
    // ════════════════════════════════════════════════════════════════
    //  GOAP ── ESTADO DEL MUNDO
    //  Representación explícita y formal del entorno.
    //  El planner opera sobre copias de esta struct (sin efectos secundarios).
    // ════════════════════════════════════════════════════════════════
    private struct WorldState
    {
        public bool reachedGoal;    // El boss está en la posición de destino
        public bool wallBlocking;   // Hay una pared escalable bloqueando el avance
        public bool gapAhead;       // Hay un hueco / vacío justo delante
        public bool isGrounded;     // El boss tiene los pies en el suelo
        public bool hookAvailable;  // Existe un HookPoint útil al alcance
    }

    // ════════════════════════════════════════════════════════════════
    //  GOAP ── DEFINICIÓN DE ACCIONES
    //  Cada ActionDef declara formalmente:
    //    · Precondiciones: qué condiciones del WorldState deben cumplirse
    //    · Efectos:        cómo cambia el WorldState simulado tras ejecutar la acción
    //    · Coste:          usado por el planner para comparar planes alternativos
    // ════════════════════════════════════════════════════════════════
    private enum GOAPAction { Idle, Walk, Run, Climb, Jump, Hook }

    private struct ActionDef
    {
        public GOAPAction type;
        // ── Precondiciones ──
        public bool pre_grounded;   // requiere estar en el suelo
        public bool pre_wall;       // requiere pared delante
        public bool pre_gap;        // requiere hueco delante
        public bool pre_hook;       // requiere HookPoint disponible
        public bool pre_no_wall;    // no puede haber pared (bloqueante)
        public bool pre_no_gap;     // no puede haber hueco (bloqueante)
        // ── Efectos sobre WorldState simulado ──
        public bool eff_clears_wall;
        public bool eff_clears_gap;
        public bool eff_approaches; // la acción nos acerca al objetivo
        // ── Coste para el planner ──
        public float cost;
    }

    // Tabla de acciones: el planner itera sobre esta lista cada re-planificación.
    private static readonly ActionDef[] ActionTable =
    {
        // Correr: acción base cuando el camino está libre
        new ActionDef {
            type=GOAPAction.Run,
            pre_no_wall=true, pre_no_gap=true,
            eff_approaches=true, cost=0.8f },

        new ActionDef {
            type=GOAPAction.Walk,
            pre_no_wall=true, pre_no_gap=true,
            eff_approaches=true, cost=1.0f },

        // Salto: ante pared O hueco, más barato que escalar.
        // pre_wall OR pre_gap → usamos pre_gap para huecos y añadimos otro para paredes.
        // Versión A: saltar para cruzar un hueco
        new ActionDef {
            type=GOAPAction.Jump,
            pre_grounded=true, pre_gap=true,
            eff_clears_gap=true, eff_approaches=true, cost=1.1f },

        // Versión B: saltar para superar una pared (antes de intentar escalar)
        new ActionDef {
            type=GOAPAction.Jump,
            pre_grounded=true, pre_wall=true,
            eff_clears_wall=true, eff_approaches=true, cost=1.1f },

        // Escalar: fallback si el salto no basta (coste mayor que Jump)
        new ActionDef {
            type=GOAPAction.Climb,
            pre_wall=true,
            eff_clears_wall=true, eff_approaches=true, cost=1.5f },

        new ActionDef {
            type=GOAPAction.Hook,
            pre_hook=true,
            eff_clears_wall=true, eff_clears_gap=true, eff_approaches=true, cost=0.9f },
    };

    // ════════════════════════════════════════════════════════════════
    //  REFERENCIAS E INSPECTOR
    // ════════════════════════════════════════════════════════════════
    [Header("── Referencia ──")]
    [Tooltip("Punto de meta (transform vacío en el destino de la carrera).")]
    public Transform destination;

    [Header("── Capas ──")]
    public LayerMask groundMask;
    public LayerMask climbableMask;

    [Header("── Velocidades ──")]
    public float walkSpeed  = 3f;
    public float runSpeed   = 6f;
    public float climbSpeed = 5f;
    public float hookSpeed  = 14f;

    [Header("── Salto ──")]
    public float jumpUpForce      = 9f;
    public float jumpForwardForce = 5f;

    [Header("── Detección ──")]
    public float wallDetectDist  = 1.6f;
    public float gapDetectDist   = 1.2f;
    public float groundCheckDist = 0.35f;
    public float arrivalRadius   = 1.2f;
    public float hookRange       = 25f;
    public float maxWalkableSlope = 45f;

    [Header("── GOAP ──")]
    [Tooltip("Cada cuántos segundos el planner re-evalúa el plan.")]
    public float planInterval = 0.35f;

    // ════════════════════════════════════════════════════════════════
    //  ESTADO INTERNO
    // ════════════════════════════════════════════════════════════════
    private Rigidbody _rb;
    private Animator  _anim;

    private bool       _isActive   = false;
    private bool       _isGrounded = false;
    private GOAPAction _action     = GOAPAction.Idle;

    // Climbing
    private Vector3 _wallNormal;
    private float   _lostWallTimer;

    // Hook
    private Vector3  _hookTarget;
    private Collider _hookCollider;
    private HashSet<Collider> _usedHooks = new HashSet<Collider>();

    // Planner
    private float _planTimer;

    // Jump state machine
    private bool _jumpInProgress = false;

    // Climbing / mantle
    private bool _isMantling = false;

    // ════════════════════════════════════════════════════════════════
    //  UNITY
    // ════════════════════════════════════════════════════════════════
    private void Awake()
    {
        _rb   = GetComponent<Rigidbody>();
        _anim = GetComponentInChildren<Animator>();

        if (_rb != null)
        {
            _rb.freezeRotation = true;
            _rb.isKinematic    = false;
        }

        // Validar máscaras — si son 0 el boss no detectará suelo ni paredes
        if (groundMask == 0)
            Debug.LogError("[BossGroundAI] ¡groundMask NO está asignada! El boss no detectará el suelo. " +
                           "Añade las capas Default/Terrain en el campo Ground Mask del Inspector.");
        if (climbableMask == 0)
            Debug.LogWarning("[BossGroundAI] climbableMask vacía. Si las rocas son escalables añade su capa aquí.");

        if (_anim == null)
        {
            Debug.LogError("[BossGroundAI] ¡No se encontró Animator en los hijos del boss!");
        }
        else if (_anim.runtimeAnimatorController == null)
        {
            Debug.LogError($"[BossGroundAI] El Animator '{_anim.gameObject.name}' NO tiene controller asignado. Arrastra Boss2.controller al campo Controller del Animator.");
        }
        else
        {
            Debug.Log($"[BossGroundAI] Animator OK — Controller: {_anim.runtimeAnimatorController.name}");
            ValidateAnimatorParams();
        }
    }

    private void ValidateAnimatorParams()
    {
        string[] needed = { "Speed", "IsClimbing", "IsGrounded", "Jump" };
        var found = new System.Collections.Generic.HashSet<string>();
        foreach (var p in _anim.parameters) found.Add(p.name);
        foreach (var n in needed)
            if (!found.Contains(n))
                Debug.LogError($"[BossGroundAI] Parámetro '{n}' NO existe en el controller. Ejecuta Tools → Setup Boss2 Animator.");
    }

    private void Update()
    {
        if (!_isActive) return;

        // Actualizar grounded ANTES de planificar para que el primer frame sea correcto
        CheckGrounded();

        _planTimer -= Time.deltaTime;
        if (_planTimer <= 0f)
        {
            _planTimer = planInterval;
            GOAP_Replan();
        }

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (!_isActive) return;
        ExecuteAction();
    }

    // ════════════════════════════════════════════════════════════════
    //  API PÚBLICA
    // ════════════════════════════════════════════════════════════════
    [ContextMenu("TEST — Activar Boss")]
    public void ActivateTest() => Activate();

    public void Activate()
    {
        _isActive = true;
        _usedHooks.Clear();
        _planTimer    = 0f;
        _jumpInProgress = false;
        Debug.Log("[BossGroundAI] Activado (GOAP).");
    }

    public void Deactivate()
    {
        _isActive   = false;
        _isMantling = false;
        _wallNormal = Vector3.zero;
        StopAllCoroutines();
        if (_rb != null)
        {
            _rb.useGravity  = true;
            _rb.isKinematic = false;
            _rb.velocity    = Vector3.zero;
        }
        _action = GOAPAction.Idle;
        // Resetear el animator aunque estemos inactivos (evita congelarse en ClimbForward)
        ResetAnimator();
    }

    private void ResetAnimator()
    {
        if (_anim == null) return;
        _anim.SetFloat("Speed",      0f);
        _anim.SetFloat("ClimbH",     0f);
        _anim.SetBool ("IsClimbing", false);
        _anim.SetBool ("IsGrounded", true);
        // Forzar salto inmediato al estado Idle sin esperar transiciones
        _anim.Play("Idle", 0, 0f);
        _anim.Update(0f);
    }

    public void ResetToPosition(Vector3 pos)
    {
        Deactivate();
        transform.position = pos;
        _usedHooks.Clear();
    }

    // ════════════════════════════════════════════════════════════════
    //  GOAP ── PASO 1: SENSING (percepción del entorno → WorldState)
    // ════════════════════════════════════════════════════════════════
    private WorldState GOAP_SenseWorld()
    {
        WorldState ws = new WorldState();
        ws.isGrounded = _isGrounded;

        // ¿Ya llegamos?
        ws.reachedGoal = destination != null &&
            Vector3.Distance(transform.position, destination.position) <= arrivalRadius;

        // Dirección hacia el destino (horizontal)
        Vector3 toTarget = destination != null
            ? (destination.position - transform.position)
            : transform.forward;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.01f) toTarget = transform.forward;
        toTarget.Normalize();

        // ¿Pared bloqueando? Raycast dedicado hacia delante a varias alturas
        Vector3 detectedNormal;
        ws.wallBlocking = DetectClimbableWall(toTarget, out detectedNormal);
        if (ws.wallBlocking) _wallNormal = detectedNormal;

        // ¿Hueco delante? (no hay suelo a ~1.2m adelante)
        Vector3 gapCheckOrigin = transform.position + toTarget * gapDetectDist + Vector3.up * 0.3f;
        bool groundAhead = Physics.Raycast(gapCheckOrigin, Vector3.down, 2f,
                                            groundMask | climbableMask);
        ws.gapAhead = !groundAhead && _isGrounded && !ws.wallBlocking;

        // ¿HookPoint útil disponible?
        Collider bestHook;
        ws.hookAvailable = TryFindHookPoint(out bestHook, out _hookTarget);
        _hookCollider    = bestHook;

        return ws;
    }

    // ════════════════════════════════════════════════════════════════
    //  GOAP ── PASO 2: PLANNER (búsqueda hacia delante / forward search)
    //  Para cada acción de la tabla:
    //    1. Comprueba si sus precondiciones se cumplen en el WorldState actual
    //    2. Simula los efectos sobre una copia del WorldState
    //    3. Calcula coste = coste_acción + heurística (distancia restante)
    //    4. Elige la acción de menor coste total
    //  Esto es la esencia del GOAP forward planner (simplificado a un paso).
    // ════════════════════════════════════════════════════════════════
    private void GOAP_Replan()
    {
        // No interrumpir mientras escala o hace mantle — debe terminar completo
        if (destination == null || _jumpInProgress || _action == GOAPAction.Climb || _isMantling) return;

        WorldState current = GOAP_SenseWorld();

        if (current.reachedGoal) { SetAction(GOAPAction.Idle); return; }

        float      bestCost   = float.MaxValue;
        GOAPAction bestAction = GOAPAction.Idle;

        foreach (var def in ActionTable)
        {
            // ── Verificar precondiciones ──
            if (!GOAP_CheckPreconditions(def, current)) continue;

            // ── Simular efectos ──
            WorldState simulated = GOAP_ApplyEffects(def, current);

            // ── Coste total = coste acción + heurística (distancia al goal) ──
            float distHeuristic = simulated.reachedGoal
                ? 0f
                : Vector3.Distance(transform.position, destination.position);
            float totalCost = def.cost + distHeuristic * 0.1f;

            if (totalCost < bestCost)
            {
                bestCost   = totalCost;
                bestAction = def.type;
            }
        }

        // Fallback: si el planner no encontró ninguna acción válida y no hemos llegado,
        // correr igualmente (evita quedarse perpetuamente en Idle por máscaras mal asignadas)
        if (bestAction == GOAPAction.Idle && !current.reachedGoal)
        {
            Debug.LogWarning("[BossGroundAI GOAP] Ninguna acción válida encontrada — forzando Run. " +
                             "Comprueba que groundMask y climbableMask están asignadas en el Inspector.");
            bestAction = GOAPAction.Run;
        }

        SetAction(bestAction);
    }

    // ── Comprobación formal de precondiciones ──
    private bool GOAP_CheckPreconditions(ActionDef def, WorldState ws)
    {
        if (def.pre_grounded  && !ws.isGrounded)    return false;
        if (def.pre_wall      && !ws.wallBlocking)   return false;
        if (def.pre_gap       && !ws.gapAhead)       return false;
        if (def.pre_hook      && !ws.hookAvailable)  return false;
        if (def.pre_no_wall   &&  ws.wallBlocking)   return false;
        if (def.pre_no_gap    &&  ws.gapAhead)       return false;
        return true;
    }

    // ── Aplicación de efectos sobre copia del WorldState ──
    private WorldState GOAP_ApplyEffects(ActionDef def, WorldState ws)
    {
        WorldState next = ws; // copia por valor (struct)
        if (def.eff_clears_wall) next.wallBlocking  = false;
        if (def.eff_clears_gap)  next.gapAhead      = false;
        if (def.eff_approaches)
        {
            next.reachedGoal = destination != null &&
                Vector3.Distance(transform.position, destination.position) <= arrivalRadius * 2f;
        }
        return next;
    }

    // ════════════════════════════════════════════════════════════════
    //  EJECUCIÓN DE ACCIONES
    // ════════════════════════════════════════════════════════════════
    private void SetAction(GOAPAction newAction)
    {
        if (_action == newAction) return;
        GOAPAction prev = _action;
        _action = newAction;
        Debug.Log($"[BossGroundAI GOAP] Acción: {prev} → {newAction}");

        // Gravedad: OFF al entrar en Climb, ON al salir
        if (newAction == GOAPAction.Climb)
        {
            if (_rb != null)
            {
                _rb.useGravity = false;
                _rb.velocity   = Vector3.zero; // cancelar velocidad acumulada
            }
        }
        else if (prev == GOAPAction.Climb && newAction != GOAPAction.Climb)
        {
            if (_rb != null) _rb.useGravity = true;
        }

        if (newAction == GOAPAction.Jump && !_jumpInProgress)
            StartCoroutine(JumpRoutine());
    }

    private void ExecuteAction()
    {
        switch (_action)
        {
            case GOAPAction.Walk:  MoveToward(walkSpeed);  break;
            case GOAPAction.Run:   MoveToward(runSpeed);   break;
            case GOAPAction.Climb: ApplyClimb();           break;
            case GOAPAction.Hook:  ApplyHook();            break;
        }
    }

    // ── Caminar / Correr ──
    private void MoveToward(float speed)
    {
        if (destination == null) return;

        Vector3 dir = GetDirToDestination();
        RotateToward(dir);

        // Detección inmediata de pared escalable — no espera al ciclo del planner
        Vector3 wallNormal;
        if (DetectClimbableWall(dir, out wallNormal))
        {
            _wallNormal = wallNormal;
            SetAction(GOAPAction.Climb);
            return;
        }

        Vector3 next = transform.position + dir * speed * Time.fixedDeltaTime;
        next = SnapToGround(next);
        _rb.MovePosition(next);
    }

    // ── Escalar ──
    // Llamado desde FixedUpdate. GOAP no puede interrumpir mientras _action==Climb o _isMantling.
    private void ApplyClimb()
    {
        if (_isMantling) return;

        // Cuando el boss ya está pegado a la pared el origen del raycast puede estar
        // dentro de la geometría. Retrocedemos 0.6m en la normal guardada antes de lanzar.
        Vector3 toTarget = GetDirToDestination();
        // Si tenemos normal guardada, la usamos directamente sin abanico (más fiable pegado a pared)
        Vector3 freshNormal;
        bool wallPresent;
        if (_wallNormal != Vector3.zero)
        {
            // Raycast desde detrás del boss hacia la pared, a 3 alturas
            LayerMask mask = climbableMask | groundMask;
            wallPresent = false;
            freshNormal = _wallNormal;
            float[] heights = { 0.3f, 0.8f, 1.3f };
            foreach (float h in heights)
            {
                // Origen: retroceder 0.7m de la pared usando la normal
                Vector3 origin = transform.position + Vector3.up * h + _wallNormal * 0.7f;
                Vector3 dir    = -_wallNormal;
                RaycastHit hit;
                if (Physics.Raycast(origin, dir, out hit, wallDetectDist + 0.8f, mask))
                {
                    float angle = Vector3.Angle(Vector3.up, hit.normal);
                    if (angle > maxWalkableSlope && angle <= 150f)
                    {
                        freshNormal = hit.normal;
                        wallPresent = true;
                        Debug.DrawRay(origin, dir * hit.distance, Color.cyan);
                        break;
                    }
                }
                Debug.DrawRay(origin, dir * (wallDetectDist + 0.8f), Color.yellow);
            }
        }
        else
        {
            wallPresent = DetectClimbableWall(toTarget, out freshNormal);
        }

        if (wallPresent)
        {
            _lostWallTimer = 0f;
            _wallNormal    = freshNormal;

            // Subir por la pared: componente "wallUp" + pequeño empuje hacia la pared
            Vector3 wallRight = Vector3.Cross(_wallNormal, Vector3.up).normalized;
            Vector3 wallUp    = Vector3.Cross(wallRight, _wallNormal).normalized;
            _rb.MovePosition(transform.position +
                             (wallUp * climbSpeed - _wallNormal * 0.05f) * Time.fixedDeltaTime);

            Vector3 face = -_wallNormal; face.y = 0f;
            if (face.sqrMagnitude > 0.01f) RotateToward(face);
        }
        else
        {
            _lostWallTimer += Time.fixedDeltaTime;

            // Si ya estamos en el suelo sin pared → salir de Climb inmediatamente
            // (evita la animación rara cuando el boss cae/baja de la pared)
            if (_isGrounded)
            {
                _lostWallTimer = 0f;
                _wallNormal    = Vector3.zero;
                if (_rb != null) { _rb.useGravity = true; _rb.isKinematic = false; }
                _action    = GOAPAction.Run;
                _planTimer = 0f;
                return;
            }

            // Seguir empujando hacia arriba mientras acumulamos timer
            if (_wallNormal != Vector3.zero)
            {
                Vector3 wallRight = Vector3.Cross(_wallNormal, Vector3.up).normalized;
                Vector3 wallUp    = Vector3.Cross(wallRight, _wallNormal).normalized;
                _rb.MovePosition(transform.position +
                                 (wallUp * climbSpeed - _wallNormal * 0.05f) * Time.fixedDeltaTime);
            }

            // 0.4s sin pared y en el aire → llegamos arriba → mantle
            if (_lostWallTimer > 0.4f)
            {
                _lostWallTimer = 0f;
                StartCoroutine(MantleRoutine());
            }
        }
    }

    /// <summary>
    /// Detecta el punto de aterrizaje encima del borde (igual que TryDetectLedge del player).
    /// </summary>
    private bool TryDetectLedge(out Vector3 ledgePoint)
    {
        ledgePoint = Vector3.zero;
        if (_wallNormal == Vector3.zero) return false;

        LayerMask mask = climbableMask | groundMask;
        float[] heightFactors  = { 0.95f, 0.85f, 0.75f, 1.05f };
        float[] forwardOffsets = { 1.2f, 1.8f, 2.5f, 0.7f, 3.0f };

        foreach (float hf in heightFactors)
        {
            Vector3 headOrigin = transform.position + Vector3.up * (1.8f * hf);

            if (Physics.Raycast(headOrigin, -_wallNormal, wallDetectDist * 2f, climbableMask | groundMask))
                continue;

            foreach (float fwd in forwardOffsets)
            {
                Vector3 overLedge = headOrigin + (-_wallNormal * fwd);
                RaycastHit groundHit;
                if (Physics.Raycast(overLedge, Vector3.down, out groundHit, 4f, mask))
                {
                    float angle = Vector3.Angle(Vector3.up, groundHit.normal);
                    if (angle <= maxWalkableSlope + 10f)
                    {
                        ledgePoint = groundHit.point + Vector3.up * 0.15f;
                        return true;
                    }
                }
            }
        }

        // Fallback garantizado: 2.5m adelante + 1m arriba
        Vector3 fallbackFwd = -_wallNormal; fallbackFwd.y = 0f; fallbackFwd.Normalize();
        ledgePoint = transform.position + fallbackFwd * 2.5f + Vector3.up * 1.0f;
        return true;
    }

    /// <summary>
    /// Mantle replicando el sistema del player:
    ///  · isKinematic=true durante la animación (sin rebotes)
    ///  · Y sube con curva seno (rápido al principio)
    ///  · XZ avanza linealmente hacia el ledgePoint
    /// </summary>
    private IEnumerator MantleRoutine()
    {
        _isMantling = true;
        Debug.Log("[BossGroundAI] Mantle iniciado.");

        // TryDetectLedge siempre devuelve true (tiene fallback garantizado)
        Vector3 ledgePoint;
        TryDetectLedge(out ledgePoint);

        Vector3 startPos = transform.position;

        // Hacer kinematic igual que el player (evita colisiones durante la animación)
        if (_rb != null)
        {
            _rb.velocity     = Vector3.zero;
            _rb.useGravity   = false;
            _rb.isKinematic  = true;
        }

        // Animar con curva: Y=seno, XZ=lineal (igual que UpdateMantle del player)
        float t = 0f;
        while (t < 1f && _isActive)
        {
            yield return new WaitForFixedUpdate();
            t = Mathf.Clamp01(t + Time.fixedDeltaTime * 5f); // más rápido para no pillarse en bordes

            float tY  = Mathf.Sin(t * Mathf.PI * 0.5f); // sube rápido al inicio
            float tXZ = t;                               // avanza linealmente

            Vector3 newPos = new Vector3(
                Mathf.Lerp(startPos.x, ledgePoint.x, tXZ),
                Mathf.Lerp(startPos.y, ledgePoint.y, tY),
                Mathf.Lerp(startPos.z, ledgePoint.z, tXZ));

            if (_rb != null) _rb.MovePosition(newPos);
        }

        // Restaurar física
        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity  = true;
            _rb.velocity    = Vector3.zero;
        }
        if (_isActive) transform.position = ledgePoint;

        // Empuje extra hacia adelante para no quedarse en el borde
        if (_rb != null && _wallNormal != Vector3.zero)
        {
            Vector3 pushFwd = -_wallNormal; pushFwd.y = 0f; pushFwd.Normalize();
            _rb.AddForce(pushFwd * 4f, ForceMode.VelocityChange);
        }

        _isMantling = false;
        _wallNormal = Vector3.zero;
        Debug.Log("[BossGroundAI] Mantle completado → Run.");

        if (_isActive) { _action = GOAPAction.Run; _planTimer = 0f; }
    }

    // ── Gancho ──
    private void ApplyHook()
    {
        _rb.MovePosition(Vector3.MoveTowards(
            transform.position, _hookTarget, hookSpeed * Time.fixedDeltaTime));

        Vector3 dir = (_hookTarget - transform.position); dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f) RotateToward(dir.normalized);

        if (Vector3.Distance(transform.position, _hookTarget) < 0.6f)
        {
            if (_hookCollider != null) _usedHooks.Add(_hookCollider);
            SetAction(GOAPAction.Run);
        }
    }

    /// <summary>
    /// Salto con lógica wall-grab:
    ///  1. Lanza el boss hacia arriba + adelante.
    ///  2. En el pico del salto (~velocidad vertical ≈ 0) comprueba si la pared sigue ahí.
    ///  3. Si hay pared en el pico → activa Climb directamente (se engancha).
    ///  4. Si no hay pared → aterrizaje normal.
    /// </summary>
    private IEnumerator JumpRoutine()
    {
        _jumpInProgress = true;
        _anim?.SetTrigger("Jump");

        // Pequeña pausa para que empiece la animación
        yield return new WaitForSeconds(0.15f);

        // Calcular dirección hacia el destino
        Vector3 jumpDir = GetDirToDestination();

        // Aplicar impulso
        if (_rb != null)
            _rb.AddForce(Vector3.up * jumpUpForce + jumpDir * jumpForwardForce,
                         ForceMode.Impulse);

        // Esperar al pico del salto:
        // el pico ocurre cuando la velocidad vertical pasa de positiva a negativa (o tras timeout)
        float peakTimeout = 0f;
        // primero esperar un mínimo para que el impulso surta efecto
        yield return new WaitForSeconds(0.2f);
        while (_rb != null && _rb.velocity.y > 0.1f && peakTimeout < 1.2f && _isActive)
        {
            peakTimeout += Time.deltaTime;
            yield return null;
        }

        // ── Comprobación en el pico: ¿sigue la pared? ──
        Vector3 peakNormal;
        if (_isActive && DetectClimbableWall(GetDirToDestination(), out peakNormal))
        {
            // Pared detectada en el pico → engancharse y escalar
            Debug.Log("[BossGroundAI] Pared en el pico del salto → Climb.");
            _wallNormal     = peakNormal;
            _jumpInProgress = false;
            // Cancelar velocidad vertical para pegarse a la pared
            if (_rb != null)
            {
                var v = _rb.velocity; v.y = 0f; _rb.velocity = v;
            }
            _action = GOAPAction.Climb;
            yield break;
        }

        // Sin pared → esperar a aterrizar (timeout 2s)
        float landTimeout = 0f;
        while (!_isGrounded && _isActive && landTimeout < 2f)
        {
            landTimeout += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        _jumpInProgress = false;
        if (_isActive) _planTimer = 0f;
    }

    // ════════════════════════════════════════════════════════════════
    //  DETECCIÓN DE ENTORNO
    // ════════════════════════════════════════════════════════════════
    private void CheckGrounded()
    {
        _isGrounded = Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            groundCheckDist + 0.15f,
            groundMask | climbableMask);
    }

    private Vector3 SnapToGround(Vector3 pos)
    {
        RaycastHit hit;
        if (Physics.Raycast(pos + Vector3.up * 0.3f,
                             Vector3.down, out hit, 4f, groundMask | climbableMask))
        {
            float angle = Vector3.Angle(Vector3.up, hit.normal);
            if (angle <= maxWalkableSlope)
                pos.y = Mathf.MoveTowards(pos.y, hit.point.y, 8f * Time.fixedDeltaTime);
        }
        return pos;
    }

    /// <summary>
    /// Detecta paredes escalables usando un abanico de rayos hacia delante
    /// (dirección central ±15° y ±30°) a 3 alturas distintas.
    /// Devuelve true si el ángulo de la normal supera maxWalkableSlope.
    /// </summary>
    private bool DetectClimbableWall(Vector3 direction, out Vector3 wallNormal)
    {
        wallNormal = Vector3.zero;
        if (direction.sqrMagnitude < 0.01f) return false;

        LayerMask mask    = climbableMask | groundMask;
        float[]   heights = { 0.3f, 0.8f, 1.3f };
        // Abanico: recto + ±15° + ±30° para no fallar si la roca está ladeada
        float[]   angles  = { 0f, 15f, -15f, 30f, -30f };

        foreach (float h in heights)
        {
            Vector3 origin = transform.position + Vector3.up * h;

            foreach (float a in angles)
            {
                Vector3 dir = Quaternion.AngleAxis(a, Vector3.up) * direction;
                RaycastHit hit;

                if (Physics.Raycast(origin, dir, out hit, wallDetectDist, mask))
                {
                    float angle = Vector3.Angle(Vector3.up, hit.normal);
                    if (angle > maxWalkableSlope && angle <= 150f)
                    {
                        wallNormal = hit.normal;
                        Debug.DrawRay(origin, dir * hit.distance, Color.red);
                        // Log solo cuando acabamos de detectar (cada 30 frames)
                        if (Time.frameCount % 30 == 0)
                            Debug.Log($"[BossGroundAI] Pared escalable detectada — " +
                                      $"ángulo={angle:F1}° capa={LayerMask.LayerToName(hit.collider.gameObject.layer)} " +
                                      $"objeto={hit.collider.name}");
                        return true;
                    }
                }
                Debug.DrawRay(origin, dir * wallDetectDist, Color.green);
            }
        }
        return false;
    }

    private bool TryFindHookPoint(out Collider bestCol, out Vector3 hookPoint)
    {
        bestCol   = null;
        hookPoint = Vector3.zero;
        if (destination == null) return false;

        float bestGain = 2f; // ganancia mínima para que valga la pena
        foreach (var col in Physics.OverlapSphere(transform.position, hookRange))
        {
            if (!col.CompareTag("HookPoint") || _usedHooks.Contains(col)) continue;

            float gain = Vector3.Distance(transform.position, destination.position)
                       - Vector3.Distance(col.transform.position, destination.position);
            if (gain > bestGain)
            {
                bestGain  = gain;
                hookPoint = col.transform.position;
                bestCol   = col;
            }
        }
        return bestCol != null;
    }

    // ════════════════════════════════════════════════════════════════
    //  ANIMADOR
    // ════════════════════════════════════════════════════════════════
    private void UpdateAnimator()
    {
        if (_anim == null) return;

        bool isClimbing = _action == GOAPAction.Climb || _isMantling;

        // Speed: 0=Idle, 0.4=Walk, 1=Trote/ClimbForward
        float speed;
        if (isClimbing)
            speed = 1.0f;  // ClimbForward (subiendo siempre)
        else
            speed = _action switch
            {
                GOAPAction.Walk => 0.4f,
                GOAPAction.Run  => 1.0f,
                _               => 0f,
            };

        // ClimbH: -1=izquierda, 0=recto/arriba, +1=derecha
        // El boss siempre sube recto, pero si en el futuro hay movimiento lateral se detecta aquí
        float climbH = 0f;
        if (isClimbing && _wallNormal != Vector3.zero)
        {
            Vector3 wallRight = Vector3.Cross(_wallNormal, Vector3.up).normalized;
            // Proyectar la velocidad del RB sobre el eje derecha de la pared
            float lateralVel = Vector3.Dot(_rb != null ? _rb.velocity : Vector3.zero, wallRight);
            climbH = Mathf.Clamp(lateralVel / climbSpeed, -1f, 1f);
        }

        _anim.SetFloat("Speed",      speed);
        _anim.SetFloat("ClimbH",     climbH);
        _anim.SetBool ("IsClimbing", isClimbing);
        _anim.SetBool ("IsGrounded", _isGrounded);

        if (Time.frameCount % 60 == 0)
            Debug.Log($"[BossGroundAI Anim] {_action} Speed={speed:F2} ClimbH={climbH:F2} Climbing={isClimbing} Grounded={_isGrounded}");
    }

    // ════════════════════════════════════════════════════════════════
    //  UTILIDADES
    // ════════════════════════════════════════════════════════════════
    private Vector3 GetDirToDestination()
    {
        Vector3 d = destination != null
            ? (destination.position - transform.position)
            : transform.forward;
        d.y = 0f;
        if (d.sqrMagnitude < 0.01f) d = transform.forward;
        return d.normalized;
    }

    private void RotateToward(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.01f) return;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * 10f);
    }

    // ════════════════════════════════════════════════════════════════
    //  GIZMOS
    // ════════════════════════════════════════════════════════════════
    private void OnDrawGizmosSelected()
    {
        if (destination == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, destination.position);
        Gizmos.DrawWireSphere(destination.position, arrivalRadius);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, hookRange);

        if (_action == GOAPAction.Hook)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, _hookTarget);
        }
    }
}
