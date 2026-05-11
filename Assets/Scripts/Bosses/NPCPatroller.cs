using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// NPC que patrulla entre waypoints llevando una "linterna" (Spot Light).
/// Si el Player entra en el cono de la linterna, la escena se reinicia.
/// Los waypoints tienen luces que se encienden secuencialmente cuando el NPC se acerca.
///
/// Setup:
///   1. Crear un GameObject "NPCPatrol" y añadir este script + un modelo/sprite.
///   2. Crear hijos vacíos como waypoints (posiciones de patrulla) y arrastrarlos al array.
///   3. Opcionalmente, poner un Point Light hijo en cada waypoint para las luces secuenciales.
///   4. El script crea automáticamente un Spot Light hijo como "linterna".
/// </summary>
public class NPCPatroller : MonoBehaviour
{
    [Header("── Waypoints ──")]
    [Tooltip("Puntos de patrulla. El NPC irá de uno a otro en orden cíclico.")]
    public Transform[] waypoints;

    [Header("── Movimiento ──")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    public float reachDistance = 0.5f;
    public float waitTimeAtWaypoint = 1f;

    [Header("── Linterna (Detección) ──")]
    [Tooltip("Ángulo del cono de la linterna (grados). Si el player está dentro, muere.")]
    [Range(10f, 90f)]
    public float flashlightAngle = 35f;

    [Tooltip("Alcance máximo de la linterna en metros.")]
    public float flashlightRange = 12f;

    [Tooltip("Color de la linterna.")]
    public Color flashlightColor = new Color(1f, 0.95f, 0.8f, 1f);

    [Tooltip("Intensidad de la linterna.")]
    public float flashlightIntensity = 3f;

    [Tooltip("Layers que bloquean la linterna (paredes, etc). Si vacío, solo comprueba distancia y ángulo.")]
    public LayerMask obstacleMask;

    [Header("── Luces de Waypoints ──")]
    [Tooltip("Distancia a la que un waypoint se ilumina cuando el NPC se acerca.")]
    public float lightActivationDistance = 3f;
    public Color waypointLightColor = new Color(1f, 0.3f, 0.1f, 1f);
    public float waypointLightIntensity = 2f;

    [Header("── Animación (Opcional) ──")]
    public Animator npcAnimator;

    // ── Privados ──
    private int _currentWaypointIndex = 0;
    private bool _isWaiting = false;
    private Light[] _waypointLights;
    private Light _spotlight;
    private Transform _playerTransform;

    // ─────────────────────────────────────────────────────────────────────────
    private void Start()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("[NPCPatroller] No hay waypoints asignados en " + gameObject.name);
            enabled = false;
            return;
        }

        // Buscar al player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _playerTransform = playerObj.transform;

        // Crear la linterna (Spot Light) como hijo
        CreateFlashlight();

        // Recoger y configurar luces de waypoints
        SetupWaypointLights();
    }

    private void Update()
    {
        if (_isWaiting || waypoints.Length == 0) return;

        MoveToWaypoint();
        UpdateWaypointLights();
        CheckFlashlightDetection();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  LINTERNA
    // ─────────────────────────────────────────────────────────────────────────

    private void CreateFlashlight()
    {
        GameObject lightObj = new GameObject("Flashlight");
        lightObj.transform.SetParent(transform, false);
        lightObj.transform.localPosition = new Vector3(0, 1.2f, 0.3f); // Altura de mano

        _spotlight = lightObj.AddComponent<Light>();
        _spotlight.type = LightType.Spot;
        _spotlight.color = flashlightColor;
        _spotlight.intensity = flashlightIntensity;
        _spotlight.range = flashlightRange;
        _spotlight.spotAngle = flashlightAngle * 2f; // Unity usa el ángulo total
        _spotlight.innerSpotAngle = flashlightAngle;
        _spotlight.shadows = LightShadows.Soft;
    }

    private void CheckFlashlightDetection()
    {
        if (_playerTransform == null) return;

        Vector3 toPlayer = _playerTransform.position - transform.position;
        float distance = toPlayer.magnitude;

        // ¿Está dentro del rango?
        if (distance > flashlightRange) return;

        // ¿Está dentro del cono de la linterna?
        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > flashlightAngle) return;

        // ¿Hay algo bloqueando la visión? (pared entre NPC y player)
        if (obstacleMask != 0)
        {
            if (Physics.Raycast(transform.position + Vector3.up * 1.2f, toPlayer.normalized, distance, obstacleMask))
                return; // Hay una pared entre medias
        }

        // ¡Detectado! Reiniciar escena
        Debug.Log("[NPCPatroller] ¡Player detectado por la linterna! Reiniciando escena...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  MOVIMIENTO
    // ─────────────────────────────────────────────────────────────────────────

    private void MoveToWaypoint()
    {
        Transform target = waypoints[_currentWaypointIndex];
        if (target == null) { NextWaypoint(); return; }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        float distance = direction.magnitude;

        if (distance <= reachDistance)
        {
            StartCoroutine(WaitAndAdvance());
            return;
        }

        direction.Normalize();
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        if (npcAnimator != null)
            npcAnimator.SetFloat("Speed", moveSpeed);
    }

    private IEnumerator WaitAndAdvance()
    {
        _isWaiting = true;
        if (npcAnimator != null) npcAnimator.SetFloat("Speed", 0f);
        yield return new WaitForSeconds(waitTimeAtWaypoint);
        NextWaypoint();
        _isWaiting = false;
    }

    private void NextWaypoint()
    {
        _currentWaypointIndex = (_currentWaypointIndex + 1) % waypoints.Length;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  LUCES DE WAYPOINTS
    // ─────────────────────────────────────────────────────────────────────────

    private void SetupWaypointLights()
    {
        _waypointLights = new Light[waypoints.Length];
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            _waypointLights[i] = waypoints[i].GetComponentInChildren<Light>();
            if (_waypointLights[i] != null)
            {
                _waypointLights[i].enabled = false;
                _waypointLights[i].color = waypointLightColor;
                _waypointLights[i].intensity = waypointLightIntensity;
            }
        }
    }

    private void UpdateWaypointLights()
    {
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null || _waypointLights[i] == null) continue;

            float dist = Vector3.Distance(transform.position, waypoints[i].position);
            if (dist <= lightActivationDistance)
            {
                _waypointLights[i].enabled = true;
                float t = 1f - (dist / lightActivationDistance);
                _waypointLights[i].intensity = Mathf.Lerp(0f, waypointLightIntensity, t);
            }
            else
            {
                _waypointLights[i].enabled = false;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  GIZMOS
    // ─────────────────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        // Cono de la linterna
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Vector3 origin = transform.position + Vector3.up * 1.2f;
        Vector3 forward = transform.forward * flashlightRange;
        Gizmos.DrawRay(origin, forward);
        // Dibujar bordes del cono
        Quaternion leftRot = Quaternion.AngleAxis(-flashlightAngle, Vector3.up);
        Quaternion rightRot = Quaternion.AngleAxis(flashlightAngle, Vector3.up);
        Gizmos.DrawRay(origin, leftRot * transform.forward * flashlightRange);
        Gizmos.DrawRay(origin, rightRot * transform.forward * flashlightRange);

        // Waypoints
        if (waypoints == null || waypoints.Length == 0) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.2f);
            int next = (i + 1) % waypoints.Length;
            if (waypoints[next] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
        }
    }
}
