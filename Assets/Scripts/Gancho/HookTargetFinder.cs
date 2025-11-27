using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookTargetFinder : MonoBehaviour
{
    [Header("Target Finding")]
    public float maxDistance = 20f;
    public LayerMask hookPointsLayer;
    public LayerMask obstacleLayer;
    public Camera firstPersonCamera;

    public IHookable CurrentTarget { get; private set; }
    public bool HasValidTarget => CurrentTarget != null && CurrentTarget.IsValidTarget;

    private HookSystem hookSystem;

    void Start()
    {
        hookSystem = GetComponent<HookSystem>();
    }

    void Update()
    {
        FindTarget();
    }

    public void FindTarget()
    {
        if (firstPersonCamera == null) return;

        Ray ray = firstPersonCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, hookPointsLayer))
        {
            IHookable hookable = hit.collider.GetComponent<IHookable>();

            if (hookable != null && hookable.IsValidTarget && !HasObstacles(hit.point))
            {
                CurrentTarget = hookable;
                return;
            }
        }

        CurrentTarget = null;
    }

    private bool HasObstacles(Vector3 targetPoint)
    {
        Vector3 origin = hookSystem.PlayerPosition + Vector3.up;
        float distance = Vector3.Distance(origin, targetPoint);

        return Physics.Raycast(origin, (targetPoint - origin).normalized, distance, obstacleLayer);
    }
}