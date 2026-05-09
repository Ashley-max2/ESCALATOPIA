using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

/// <summary>
/// ML-Agent para los Bosses de ESCALATOPIA.
///
/// ── MECÁNICAS HEREDADAS DEL JUGADOR ──────────────────────────────────
///   Usa el prefab de Player 1 (personajePrincipal) con sus mismos scripts:
///   PlayerStateMachine, GrapplingHook, StaminaSystem, AgarrarLanzarSoltar.
///
/// ── ESPACIO DE OBSERVACIONES (26 obs) ────────────────────────────────
///   [0-2]  Dirección normalizada a la siguiente meta
///   [3]    Distancia normalizada a la siguiente meta (÷100)
///   [4]    Porcentaje de estamina
///   [5]    IsGrounded
///   [6]    IsClimbing
///   [7]    IsHooking
///   [8-10] Velocidad normalizada (÷20)
///   [11-13] Dirección al jugador objetivo (si existe)
///   [14-18] Progreso de metas este episodio (binario x5)
///   [19]   Distancia al suelo de muerte (normalizada)
///   [20]   Ratio de metas masterizadas globalmente (÷5)
///   [21]   Tiempo normalizado del episodio (÷MaxStep)
///   [22]   Distancia al suelo más cercano (para salto y escalada)
///   [23]   Hay pared escalable enfrente (0/1)
///   [24]   Hay punto de gancho en rango (0/1)
///   [25]   Ángulo de pendiente bajo el boss (÷90)
///
/// ── ESPACIO DE ACCIONES ───────────────────────────────────────────────
///   Continuas [4]: MoveX, MoveZ, CameraX, CameraY
///   Discretas [5]: Jump(3), Hook(2), ReleaseHook(2), Sprint(2), Interact(2)
///
/// ── RECOMPENSAS ───────────────────────────────────────────────────────
///   Meta alcanzada (nueva):      +100 (si no está masterizada)
///   Saltar:                      +0.5 (cuando está en el suelo)
///   Gancho enganchado:           +10
///   Acercarse a meta:            +proximity_shaping (mejora continua)
///   Metas en orden (bonus):      +20 por meta en secuencia
///   Tour completo (1→5):         +200 extra
///   Velocidad eficiente:         +pequeño bonus por moverse rápido hacia meta
///   Estancamiento:               -0.02/step si no se mueve
///   Paso de existencia:          -0.001/step (fuerza eficiencia)
///   Muerte/muerte floor:         -1 final
///
/// ── MEJORAS INCORPORADAS ─────────────────────────────────────────────
///   • Proximity reward shaping con potential-based functions
///   • Stagnation detection y penalización
///   • Sequential meta bonus
///   • Full-tour bonus al completar Meta1→5 en orden
///   • Exploración aleatoria explícita en los primeros N pasos (epsilon-greedy)
///   • Velocidad eficiente bonus
///   • Todas las variables de mecánicas son randoms ajustables por Inspector
/// </summary>
[RequireComponent(typeof(PlayerStateMachine))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(StaminaSystem))]
public class BossAgent : Agent
{
    // ─── Referencias de componentes ───────────────────────────────────
    private PlayerStateMachine _psm;
    private StaminaSystem      _stamina;
    private GrapplingHook      _hook;
    private AgarrarLanzarSoltar _grab;
    private PlayerInputHandler _input;
    private Rigidbody          _rb;

    // ─── Posición de spawn (fogata) ───────────────────────────────────
    [Header("=== SPAWN ===")]
    [Tooltip("Transform de la Fogata: spawn point de cada episodio")]
    public Transform fogataSpawn;
    [Tooltip("Radio de variación random del spawn (para diversidad)")]
    public float spawnRadius = 2f;

    // ─── Target (jugador real) ────────────────────────────────────────
    [Header("=== TARGET ===")]
    [Tooltip("Transform del jugador (opcional, para comportamiento de boss)")]
    public Transform playerTarget;

    // ─── Metas ───────────────────────────────────────────────────────
    [Header("=== METAS ===")]
    [Tooltip("Se rellena automáticamente con los objetos llamados Meta1…Meta5")]
    public Transform[] metas = new Transform[5];

    // ─── Variables de mecánicas randoms (ajustables por Inspector) ────
    [Header("=== VARIABLES MECÁNICAS (rangos) ===")]
    [Header("Velocidad")]
    public float minWalkSpeed = 2f;    public float maxWalkSpeed = 8f;
    public float minRunSpeed  = 4f;    public float maxRunSpeed  = 14f;
    [Header("Salto")]
    public float minJumpForce = 5f;    public float maxJumpForce = 15f;
    public float minFallMult  = 1.5f;  public float maxFallMult  = 4f;
    [Header("Escalada")]
    public float minClimbSpeed = 2f;   public float maxClimbSpeed = 7f;
    public float minClimbStam  = 1f;   public float maxClimbStam  = 10f;
    [Header("Stamina")]
    public float minMaxStamina = 50f;  public float maxMaxStamina = 200f;
    public float minRegenRate  = 5f;   public float maxRegenRate  = 30f;
    [Header("Gancho")]
    public float minHookRange   = 15f; public float maxHookRange   = 50f;
    public float minHookTravel  = 15f; public float maxHookTravel  = 45f;
    public float minHookPull    = 10f; public float maxHookPull    = 30f;
    public float minHookAccel   = 0.1f;public float maxHookAccel   = 0.5f;
    [Header("Agarre/Lanzamiento")]
    public float minGrabDist = 2f;    public float maxGrabDist = 6f;
    public float minThrowForce=5f;    public float maxThrowForce=25f;

    // ─── Exploración epsilon-greedy ───────────────────────────────────
    [Header("=== EXPLORACIÓN ===")]
    [Tooltip("Pasos iniciales con exploración pura aleatoria")]
    public int explorationSteps = 5000;
    [Tooltip("Probabilidad de acción aleatoria fuera del periodo de exploración")]
    [Range(0f, 0.3f)]
    public float epsilonMin = 0.05f;

    // ─── Reward shaping ───────────────────────────────────────────────
    [Header("=== REWARD SHAPING ===")]
    public float proximityShapingScale    = 0.1f;
    public float velocityBonusScale       = 0.002f;
    public float stagnationPenalty        = 0.02f;
    public float existencePenalty         = 0.001f;
    public float sequentialMetaBonus      = 20f;
    public float fullTourBonus            = 200f;

    // ─── Estado interno del episodio ──────────────────────────────────
    private bool[]  _metasReachadasEsteEpisodio = new bool[5];
    private int     _nextMetaIndex;          // 0..4 = Meta1..Meta5
    private float   _prevDistToMeta;
    private Vector3 _prevPosition;
    private int     _stagnationCounter;
    private float   _episodeCumulativeReward;
    private bool    _completedFullTour;

    // Detección de punto de gancho y pared (para observaciones)
    private bool _hookPointInRange;
    private bool _climbableWallAhead;
    private float _groundDist;
    private float _slopeAngle;

    // Suelo de muerte
    private const string MUERTE_TAG  = "Muerte";
    private const string MUERTE_NAME = "Muerte";

    // ─── Lifecycle ────────────────────────────────────────────────────
    public override void Initialize()
    {
        _psm    = GetComponent<PlayerStateMachine>();
        _stamina= GetComponent<StaminaSystem>();
        _hook   = GetComponentInChildren<GrapplingHook>();
        _grab   = GetComponentInChildren<AgarrarLanzarSoltar>();
        _input  = GetComponent<PlayerInputHandler>();
        _rb     = GetComponent<Rigidbody>();

        _input.IsAI = true;

        // Suscribir eventos
        _psm.OnDie               += HandleDeath;
        if (_hook != null) _hook.OnHookAttached += HandleHookAttached;

        // Buscar metas automáticamente si no están asignadas
        AutoFindMetas();

        // Buscar fogata si no está asignada
        if (fogataSpawn == null)
        {
            var fog = GameObject.Find("Fogata") ?? GameObject.Find("Fogata_Cima");
            if (fog != null) fogataSpawn = fog.transform;
        }
    }

    private void OnDestroy()
    {
        if (_psm  != null) _psm.OnDie          -= HandleDeath;
        if (_hook != null) _hook.OnHookAttached -= HandleHookAttached;
    }

    // ─── Inicio de episodio ───────────────────────────────────────────
    public override void OnEpisodeBegin()
    {
        // Registrar reward del episodio que terminó
        if (_episodeCumulativeReward != 0f && GlobalMetaTracker.Instance != null)
            GlobalMetaTracker.Instance.RegisterEpisodeReward(_episodeCumulativeReward);
        _episodeCumulativeReward = 0f;

        // Randomizar variables de mecánicas
        RandomizeMechanics();

        // Spawn en fogata con pequeña variación
        Vector3 spawnPos = fogataSpawn != null
            ? fogataSpawn.position + new Vector3(
                Random.Range(-spawnRadius, spawnRadius), 0.2f,
                Random.Range(-spawnRadius, spawnRadius))
            : transform.position;
        transform.position = spawnPos;

        // Reset física
        _rb.velocity        = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        // Reset estado
        _psm.TransitionToState(_psm.States.Grounded());
        _stamina.RestoreStamina();

        // Reset tracking
        _metasReachadasEsteEpisodio = new bool[5];
        _nextMetaIndex   = 0;
        _prevDistToMeta  = GetDistToNextMeta();
        _prevPosition    = transform.position;
        _stagnationCounter = 0;
        _completedFullTour = false;

        // Bonus de inicio por metas masterizadas globalmente
        float startBonus = GlobalMetaTracker.Instance != null
            ? GlobalMetaTracker.Instance.GetStartingBonus()
            : 0f;
        if (startBonus > 0f)
        {
            AddReward(startBonus);
            _episodeCumulativeReward += startBonus;
        }

        // Calcular distancia inicial al siguiente meta
        _prevDistToMeta = GetDistToNextMeta();
    }

    // ─── Observaciones ────────────────────────────────────────────────
    public override void CollectObservations(VectorSensor sensor)
    {
        // Refrescar valores de entorno
        UpdateEnvironmentSense();

        // [0-3] Dirección + distancia a la siguiente meta
        if (_nextMetaIndex < 5 && metas[_nextMetaIndex] != null)
        {
            Vector3 dir = metas[_nextMetaIndex].position - transform.position;
            sensor.AddObservation(dir.normalized);
            sensor.AddObservation(Mathf.Clamp01(dir.magnitude / 100f));
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
        }

        // [4-7] Estado propio
        sensor.AddObservation(_stamina != null ? _stamina.StaminaPercent : 1f);
        sensor.AddObservation(_psm.IsGrounded  ? 1f : 0f);
        sensor.AddObservation(_psm.IsClimbing  ? 1f : 0f);
        sensor.AddObservation(_psm.IsHooking   ? 1f : 0f);

        // [8-10] Velocidad normalizada
        sensor.AddObservation(_rb.velocity / 20f);

        // [11-13] Dirección al jugador (si existe)
        if (playerTarget != null)
            sensor.AddObservation((playerTarget.position - transform.position).normalized);
        else
            sensor.AddObservation(Vector3.zero);

        // [14-18] Progreso de metas este episodio
        for (int i = 0; i < 5; i++)
            sensor.AddObservation(_metasReachadasEsteEpisodio[i] ? 1f : 0f);

        // [19] Distancia normalizada al suelo de muerte (buscamos hacia abajo)
        float deathDist = GetDistToDeathFloor();
        sensor.AddObservation(Mathf.Clamp01(deathDist / 50f));

        // [20] Ratio de metas masterizadas globalmente
        int mastered = GlobalMetaTracker.Instance != null
            ? GlobalMetaTracker.Instance.MetasMastered : 0;
        sensor.AddObservation(mastered / 5f);

        // [21] Tiempo normalizado del episodio
        sensor.AddObservation(Mathf.Clamp01((float)StepCount / MaxStep));

        // [22] Distancia al suelo más cercano
        sensor.AddObservation(Mathf.Clamp01(_groundDist / 10f));

        // [23] Pared escalable enfrente
        sensor.AddObservation(_climbableWallAhead ? 1f : 0f);

        // [24] Punto de gancho en rango
        sensor.AddObservation(_hookPointInRange ? 1f : 0f);

        // [25] Ángulo de pendiente bajo el boss
        sensor.AddObservation(_slopeAngle / 90f);
    }

    // ─── Acciones ─────────────────────────────────────────────────────
    public override void OnActionReceived(ActionBuffers actions)
    {
        // ── Exploración epsilon-greedy temprana ──
        float epsilon = StepCount < explorationSteps
            ? Mathf.Lerp(1f, epsilonMin, (float)StepCount / explorationSteps)
            : epsilonMin;

        if (Random.value < epsilon)
        {
            ApplyRandomAction();
        }
        else
        {
            ApplyNetworkAction(actions);
        }

        // ── Recompensas por paso ──
        ApplyStepRewards();

        // Actualizar tracking de episodio
        _episodeCumulativeReward = GetCumulativeReward();
    }

    // ─── Modo humano (debug con teclado) ─────────────────────────────
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var c = actionsOut.ContinuousActions;
        c[0] = Input.GetAxisRaw("Horizontal");
        c[1] = Input.GetAxisRaw("Vertical");
        c[2] = Input.GetAxis("Mouse X");
        c[3] = Input.GetAxis("Mouse Y");

        var d = actionsOut.DiscreteActions;
        d[0] = Input.GetKey(KeyCode.Space)        ? 1 : 0;
        d[1] = Input.GetMouseButtonDown(1)        ? 1 : 0;
        d[2] = Input.GetMouseButtonDown(0)        ? 1 : 0;
        d[3] = Input.GetKey(KeyCode.LeftShift)    ? 1 : 0;
        d[4] = Input.GetKeyDown(KeyCode.E)        ? 1 : 0;
    }

    // ─── Triggers ─────────────────────────────────────────────────────
    private void OnCollisionEnter(Collision col)
    {
        if (IsMuerte(col.gameObject))
        {
            AddReward(-1f);
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsMuerte(other.gameObject))
        {
            AddReward(-1f);
            EndEpisode();
            return;
        }

        // Comprobación de metas
        CheckMetaTrigger(other.gameObject);
    }

    // ─── Helpers de detección ─────────────────────────────────────────

    private void CheckMetaTrigger(GameObject obj)
    {
        for (int i = 0; i < 5; i++)
        {
            if (metas[i] == null) continue;
            if (obj != metas[i].gameObject && obj.transform != metas[i]) continue;

            if (_metasReachadasEsteEpisodio[i]) return; // ya la tenemos este episodio

            _metasReachadasEsteEpisodio[i] = true;

            // Recompensa solo si NO está masterizada
            bool mastered = GlobalMetaTracker.Instance != null
                && GlobalMetaTracker.Instance.IsMetaMastered(i);

            if (!mastered)
            {
                AddReward(GlobalMetaTracker.Instance != null
                    ? GlobalMetaTracker.Instance.metaReward
                    : 100f);
                GlobalMetaTracker.Instance?.RegisterMetaReached(i);
                Debug.Log($"[BossAgent:{name}] 🎯 Meta{i+1} alcanzada! (+100)");
            }
            else
            {
                Debug.Log($"[BossAgent:{name}] Meta{i+1} ya masterizada, sin puntos extra.");
            }

            // Bonus secuencial: si se alcanzó en orden (Meta i era el siguiente esperado)
            if (i == _nextMetaIndex)
            {
                AddReward(sequentialMetaBonus);
                _nextMetaIndex = i + 1;
                Debug.Log($"[BossAgent:{name}] ✅ Bonus secuencial Meta{i+1} (+{sequentialMetaBonus})");
            }

            // Tour completo 1→5 en orden
            if (_nextMetaIndex >= 5 && !_completedFullTour)
            {
                _completedFullTour = true;
                AddReward(fullTourBonus);
                Debug.Log($"[BossAgent:{name}] 🏅 TOUR COMPLETO! (+{fullTourBonus})");
                EndEpisode(); // Éxito absoluto
            }

            // Actualizar distancia al siguiente meta
            _prevDistToMeta = GetDistToNextMeta();
            return;
        }
    }

    private bool IsMuerte(GameObject go) =>
        go.name == MUERTE_NAME || go.CompareTag(MUERTE_TAG);

    // ─── Eventos de mecánicas ─────────────────────────────────────────

    private void HandleHookAttached()
    {
        AddReward(10f);
        Debug.Log($"[BossAgent:{name}] 🪝 Gancho enganchado! (+10)");
    }

    private void HandleDeath()
    {
        AddReward(-1f);
        EndEpisode();
    }

    // ─── Aplicar acciones de red ──────────────────────────────────────

    private void ApplyNetworkAction(ActionBuffers actions)
    {
        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        _input.SetAIMovement(moveX, moveZ);
        _input.SetAICamera(actions.ContinuousActions[2], actions.ContinuousActions[3]);

        // Jump: 0=nada, 1=press, 2=hold
        int jumpAct = actions.DiscreteActions[0];
        _input.SetAIJump(jumpAct == 1, jumpAct == 2);

        // Recompensa por saltar en el suelo
        if (jumpAct == 1 && _psm.IsGrounded)
        {
            AddReward(0.5f);
            Debug.Log($"[BossAgent:{name}] ⬆️ Salto (+0.5)");
        }

        _input.SetAIHook(actions.DiscreteActions[1] == 1,
                         actions.DiscreteActions[2] == 1);
        _input.SetAISprint(actions.DiscreteActions[3] == 1);
        _input.SetAIInteract(actions.DiscreteActions[4] == 1);
    }

    /// <summary>
    /// Exploración aleatoria: usa todas las mecánicas al azar para descubrir recompensas.
    /// </summary>
    private void ApplyRandomAction()
    {
        // Movimiento aleatorio
        _input.SetAIMovement(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        _input.SetAICamera(Random.Range(-1f, 1f), Random.Range(-0.5f, 0.5f));

        // Acciones con probabilidad variable para que se intenten todas
        bool jump     = Random.value < 0.08f;
        bool hookFire = Random.value < 0.06f && _hookPointInRange;
        bool sprint   = Random.value < 0.3f;
        bool interact = Random.value < 0.02f;

        _input.SetAIJump(jump, false);
        _input.SetAIHook(hookFire, Random.value < 0.05f);
        _input.SetAISprint(sprint);
        _input.SetAIInteract(interact);

        if (jump && _psm.IsGrounded) AddReward(0.5f);
    }

    // ─── Recompensas por paso ─────────────────────────────────────────

    private void ApplyStepRewards()
    {
        // Penalización de existencia (fuerza eficiencia temporal)
        AddReward(-existencePenalty);

        // Proximity shaping: diferencia de potencial (Ng et al., 1999)
        float currentDist = GetDistToNextMeta();
        float improvement = _prevDistToMeta - currentDist;
        if (improvement > 0f)
            AddReward(improvement * proximityShapingScale);
        _prevDistToMeta = currentDist;

        // Bonus de velocidad eficiente hacia la meta
        if (_nextMetaIndex < 5 && metas[_nextMetaIndex] != null)
        {
            Vector3 toMeta = (metas[_nextMetaIndex].position - transform.position).normalized;
            float alignedSpeed = Vector3.Dot(_rb.velocity, toMeta);
            if (alignedSpeed > 1f)
                AddReward(alignedSpeed * velocityBonusScale);
        }

        // Penalización de estancamiento
        float moved = Vector3.Distance(transform.position, _prevPosition);
        if (moved < 0.05f)
        {
            _stagnationCounter++;
            if (_stagnationCounter > 50) // ~2s a 25 fps
                AddReward(-stagnationPenalty);
        }
        else
        {
            _stagnationCounter = 0;
        }
        _prevPosition = transform.position;
    }

    // ─── Randomización de mecánicas ───────────────────────────────────

    private void RandomizeMechanics()
    {
        if (_psm != null)
        {
            float walk = Random.Range(minWalkSpeed, maxWalkSpeed);
            _psm.SetWalkSpeed(walk);
            _psm.SetRunSpeed(Random.Range(Mathf.Max(walk, minRunSpeed), maxRunSpeed));
            _psm.SetJumpForce(Random.Range(minJumpForce, maxJumpForce));
            _psm.SetClimbSpeed(Random.Range(minClimbSpeed, maxClimbSpeed));
            _psm.SetClimbStaminaCost(Random.Range(minClimbStam, maxClimbStam));
            _psm.SetFallMultiplier(Random.Range(minFallMult, maxFallMult));
            _psm.SetHookAccelerationTime(Random.Range(minHookAccel, maxHookAccel));
        }

        if (_stamina != null)
        {
            _stamina.SetMaxStamina(Random.Range(minMaxStamina, maxMaxStamina));
            _stamina.SetRegenRate(Random.Range(minRegenRate, maxRegenRate));
        }

        if (_hook != null)
        {
            _hook.SetMaxRange(Random.Range(minHookRange, maxHookRange));
            _hook.SetTravelSpeed(Random.Range(minHookTravel, maxHookTravel));
            _hook.SetPullSpeed(Random.Range(minHookPull, maxHookPull));
        }

        if (_grab != null)
        {
            _grab.SetDistanciaMax(Random.Range(minGrabDist, maxGrabDist));
            _grab.SetFuerzaLanzamiento(Random.Range(minThrowForce, maxThrowForce));
        }
    }

    // ─── Helpers de entorno ───────────────────────────────────────────

    private void UpdateEnvironmentSense()
    {
        // Distancia al suelo
        _groundDist = 0f;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit gHit, 20f))
        {
            _groundDist  = gHit.distance;
            _slopeAngle  = Vector3.Angle(Vector3.up, gHit.normal);
        }

        // Pared escalable enfrente
        _climbableWallAhead = _psm.CheckClimbableSurface(out _);

        // Punto de gancho en rango
        _hookPointInRange = false;
        if (_hook != null)
        {
            var hookPoints = GameObject.FindGameObjectsWithTag("HookPoint");
            foreach (var hp in hookPoints)
            {
                if (Vector3.Distance(transform.position, hp.transform.position) < 30f)
                {
                    _hookPointInRange = true;
                    break;
                }
            }
        }
    }

    private float GetDistToNextMeta()
    {
        if (_nextMetaIndex >= 5 || metas[_nextMetaIndex] == null) return 0f;
        return Vector3.Distance(transform.position, metas[_nextMetaIndex].position);
    }

    private float GetDistToDeathFloor()
    {
        var muerte = GameObject.Find(MUERTE_NAME);
        if (muerte == null) return 100f;
        return Mathf.Abs(transform.position.y - muerte.transform.position.y);
    }

    private void AutoFindMetas()
    {
        for (int i = 0; i < 5; i++)
        {
            if (metas[i] != null) continue;
            var go = GameObject.Find($"Meta{i + 1}");
            if (go != null) metas[i] = go.transform;
        }
    }

    // ─── Gizmos ───────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (_nextMetaIndex < 5 && metas != null && metas[_nextMetaIndex] != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, metas[_nextMetaIndex].position);
            Gizmos.DrawWireSphere(metas[_nextMetaIndex].position, 1.5f);
        }
        if (fogataSpawn != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(fogataSpawn.position, spawnRadius);
        }
    }
}
