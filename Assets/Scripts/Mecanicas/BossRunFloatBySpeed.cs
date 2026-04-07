using UnityEngine;

public class BossRunFloatBySpeed : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private string runFloatParameter = "Run";

    [Header("Speed Detection")]
    [SerializeField] private float movementThreshold = 0.1f;
    [SerializeField] private float speedForFullRun = 4f;
    [SerializeField] private bool ignoreYMovement = true;
    [SerializeField] private bool useRigidbodyVelocity = true;
    [SerializeField] private float smoothTime = 0.08f;

    private Rigidbody _rb;
    private Vector3 _lastPosition;
    private int _runParamHash;
    private float _runVelocity;
    private float _currentRun;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        _rb = GetComponent<Rigidbody>();

        _runParamHash = Animator.StringToHash(runFloatParameter);
    }

    private void OnEnable()
    {
        _lastPosition = transform.position;
        _currentRun = 0f;

        if (animator != null)
            animator.SetFloat(_runParamHash, 0f);
    }

    private void Update()
    {
        if (animator == null)
            return;

        float speed = GetCurrentSpeed();

        float targetRun = 0f;
        if (speed >= movementThreshold)
        {
            float denom = Mathf.Max(speedForFullRun - movementThreshold, 0.0001f);
            targetRun = Mathf.Clamp01((speed - movementThreshold) / denom);
        }

        if (smoothTime > 0f)
            _currentRun = Mathf.SmoothDamp(_currentRun, targetRun, ref _runVelocity, smoothTime);
        else
            _currentRun = targetRun;

        animator.SetFloat(_runParamHash, _currentRun);
    }

    private float GetCurrentSpeed()
    {
        if (useRigidbodyVelocity && _rb != null)
        {
            Vector3 velocity = _rb.velocity;
            if (ignoreYMovement)
                velocity.y = 0f;
            return velocity.magnitude;
        }

        Vector3 delta = transform.position - _lastPosition;
        _lastPosition = transform.position;

        if (ignoreYMovement)
            delta.y = 0f;

        return delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
    }
}
