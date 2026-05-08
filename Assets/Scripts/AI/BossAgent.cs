using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class BossAgent : Agent
{
    [Header("=== TARGET ===")]
    public Transform target;
    
    [Header("=== CONFIG (RANGES) ===")]
    public float minWalkSpeed = 2f;
    public float maxWalkSpeed = 8f;
    public float minJumpForce = 5f;
    public float maxJumpForce = 15f;
    public float minMaxStamina = 50f;
    public float maxMaxStamina = 200f;
    
    private PlayerStateMachine _psm;
    private StaminaSystem _stamina;
    private GrapplingHook _hook;
    private AgarrarLanzarSoltar _grab;
    private PlayerInputHandler _input;
    private Rigidbody _rb;
    private Vector3 _initialPosition;
    private System.Collections.Generic.HashSet<string> _reachedMetas = new System.Collections.Generic.HashSet<string>();
    private Transform[] _metas;
    private int _nextMetaIndex = 0;

    public override void Initialize()
    {
        _psm = GetComponent<PlayerStateMachine>();
        _stamina = GetComponent<StaminaSystem>();
        _hook = GetComponentInChildren<GrapplingHook>();
        _grab = GetComponentInChildren<AgarrarLanzarSoltar>();
        _input = GetComponent<PlayerInputHandler>();
        _rb = GetComponent<Rigidbody>();
        _initialPosition = transform.position;
        
        _input.IsAI = true;
        _psm.OnDie += HandleDeath;
        _hook.OnHookAttached += HandleHookAttached;

        // Find and sort metas
        var metaObjs = GameObject.FindObjectsOfType<GameObject>();
        var metaList = new System.Collections.Generic.List<Transform>();
        for (int i = 1; i <= 5; i++)
        {
            foreach (var obj in metaObjs)
            {
                if (obj.name == "Meta" + i)
                {
                    metaList.Add(obj.transform);
                    break;
                }
            }
        }
        _metas = metaList.ToArray();
    }

    private void OnDestroy()
    {
        if (_psm != null) _psm.OnDie -= HandleDeath;
        if (_hook != null) _hook.OnHookAttached -= HandleHookAttached;
    }

    private void HandleHookAttached()
    {
        AddReward(10f);
        Debug.Log("BossAgent: Hook Reward (+10)");
    }

    private void HandleDeath()
    {
        SetReward(-1f);
        EndEpisode();
    }

    public override void OnEpisodeBegin()
    {
        if (_psm == null) _psm = GetComponent<PlayerStateMachine>();
        if (_stamina == null) _stamina = GetComponent<StaminaSystem>();
        if (_hook == null) _hook = GetComponentInChildren<GrapplingHook>();
        if (_grab == null) _grab = GetComponentInChildren<AgarrarLanzarSoltar>();
        if (_rb == null) _rb = GetComponent<Rigidbody>();

        // Randomize mechanics as requested
        if (_psm != null)
        {
            _psm.SetWalkSpeed(Random.Range(minWalkSpeed, maxWalkSpeed));
            _psm.SetRunSpeed(Random.Range(_psm.WalkSpeed, _psm.WalkSpeed * 2f));
            _psm.SetJumpForce(Random.Range(minJumpForce, maxJumpForce));
            _psm.SetClimbSpeed(Random.Range(2f, 7f));
            _psm.SetClimbStaminaCost(Random.Range(1f, 10f));
            _psm.SetFallMultiplier(Random.Range(1.5f, 4f));
            _psm.SetHookAccelerationTime(Random.Range(0.1f, 0.5f));
        }

        if (_stamina != null)
        {
            _stamina.SetMaxStamina(Random.Range(minMaxStamina, maxMaxStamina));
            _stamina.SetRegenRate(Random.Range(5f, 30f));
            _stamina.RestoreStamina();
        }

        if (_hook != null)
        {
            _hook.SetMaxRange(Random.Range(15f, 50f));
            _hook.SetTravelSpeed(Random.Range(15f, 45f));
            _hook.SetPullSpeed(Random.Range(10f, 30f));
        }

        if (_grab != null)
        {
            _grab.SetDistanciaMax(Random.Range(2f, 6f));
            _grab.SetFuerzaLanzamiento(Random.Range(5f, 25f));
        }

        // Reset position and state
        transform.position = _initialPosition + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        
        // Transition to Grounded state (bypassing death scene)
        if (_psm != null)
        {
            _psm.TransitionToState(_psm.States.Grounded());
        }
        
        _reachedMetas.Clear();
        _nextMetaIndex = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. Next Meta Direction
        if (_metas != null && _nextMetaIndex < _metas.Length)
        {
            Vector3 dirToMeta = _metas[_nextMetaIndex].position - transform.position;
            sensor.AddObservation(dirToMeta.normalized); // 3
            sensor.AddObservation(Mathf.Clamp01(dirToMeta.magnitude / 100f)); // 1
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
        }

        // 2. Own state
        sensor.AddObservation(_stamina != null ? _stamina.StaminaPercent : 1f); // 1
        sensor.AddObservation(_psm.IsGrounded ? 1f : 0f); // 1
        sensor.AddObservation(_psm.IsClimbing ? 1f : 0f); // 1
        sensor.AddObservation(_psm.IsHooking ? 1f : 0f); // 1
        
        // 3. Velocity
        sensor.AddObservation(_rb.velocity / 20f); // 3
        
        // 4. Target Player relative pos (if any)
        if (target != null)
        {
            Vector3 dirToTarget = target.position - transform.position;
            sensor.AddObservation(dirToTarget.normalized); // 3
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
        }
        // Total obs: 3+1+1+1+1+1+3+3 = 14
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Continuous actions: [0]=MoveX, [1]=MoveZ, [2]=CameraX, [3]=CameraY
        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        _input.SetAIMovement(moveX, moveZ);

        float camX = actions.ContinuousActions[2];
        float camY = actions.ContinuousActions[3];
        _input.SetAICamera(camX, camY);

        // Discrete actions: [0]=Jump, [1]=Hook, [2]=ReleaseHook, [3]=Sprint
        bool jump = actions.DiscreteActions[0] == 1;
        bool jumpHeld = actions.DiscreteActions[0] == 2;
        _input.SetAIJump(jump, jumpHeld);
        
        bool hook = actions.DiscreteActions[1] == 1;
        bool release = actions.DiscreteActions[2] == 1;
        _input.SetAIHook(hook, release);

        bool sprint = actions.DiscreteActions[3] == 1;
        _input.SetAISprint(sprint);

        bool interact = actions.DiscreteActions[4] == 1;
        _input.SetAIInteract(interact);

        // Jump Reward
        if (jump && _psm.IsGrounded)
        {
            AddReward(0.5f);
        }

        // Rewards logic
        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            
            // Encourage being near the player (but not necessarily touching if it's a ranged boss)
            // This is just a basic template, the user can refine this.
            AddReward(-0.001f); // Existence penalty to encourage efficiency
            
            if (distance < 5f)
                AddReward(0.01f);
        }

        // Falling off the world
        if (transform.position.y < -20f)
        {
            SetReward(-1f);
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Death Zone
        if (other.gameObject.name == "Muerte")
        {
            SetReward(-1f);
            EndEpisode();
            return;
        }

        // Metas
        if (other.gameObject.name.StartsWith("Meta"))
        {
            if (!_reachedMetas.Contains(other.gameObject.name))
            {
                AddReward(100f);
                _reachedMetas.Add(other.gameObject.name);
                Debug.Log($"BossAgent: Reached {other.gameObject.name} (+100)");
                
                // Increment index if it matches the current next meta
                if (other.gameObject.name == "Meta" + (_nextMetaIndex + 1))
                {
                    _nextMetaIndex++;
                }
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuous = actionsOut.ContinuousActions;
        continuous[0] = Input.GetAxisRaw("Horizontal");
        continuous[1] = Input.GetAxisRaw("Vertical");
        continuous[2] = Input.GetAxis("Mouse X");
        continuous[3] = Input.GetAxis("Mouse Y");

        var discrete = actionsOut.DiscreteActions;
        discrete[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
        discrete[1] = Input.GetMouseButtonDown(1) ? 1 : 0;
        discrete[2] = Input.GetMouseButtonDown(0) ? 1 : 0;
        discrete[3] = Input.GetKey(KeyCode.LeftShift) ? 1 : 0;
        discrete[4] = Input.GetKeyDown(KeyCode.E) ? 1 : 0;
    }
}
