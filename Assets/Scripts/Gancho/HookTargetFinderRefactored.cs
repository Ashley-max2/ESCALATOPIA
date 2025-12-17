using UnityEngine;

/// <summary>
/// Refactored HookTargetFinder implementing IHookTargetingService.
/// Single Responsibility: Finding and validating hook targets.
/// </summary>
public class HookTargetFinderRefactored : MonoBehaviour, IHookTargetingService
{
    [Header("Target Finding Settings")]
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private LayerMask hookPointsLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Camera targetCamera;

    [Header("Visual Feedback")]
    [SerializeField] private Color highlightColor = Color.cyan;
    [SerializeField] private float highlightIntensity = 1.5f;

    private IHookable currentTarget;
    private IHookable highlightedTarget;
    private Material originalMaterial;

    public IHookable CurrentTarget => currentTarget;
    public bool HasValidTarget => currentTarget != null && currentTarget.IsValidTarget;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        // Continuously search for targets
        Vector3 origin = transform.position;
        Vector3 direction = targetCamera.transform.forward;
        currentTarget = FindBestHookPoint(origin, direction, maxDistance);

        // Update visual feedback
        if (currentTarget != null)
        {
            HighlightTarget(currentTarget);
        }
        else
        {
            ClearHighlight();
        }
    }

    /// <summary>
    /// Find the best hook point within range and view.
    /// </summary>
    public IHookable FindBestHookPoint(Vector3 origin, Vector3 direction, float maxDistance)
    {
        Ray ray = new Ray(origin, direction);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, hookPointsLayer))
        {
            IHookable hookable = hit.collider.GetComponent<IHookable>();

            if (hookable != null && IsHookPointValid(hookable, origin))
            {
                return hookable;
            }
        }

        return null;
    }

    /// <summary>
    /// Check if a hook point is valid and reachable.
    /// </summary>
    public bool IsHookPointValid(IHookable hookPoint, Vector3 origin)
    {
        if (hookPoint == null || !hookPoint.IsValidTarget)
            return false;

        Vector3 targetPoint = hookPoint.HookPoint;
        float distance = Vector3.Distance(origin, targetPoint);

        // Check distance
        if (distance > maxDistance)
            return false;

        // Check for obstacles
        Vector3 direction = (targetPoint - origin).normalized;
        RaycastHit hit;

        if (Physics.Raycast(origin, direction, out hit, distance, obstacleLayer))
        {
            return false; // Obstacle in the way
        }

        return true;
    }

    /// <summary>
    /// Get all hook points within range.
    /// </summary>
    public IHookable[] GetHookPointsInRange(Vector3 origin, float maxDistance)
    {
        Collider[] colliders = Physics.OverlapSphere(origin, maxDistance, hookPointsLayer);
        System.Collections.Generic.List<IHookable> hookPoints = new System.Collections.Generic.List<IHookable>();

        foreach (Collider col in colliders)
        {
            IHookable hookable = col.GetComponent<IHookable>();
            if (hookable != null && IsHookPointValid(hookable, origin))
            {
                hookPoints.Add(hookable);
            }
        }

        return hookPoints.ToArray();
    }

    /// <summary>
    /// Highlight the currently targeted hook point.
    /// </summary>
    public void HighlightTarget(IHookable hookPoint)
    {
        if (hookPoint == highlightedTarget)
            return; // Already highlighted

        // Clear previous highlight
        ClearHighlight();

        // Highlight new target
        if (hookPoint != null)
        {
            GameObject targetObj = (hookPoint as MonoBehaviour)?.gameObject;
            if (targetObj != null)
            {
                Renderer renderer = targetObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // Store original material
                    originalMaterial = renderer.material;

                    // Apply highlight (simple color change)
                    Material highlightMat = new Material(originalMaterial);
                    highlightMat.color = highlightColor;
                    highlightMat.SetFloat("_EmissionIntensity", highlightIntensity);
                    renderer.material = highlightMat;
                }
            }

            highlightedTarget = hookPoint;
        }
    }

    /// <summary>
    /// Clear any target highlighting.
    /// </summary>
    public void ClearHighlight()
    {
        if (highlightedTarget != null)
        {
            GameObject targetObj = (highlightedTarget as MonoBehaviour)?.gameObject;
            if (targetObj != null)
            {
                Renderer renderer = targetObj.GetComponent<Renderer>();
                if (renderer != null && originalMaterial != null)
                {
                    renderer.material = originalMaterial;
                }
            }

            highlightedTarget = null;
            originalMaterial = null;
        }
    }

    private void OnDestroy()
    {
        ClearHighlight();
    }
}
