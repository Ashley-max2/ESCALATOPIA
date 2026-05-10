using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookSystem : MonoBehaviour
{
    private enum HookMode
    {
        PullPlayer,
        PullObject
    }

    [Header("Referencias")]
    private LineRenderer line;
    public Transform firePoint;
    public Transform crosshairSphere;

    [Header("Ajustes")]
    [SerializeField] private LayerMask hookLayer;
    [SerializeField] private LayerMask climbWallLayer;
    [SerializeField] private LayerMask pullableLayer;

    [SerializeField] private float hookRange = 30f;
    [SerializeField] private float hookSpeed = 40f;
    [SerializeField] private float pullObjectStopDistance = 1.5f; // distancia a la que el objeto se suelta

    [SerializeField] private HookMode currentMode = HookMode.PullPlayer;

    [HideInInspector] public Vector3 hookTarget;
    [HideInInspector] public bool isHooking;


    private Vector3 hookImpactPoint;
    private Vector3 hookImpactNormal;

    private void Awake()
    {
        line = GetComponentInChildren<LineRenderer>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchMode();
        }

        if (Input.GetMouseButton(1)) // SOLO si está apuntando
        {
            if (Input.GetMouseButtonDown(0))
            {
                TryHook();
            }
        }
    }

    void SwitchMode()
    {
        if (currentMode == HookMode.PullPlayer)
            currentMode = HookMode.PullObject;
        else
            currentMode = HookMode.PullPlayer;
    }

    void TryHook()
    {
        if (isHooking) return;

        Camera cam = Camera.main;

        Vector3 dir = crosshairSphere.position - cam.transform.position;
        Ray ray = new Ray(cam.transform.position, dir.normalized);

        if (currentMode == HookMode.PullPlayer)
        {
            if (Physics.Raycast(ray, out RaycastHit hit, hookRange, hookLayer))
            {
                hookTarget = hit.point;
                hookImpactPoint = hit.point;
                hookImpactNormal = hit.normal;

                StartCoroutine(HookMovement(hit));
            }
        }
        else // PullObject
        {
            if (Physics.Raycast(ray, out RaycastHit hit, hookRange, pullableLayer))
            {
                hookTarget = hit.point;
                StartCoroutine(PullObjectRoutine(hit));
            }
        }
    }

    IEnumerator PullObjectRoutine(RaycastHit hit)
    {
        isHooking = true;
        line.enabled = true;

        Rigidbody objectRb = hit.collider.GetComponent<Rigidbody>();

        if (objectRb == null)
        {
            StopHook();
            yield break;
        }

        float stopDistance = pullObjectStopDistance;

        while (Vector3.Distance(objectRb.position, transform.position) > stopDistance)
        {
            Vector3 dir = (transform.position - objectRb.position).normalized;

            objectRb.MovePosition(
                objectRb.position + dir * hookSpeed * Time.deltaTime
            );

            hookTarget = objectRb.position;

            yield return null;
        }

        // 🔥 Soltar automáticamente
        objectRb.velocity = Vector3.zero;
        objectRb.angularVelocity = Vector3.zero;

        StopHook();
    }

    IEnumerator HookMovement(RaycastHit hit)
    {
        isHooking = true;
        line.enabled = true;

        Rigidbody rb = GetComponent<Rigidbody>();


        Vector3 startPos = transform.position;
        Vector3 targetPos = hookTarget;

        Vector3 direction = (targetPos - startPos).normalized;
        float distance = Vector3.Distance(startPos, targetPos);

        float travelTime = distance / hookSpeed;
        float elapsed = 0f;

        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / travelTime);

            Vector3 nextPos = startPos + direction * (distance * t);

            if (rb != null)
                rb.MovePosition(nextPos);
            else
                transform.position = nextPos;

            yield return null;
        }

        if (rb != null)
            rb.MovePosition(targetPos);
        else
            transform.position = targetPos;

        Physics.SyncTransforms();

        // Climb transition ahora se gestiona desde PlayerHookState en la state machine

        yield return new WaitForFixedUpdate();

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Physics.SyncTransforms();

        // 🔥 Pequeña estabilización para evitar caída al llegar al hook
        yield return new WaitForFixedUpdate();

        StopHook();
    }



    void StopHook()
    {
        isHooking = false;
        line.enabled = false;
    }
}