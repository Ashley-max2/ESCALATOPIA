using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class BossMLAgent : Agent
{
    // =====================================================================
    //  REFERENCIAS (Asignar en el Inspector)
    // =====================================================================
    [Header("=== REFERENCIAS ===")]
    [Tooltip("Transform de la meta a la que el Boss debe llegar")]
    public Transform meta;

    [Tooltip("Transform del punto de inicio del Boss (se teletransporta aqui cada episodio)")]
    public Transform puntoInicio;

    // =====================================================================
    //  RANGOS DE DOMAIN RANDOMIZATION (Min/Max que se aplican cada episodio)
    // =====================================================================
    [Header("=== DOMAIN RANDOMIZATION: CORRER ===")]
    public float velocidadCorrerMin = 4f;
    public float velocidadCorrerMax = 12f;

    [Header("=== DOMAIN RANDOMIZATION: SALTO ===")]
    public float fuerzaSaltoMin = 6f;
    public float fuerzaSaltoMax = 14f;

    [Header("=== DOMAIN RANDOMIZATION: ESCALADA ===")]
    public float velocidadEscaladaMin = 2f;
    public float velocidadEscaladaMax = 6f;

    [Header("=== DOMAIN RANDOMIZATION: GANCHO ===")]
    public float velocidadGanchoMin = 40f;
    public float velocidadGanchoMax = 60f;
    public float distanciaGanchoMin = 25f;
    public float distanciaGanchoMax = 50f;

    [Header("=== DOMAIN RANDOMIZATION: SALTO DE PARED ===")]
    public float fuerzaSaltoParedMin = 5f;
    public float fuerzaSaltoParedMax = 12f;
    public float fuerzaSaltoParedArribaMin = 4f;
    public float fuerzaSaltoParedArribaMax = 10f;

    // =====================================================================
    //  DETECCION (Layers y distancias fisicas)
    // =====================================================================
    [Header("=== DETECCION ===")]
    public LayerMask groundMask;
    public LayerMask climbableMask;
    public LayerMask hookPointMask;
    public float groundCheckRadius = 0.3f;
    public float climbCheckDistance = 0.8f;
    public float minClimbAngle = 45f;
    public float maxClimbAngle = 135f;

    [Header("=== FISICAS ===")]
    public float fallMultiplier = 2.5f;
    public float alturaMinimaMuerte = -10f;

    // =====================================================================
    //  STATS ACTUALES DEL EPISODIO (Se randomizan, solo lectura en Inspector)
    // =====================================================================
    [Header("=== STATS ACTUALES (Solo Lectura) ===")]
    [SerializeField] private float _velocidadCorrer;
    [SerializeField] private float _fuerzaSalto;
    [SerializeField] private float _velocidadEscalada;
    [SerializeField] private float _velocidadGancho;
    [SerializeField] private float _distanciaGancho;
    [SerializeField] private float _fuerzaSaltoPared;
    [SerializeField] private float _fuerzaSaltoParedArriba;

    // =====================================================================
    //  ESTADO INTERNO
    // =====================================================================
    private Rigidbody rb;
    private CapsuleCollider col;
    private Animator animator;

    private bool estaEnSuelo;
    private bool estaEscalando;
    private bool estaEnganchado;
    private Vector3 normalPared;
    private Vector3 objetivoGancho;

    private float cooldownSalto;
    private float cooldownGancho;
    private float tiempoEscalando;
    private float distanciaInicialAMeta;
    private float mejorDistanciaAMeta;
    private Vector3 posicionSegura;

    // =====================================================================
    //  UNITY CALLBACKS
    // =====================================================================
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        animator = GetComponentInChildren<Animator>();

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    // =====================================================================
    //  ML-AGENTS: INICIO DE EPISODIO
    // =====================================================================
    public override void OnEpisodeBegin()
    {
        // 1. DOMAIN RANDOMIZATION: Randomizar todas las stats
        _velocidadCorrer = Random.Range(velocidadCorrerMin, velocidadCorrerMax);
        _fuerzaSalto = Random.Range(fuerzaSaltoMin, fuerzaSaltoMax);
        _velocidadEscalada = Random.Range(velocidadEscaladaMin, velocidadEscaladaMax);
        _velocidadGancho = Random.Range(velocidadGanchoMin, velocidadGanchoMax);
        _distanciaGancho = Random.Range(distanciaGanchoMin, distanciaGanchoMax);
        _fuerzaSaltoPared = Random.Range(fuerzaSaltoParedMin, fuerzaSaltoParedMax);
        _fuerzaSaltoParedArriba = Random.Range(fuerzaSaltoParedArribaMin, fuerzaSaltoParedArribaMax);

        // 2. Teletransportar al Boss al inicio
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (puntoInicio != null)
        {
            transform.localPosition = puntoInicio.localPosition;
            transform.localRotation = puntoInicio.localRotation;
        }
        else
        {
            transform.localPosition = Vector3.up;
            transform.localRotation = Quaternion.identity;
        }

        // 3. Resetear estado
        estaEnSuelo = false;
        estaEscalando = false;
        estaEnganchado = false;
        normalPared = Vector3.zero;
        objetivoGancho = Vector3.zero;
        cooldownSalto = 0f;
        cooldownGancho = 0f;
        tiempoEscalando = 0f;
        posicionSegura = transform.position;

        rb.useGravity = true;
        rb.isKinematic = false;

        // 4. Calcular referencia de distancia para recompensas
        if (meta != null)
        {
            distanciaInicialAMeta = Vector3.Distance(transform.position, meta.position);
            mejorDistanciaAMeta = distanciaInicialAMeta;
        }
    }

    // =====================================================================
    //  ML-AGENTS: OBSERVACIONES (Los "sentidos" del Boss)
    // =====================================================================
    public override void CollectObservations(VectorSensor sensor)
    {
        // --- AUTOCONCIENCIA CORPORAL (Domain Randomization) ---
        // El cerebro SABE que stats tiene su cuerpo actual (7 valores)
        sensor.AddObservation(_velocidadCorrer / velocidadCorrerMax);
        sensor.AddObservation(_fuerzaSalto / fuerzaSaltoMax);
        sensor.AddObservation(_velocidadEscalada / velocidadEscaladaMax);
        sensor.AddObservation(_velocidadGancho / velocidadGanchoMax);
        sensor.AddObservation(_distanciaGancho / distanciaGanchoMax);
        sensor.AddObservation(_fuerzaSaltoPared / fuerzaSaltoParedMax);
        sensor.AddObservation(_fuerzaSaltoParedArriba / fuerzaSaltoParedArribaMax);

        // --- CONCIENCIA ESPACIAL ---
        // Velocidad actual normalizada (3 valores)
        sensor.AddObservation(rb.velocity / 15f);

        // Direccion hacia la meta normalizada (3 valores)
        Vector3 dirMeta = Vector3.zero;
        float distMeta = 0f;
        if (meta != null)
        {
            dirMeta = (meta.position - transform.position).normalized;
            distMeta = Vector3.Distance(transform.position, meta.position);
        }
        sensor.AddObservation(dirMeta);

        // Distancia a la meta normalizada (1 valor)
        sensor.AddObservation(distMeta / (distanciaInicialAMeta + 0.01f));

        // Altura relativa al inicio (1 valor)
        float alturaRelativa = transform.position.y - (puntoInicio != null ? puntoInicio.position.y : 0f);
        sensor.AddObservation(alturaRelativa / 50f);

        // --- ESTADO ACTUAL ---
        sensor.AddObservation(estaEnSuelo ? 1f : 0f);        // (1)
        sensor.AddObservation(estaEscalando ? 1f : 0f);      // (1)
        sensor.AddObservation(estaEnganchado ? 1f : 0f);      // (1)

        // Normal de la pared si esta escalando (3 valores)
        sensor.AddObservation(estaEscalando ? normalPared : Vector3.zero);

        // Puede usar gancho ahora? (1 valor)
        bool puedeGancho = cooldownGancho <= 0f && !estaEnganchado && BuscarMejorHookPoint() != Vector3.zero;
        sensor.AddObservation(puedeGancho ? 1f : 0f);

        // Puede saltar ahora? (1 valor)
        bool puedeSaltar = (estaEnSuelo || estaEscalando) && cooldownSalto <= 0f;
        sensor.AddObservation(puedeSaltar ? 1f : 0f);

        // Direccion al hookpoint mas cercano (3 valores)
        Vector3 hookDir = Vector3.zero;
        Vector3 hookPos = BuscarMejorHookPoint();
        if (hookPos != Vector3.zero)
        {
            hookDir = (hookPos - transform.position).normalized;
        }
        sensor.AddObservation(hookDir);

        // TOTAL: 7 + 3 + 3 + 1 + 1 + 1 + 1 + 1 + 3 + 1 + 1 + 3 = 26
    }

    // =====================================================================
    //  ML-AGENTS: ACCIONES (Las "decisiones" del cerebro)
    // =====================================================================
    // Acciones Continuas (4):
    //   [0] = Movimiento eje X (-1 a 1)
    //   [1] = Movimiento eje Z (-1 a 1)
    //   [2] = Escalada eje horizontal (-1 a 1)
    //   [3] = Escalada eje vertical (-1 a 1)
    //
    // Acciones Discretas (2 ramas):
    //   Branch 0: Saltar (0=no, 1=si)             -> Size 2
    //   Branch 1: Usar Gancho (0=no, 1=si)         -> Size 2

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Leer acciones continuas
        float moverX = actions.ContinuousActions[0];
        float moverZ = actions.ContinuousActions[1];
        float escalarX = actions.ContinuousActions[2];
        float escalarY = actions.ContinuousActions[3];

        // Leer acciones discretas
        int saltar = actions.DiscreteActions[0];
        int gancho = actions.DiscreteActions[1];

        // Actualizar detecciones fisicas
        ActualizarSuelo();
        ActualizarEscalada();
        ActualizarCooldowns();

        // ============ EJECUTAR ACCIONES SEGUN ESTADO ============

        if (estaEnganchado)
        {
            // Viajando por el gancho: No hacer nada mas, el movimiento se hace en FixedUpdate
            EjecutarViajeGancho();
        }
        else if (estaEscalando)
        {
            // Escalando una pared
            EjecutarEscalada(escalarX, escalarY);

            // Salto de pared
            if (saltar == 1 && cooldownSalto <= 0f)
            {
                EjecutarSaltoDePared();
            }

            // Gancho desde la pared
            if (gancho == 1 && cooldownGancho <= 0f)
            {
                IntentarGancho();
            }
        }
        else if (estaEnSuelo)
        {
            // En el suelo: Correr
            EjecutarMovimientoSuelo(moverX, moverZ);

            // Saltar
            if (saltar == 1 && cooldownSalto <= 0f)
            {
                EjecutarSalto();
            }

            // Gancho desde el suelo
            if (gancho == 1 && cooldownGancho <= 0f)
            {
                IntentarGancho();
            }
        }
        else
        {
            // En el aire: Control aereo limitado
            EjecutarControlAereo(moverX, moverZ);
            AplicarFisicasMejoradas();

            // Auto-escalar al chocar con pared escalable
            IntentarAutoEscalada();

            // Gancho en el aire
            if (gancho == 1 && cooldownGancho <= 0f)
            {
                IntentarGancho();
            }
        }

        // ============ SISTEMA DE RECOMPENSAS ============
        CalcularRecompensas();

        // Animar
        ActualizarAnimator();
    }

    // =====================================================================
    //  ML-AGENTS: MODO HEURISTICO (Control manual para probar)
    // =====================================================================
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Continuas
        var continuas = actionsOut.ContinuousActions;
        continuas[0] = Input.GetKey(KeyCode.D) ? 1f : Input.GetKey(KeyCode.A) ? -1f : 0f;
        continuas[1] = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;
        continuas[2] = continuas[0]; // Mismo lateral para escalada
        continuas[3] = continuas[1]; // Mismo vertical para escalada

        // Discretas
        var discretas = actionsOut.DiscreteActions;
        discretas[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
        discretas[1] = Input.GetKey(KeyCode.E) ? 1 : 0;
    }

    // =====================================================================
    //  MECANICA: MOVIMIENTO EN SUELO (Replica de PlayerGroundedState)
    // =====================================================================
    private void EjecutarMovimientoSuelo(float inputX, float inputZ)
    {
        Vector3 direccion = new Vector3(inputX, 0, inputZ).normalized;
        if (direccion.sqrMagnitude < 0.01f) return;

        Vector3 velocidadObjetivo = direccion * _velocidadCorrer;
        Vector3 velActual = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        Vector3 nuevaVel = Vector3.Lerp(velActual, velocidadObjetivo, 10f * Time.fixedDeltaTime);

        rb.velocity = new Vector3(nuevaVel.x, rb.velocity.y, nuevaVel.z);

        // Rotar hacia la direccion de movimiento
        if (direccion.sqrMagnitude > 0.01f)
        {
            Quaternion rotObjetivo = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotObjetivo, 10f * Time.fixedDeltaTime);
        }
    }

    // =====================================================================
    //  MECANICA: SALTO (Replica de PlayerJumpState)
    // =====================================================================
    private void EjecutarSalto()
    {
        Vector3 vel = rb.velocity;
        vel.y = 0;
        rb.velocity = vel;

        rb.AddForce(Vector3.up * _fuerzaSalto, ForceMode.Impulse);
        cooldownSalto = 0.3f;
        estaEnSuelo = false;
    }

    // =====================================================================
    //  MECANICA: CONTROL AEREO (Replica de PlayerAirborneState)
    // =====================================================================
    private void EjecutarControlAereo(float inputX, float inputZ)
    {
        Vector3 direccion = new Vector3(inputX, 0, inputZ).normalized;
        if (direccion.sqrMagnitude < 0.01f) return;

        float controlAereo = 0.5f;
        Vector3 velocidadObjetivo = direccion * _velocidadCorrer;
        Vector3 velActual = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        Vector3 nuevaVel = Vector3.Lerp(velActual, velocidadObjetivo, 10f * controlAereo * Time.fixedDeltaTime);

        rb.velocity = new Vector3(nuevaVel.x, rb.velocity.y, nuevaVel.z);
    }

    private void AplicarFisicasMejoradas()
    {
        // Caida mas rapida (mejor sensacion de salto como en el player)
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    // =====================================================================
    //  MECANICA: ESCALADA (Replica de PlayerClimbState)
    // =====================================================================
    private void EjecutarEscalada(float inputX, float inputY)
    {
        // Calcular ejes de la pared (igual que el player)
        Vector3 wallRight = Vector3.Cross(normalPared, Vector3.up).normalized;
        Vector3 wallUp = Vector3.Cross(wallRight, normalPared).normalized;

        Vector3 climbDir = (wallRight * inputX + wallUp * inputY).normalized;
        Vector3 velocidadObjetivo = climbDir * _velocidadEscalada;

        // Empuje hacia la pared para mantener contacto
        velocidadObjetivo += -normalPared * 2f;

        rb.velocity = Vector3.Lerp(rb.velocity, velocidadObjetivo, Time.fixedDeltaTime * 10f);

        // Mantle: Si sube y no hay pared arriba, subir automaticamente
        if (inputY > 0.3f)
        {
            IntentarMantle();
        }

        tiempoEscalando += Time.fixedDeltaTime;
    }

    private void IntentarMantle()
    {
        float playerHeight = col != null ? col.height : 2f;
        float checkHeight = playerHeight * 0.85f;
        Vector3 headOrigin = transform.position + Vector3.up * checkHeight;

        bool hayParedArriba = Physics.Raycast(headOrigin, -normalPared, climbCheckDistance * 2f, climbableMask);

        if (!hayParedArriba)
        {
            // No hay pared arriba, buscar suelo encima
            Vector3 sobreBorde = headOrigin + (-normalPared * 0.8f);
            RaycastHit groundHit;
            if (Physics.Raycast(sobreBorde, Vector3.down, out groundHit, 3f, groundMask | climbableMask))
            {
                // Teletransportar encima (mantle simplificado para IA)
                transform.position = groundHit.point + Vector3.up * 0.5f;
                rb.velocity = Vector3.zero;
                estaEscalando = false;
                rb.useGravity = true;
            }
        }
    }

    // =====================================================================
    //  MECANICA: SALTO DE PARED (Replica de PlayerWallJumpState)
    // =====================================================================
    private void EjecutarSaltoDePared()
    {
        rb.velocity = Vector3.zero;

        Vector3 fuerzaSalto = normalPared * _fuerzaSaltoPared + Vector3.up * _fuerzaSaltoParedArriba;
        rb.AddForce(fuerzaSalto, ForceMode.Impulse);

        // Rotar mirando lejos de la pared
        Vector3 dirMirar = new Vector3(normalPared.x, 0, normalPared.z).normalized;
        if (dirMirar.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(dirMirar);
        }

        estaEscalando = false;
        rb.useGravity = true;
        cooldownSalto = 0.3f;
    }

    // =====================================================================
    //  MECANICA: GANCHO (Replica de GrapplingHook + PlayerHookState)
    // =====================================================================
    private void IntentarGancho()
    {
        Vector3 hookTarget = BuscarMejorHookPoint();
        if (hookTarget == Vector3.zero) return;

        estaEnganchado = true;
        objetivoGancho = hookTarget;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        cooldownGancho = 1.5f;

        if (estaEscalando)
        {
            estaEscalando = false;
        }
    }

    private void EjecutarViajeGancho()
    {
        if (objetivoGancho == Vector3.zero)
        {
            LiberarGancho();
            return;
        }

        // Mover hacia el punto de gancho (igual que PlayerHookState)
        Vector3 direccion = (objetivoGancho - transform.position).normalized;
        rb.velocity = direccion * _velocidadGancho;

        // Rotar hacia la direccion de viaje
        Vector3 horizontalDir = new Vector3(direccion.x, 0, direccion.z).normalized;
        if (horizontalDir.sqrMagnitude > 0.01f)
        {
            Quaternion rotObjetivo = Quaternion.LookRotation(horizontalDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotObjetivo, Time.fixedDeltaTime * 10f);
        }

        // Comprobar llegada
        float distancia = Vector3.Distance(transform.position, objetivoGancho);
        if (distancia < 1.5f)
        {
            rb.velocity = Vector3.zero;
            posicionSegura = transform.position;

            // Auto-escalar si hay pared escalable al llegar
            RaycastHit hit;
            if (CheckClimbableSurface(out hit))
            {
                normalPared = hit.normal;
                estaEscalando = true;
                rb.useGravity = false;
                SnapRotacionAPared();
            }

            LiberarGancho();
        }
    }

    private void LiberarGancho()
    {
        estaEnganchado = false;
        objetivoGancho = Vector3.zero;
        if (!estaEscalando)
        {
            rb.useGravity = true;
        }
    }

    private Vector3 BuscarMejorHookPoint()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _distanciaGancho, hookPointMask);

        Transform mejorTarget = null;
        float mejorDistancia = float.MaxValue;

        foreach (var c in colliders)
        {
            if (!c.CompareTag("HookPoint")) continue;

            Vector3 dirATarget = c.transform.position - transform.position;
            float dist = dirATarget.magnitude;

            // Verificar linea de vision
            if (Physics.Raycast(transform.position + Vector3.up, dirATarget.normalized,
                dist - 0.5f, ~hookPointMask))
                continue;

            if (dist < mejorDistancia)
            {
                mejorDistancia = dist;
                mejorTarget = c.transform;
            }
        }

        return mejorTarget != null ? mejorTarget.position : Vector3.zero;
    }

    // =====================================================================
    //  DETECCION FISICA
    // =====================================================================
    private void ActualizarSuelo()
    {
        // Deteccion de suelo con SphereCast (igual que el player)
        Vector3 origen = transform.position + Vector3.up * 0.1f;
        estaEnSuelo = Physics.CheckSphere(origen + Vector3.down * 0.1f, groundCheckRadius, groundMask);

        if (estaEnSuelo)
        {
            posicionSegura = transform.position;
        }
    }

    private void ActualizarEscalada()
    {
        if (!estaEscalando) return;

        RaycastHit hit;
        if (CheckClimbableSurface(out hit))
        {
            normalPared = hit.normal;

            // Rotar hacia la pared suavemente
            Vector3 lookDir = -hit.normal;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * 15f);
            }
        }
        else
        {
            // Perdio contacto con la pared
            estaEscalando = false;
            rb.useGravity = true;
        }

        // Si toca el suelo escalando, dejar de escalar
        if (estaEnSuelo)
        {
            estaEscalando = false;
            rb.useGravity = true;
        }
    }

    private void IntentarAutoEscalada()
    {
        RaycastHit hit;
        if (CheckClimbableSurface(out hit))
        {
            normalPared = hit.normal;
            estaEscalando = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            tiempoEscalando = 0f;
            SnapRotacionAPared();
        }
    }

    private bool CheckClimbableSurface(out RaycastHit hit)
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        float checkDist = estaEscalando ? climbCheckDistance * 1.5f : climbCheckDistance;

        // Raycast directo
        if (Physics.Raycast(origin, transform.forward, out hit, checkDist, climbableMask))
        {
            float surfaceAngle = Vector3.Angle(Vector3.up, hit.normal);
            if (surfaceAngle >= minClimbAngle && surfaceAngle <= maxClimbAngle)
            {
                return true;
            }
        }

        // SphereCast cobertura
        if (Physics.SphereCast(origin, 0.25f, transform.forward, out hit, checkDist, climbableMask))
        {
            float surfaceAngle = Vector3.Angle(Vector3.up, hit.normal);
            if (surfaceAngle >= minClimbAngle && surfaceAngle <= maxClimbAngle)
            {
                return true;
            }
        }

        hit = default;
        return false;
    }

    private void SnapRotacionAPared()
    {
        Vector3 lookDir = -normalPared;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        }
    }

    private void ActualizarCooldowns()
    {
        if (cooldownSalto > 0) cooldownSalto -= Time.fixedDeltaTime;
        if (cooldownGancho > 0) cooldownGancho -= Time.fixedDeltaTime;
    }

    // =====================================================================
    //  SISTEMA DE RECOMPENSAS
    // =====================================================================
    private void CalcularRecompensas()
    {
        if (meta == null) return;

        float distActual = Vector3.Distance(transform.position, meta.position);

        // CASTIGO: Penalizacion por tiempo (que se mueva rapido)
        AddReward(-0.5f / MaxStep);

        // RECOMPENSA: Progreso hacia la meta (se acerca)
        if (distActual < mejorDistanciaAMeta)
        {
            float progreso = mejorDistanciaAMeta - distActual;
            AddReward(progreso * 0.1f);
            mejorDistanciaAMeta = distActual;
        }

        // RECOMPENSA MAXIMA: Llego a la meta
        if (distActual < 2.5f)
        {
            SetReward(10f);
            EndEpisode();
            return;
        }

        // CASTIGO: Caer al vacio
        if (transform.position.y < alturaMinimaMuerte)
        {
            SetReward(-3f);
            EndEpisode();
            return;
        }

        // CASTIGO: Quedarse quieto (velocidad horizontal casi nula durante mucho tiempo)
        float velHorizontal = new Vector2(rb.velocity.x, rb.velocity.z).magnitude;
        if (velHorizontal < 0.1f && estaEnSuelo && !estaEscalando)
        {
            AddReward(-0.01f);
        }
    }

    // =====================================================================
    //  ANIMATOR
    // =====================================================================
    private void ActualizarAnimator()
    {
        if (animator == null) return;

        float velHorizontal = new Vector2(rb.velocity.x, rb.velocity.z).magnitude;

        if (estaEscalando)
        {
            animator.SetBool("Climb", true);
            animator.SetFloat("ClimbSpeed", Mathf.Abs(rb.velocity.y) > 0.1f ? 1f : 0f);
            animator.SetFloat("Speed", 0f);
            animator.SetBool("Run", false);
        }
        else
        {
            animator.SetBool("Climb", false);
            animator.SetFloat("ClimbSpeed", 0f);

            bool seMoviendo = velHorizontal > 0.5f;
            animator.SetFloat("Speed", seMoviendo ? 1f : 0f);
            animator.SetBool("Run", seMoviendo);
        }
    }

    // =====================================================================
    //  COLISIONES (Trigger de meta y hookpoints)
    // =====================================================================
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == meta)
        {
            SetReward(10f);
            EndEpisode();
        }
    }

    // =====================================================================
    //  GIZMOS (Visualizacion en editor)
    // =====================================================================
    private void OnDrawGizmosSelected()
    {
        // Rango de gancho
        Gizmos.color = new Color(0, 1, 1, 0.1f);
        Gizmos.DrawWireSphere(transform.position, _distanciaGancho > 0 ? _distanciaGancho : distanciaGanchoMin);

        // Suelo
        Gizmos.color = estaEnSuelo ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * 0.1f, groundCheckRadius);

        // Escalada
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * climbCheckDistance);

        // Objetivo gancho
        if (estaEnganchado && objetivoGancho != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, objetivoGancho);
            Gizmos.DrawWireSphere(objetivoGancho, 0.5f);
        }

        // Meta
        if (meta != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, meta.position);
        }
    }
}
