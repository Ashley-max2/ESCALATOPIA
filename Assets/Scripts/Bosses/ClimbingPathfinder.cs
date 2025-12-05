using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Representa un punto de escalada en la ruta
/// </summary>
[System.Serializable]
public class ClimbPoint
{
    public Vector3 position;
    public ClimbPointType type;
    public float difficulty; // 0-1, donde 1 es más difícil
    public List<ClimbPoint> connections = new List<ClimbPoint>();

    public ClimbPoint(Vector3 pos, ClimbPointType pointType, float diff = 0.5f)
    {
        position = pos;
        type = pointType;
        difficulty = diff;
    }
}

public enum ClimbPointType
{
    Handhold,      // Agarre de mano
    Foothold,      // Apoyo de pie
    HookPoint,     // Punto de gancho
    LedgeRest,     // Repisa para descansar
    Checkpoint     // Punto de control
}

/// <summary>
/// Sistema de pathfinding para encontrar rutas de escalada
/// </summary>
public class ClimbingPathfinder : MonoBehaviour
{
    [Header("Pathfinding Settings")]
    [SerializeField] private float maxClimbDistance = 2f;
    [SerializeField] private float pointSpacing = 1.5f; // Espaciado entre puntos generados
    [SerializeField] private bool debugMode = false;

    [Header("Layer Detection")]
    [SerializeField] private LayerMask agarreLayer; // Layer para puntos de gancho
    [SerializeField] private LayerMask paredLayer; // Layer para paredes escalables
    [SerializeField] private LayerMask sueloLayer; // Layer para superficies caminables
    
    [Header("Tag Detection")]
    [SerializeField] private string escalableTag = "escalable"; // Tag para objetos escalables

    private List<ClimbPoint> climbPoints = new List<ClimbPoint>();
    private Dictionary<ClimbPoint, float> gScore = new Dictionary<ClimbPoint, float>();
    private Dictionary<ClimbPoint, float> fScore = new Dictionary<ClimbPoint, float>();
    private bool isInitialized = false;

    /// <summary>
    /// Inicializa los puntos de escalada automáticamente desde layers y tags
    /// </summary>
    [ContextMenu("Initialize Climb Points")]
    public void InitializeClimbPoints()
    {
        climbPoints.Clear();
        
        Debug.Log("=== Iniciando detección automática de puntos de escalada ===");

        // 1. Detectar puntos de agarre (HookPoints) por layer
        DetectHookPoints();

        // 2. Detectar superficies escalables por tag y layer
        DetectClimbableSurfaces();

        // 3. Detectar superficies caminables (suelo)
        DetectWalkableSurfaces();

        // 4. Conectar puntos cercanos
        ConnectClimbPoints();

        isInitialized = true;

        if (debugMode)
        {
            Debug.Log($"<color=green>✓ ClimbingPathfinder inicializado: {climbPoints.Count} puntos detectados</color>");
            LogPointStatistics();
        }
    }

    /// <summary>
    /// Detecta puntos de gancho basándose en la layer "agarre"
    /// </summary>
    private void DetectHookPoints()
    {
        // Buscar todos los colliders en la layer de agarre
        Collider[] hookColliders = FindCollidersInLayer(agarreLayer);
        
        foreach (Collider col in hookColliders)
        {
            // Crear punto en el centro del collider
            Vector3 position = col.bounds.center;
            
            ClimbPoint point = new ClimbPoint(position, ClimbPointType.HookPoint, 0.6f);
            climbPoints.Add(point);
        }

        if (debugMode)
            Debug.Log($"→ Detectados {hookColliders.Length} puntos de gancho (HookPoints)");
    }

    /// <summary>
    /// Detecta superficies escalables por tag "escalable" o layer "pared"
    /// </summary>
    private void DetectClimbableSurfaces()
    {
        int pointsAdded = 0;

        // Buscar por tag "escalable"
        GameObject[] escalableObjects = GameObject.FindGameObjectsWithTag(escalableTag);
        
        foreach (GameObject obj in escalableObjects)
        {
            pointsAdded += GenerateClimbPointsOnSurface(obj, ClimbPointType.Handhold);
        }

        // Buscar por layer "pared"
        Collider[] wallColliders = FindCollidersInLayer(paredLayer);
        
        foreach (Collider col in wallColliders)
        {
            // Evitar duplicados si ya tiene el tag
            if (!col.CompareTag(escalableTag))
            {
                pointsAdded += GenerateClimbPointsOnSurface(col.gameObject, ClimbPointType.Handhold);
            }
        }

        if (debugMode)
            Debug.Log($"→ Generados {pointsAdded} puntos de escalada (Handholds)");
    }

    /// <summary>
    /// Detecta superficies caminables basándose en la layer "suelo"
    /// </summary>
    private void DetectWalkableSurfaces()
    {
        int pointsAdded = 0;
        
        Collider[] groundColliders = FindCollidersInLayer(sueloLayer);
        
        foreach (Collider col in groundColliders)
        {
            pointsAdded += GenerateWalkPointsOnGround(col);
        }

        if (debugMode)
            Debug.Log($"→ Generados {pointsAdded} puntos de camino (Ground)");
    }

    /// <summary>
    /// Genera puntos de escalada en una superficie dada
    /// </summary>
    private int GenerateClimbPointsOnSurface(GameObject surface, ClimbPointType pointType)
    {
        Collider collider = surface.GetComponent<Collider>();
        if (collider == null) return 0;

        Bounds bounds = collider.bounds;
        int pointsGenerated = 0;

        // Limitar el número de puntos por objeto
        float spacing = Mathf.Max(pointSpacing, 1.5f);
        
        // Limitar el área de generación si es muy grande
        int maxPointsPerAxis = 20;
        float xStep = Mathf.Max(spacing, bounds.size.x / maxPointsPerAxis);
        float yStep = Mathf.Max(spacing, bounds.size.y / maxPointsPerAxis);
        
        // Generar grid de puntos en la superficie
        for (float y = bounds.min.y; y <= bounds.max.y; y += yStep)
        {
            for (float x = bounds.min.x; x <= bounds.max.x; x += xStep)
            {
                // Solo probar un raycast por posición (optimizado)
                Vector3 testPosition = new Vector3(x, y, bounds.center.z);
                
                // Raycast para encontrar el punto exacto en la superficie
                RaycastHit hit;
                Vector3 rayOrigin = testPosition + Vector3.forward * (bounds.extents.z + 0.5f);
                float rayDistance = (bounds.extents.z * 2) + 1f;
                
                if (Physics.Raycast(rayOrigin, Vector3.back, out hit, rayDistance))
                {
                    if (hit.collider == collider)
                    {
                        // Crear punto ligeramente separado de la superficie
                        Vector3 pointPosition = hit.point + hit.normal * 0.1f;
                        
                        // Determinar dificultad basada en altura
                        float heightRatio = (y - bounds.min.y) / Mathf.Max(bounds.size.y, 0.1f);
                        float difficulty = Mathf.Lerp(0.3f, 0.8f, heightRatio);
                        
                        // Determinar tipo específico según altura
                        ClimbPointType finalType = pointType;
                        if (heightRatio > 0.8f && Random.value < 0.2f)
                        {
                            finalType = ClimbPointType.LedgeRest; // Repisas cerca del tope
                        }
                        
                        ClimbPoint point = new ClimbPoint(pointPosition, finalType, difficulty);
                        climbPoints.Add(point);
                        pointsGenerated++;
                    }
                }
                
                // Limitar total de puntos para evitar lag
                if (pointsGenerated > 100)
                {
                    if (debugMode)
                        Debug.LogWarning($"Límite de 100 puntos alcanzado para {surface.name}. Considera reducir Point Spacing.");
                    return pointsGenerated;
                }
            }
        }

        return pointsGenerated;
    }

    /// <summary>
    /// Genera puntos de camino en superficies de suelo
    /// </summary>
    private int GenerateWalkPointsOnGround(Collider groundCollider)
    {
        Bounds bounds = groundCollider.bounds;
        int pointsGenerated = 0;

        // Espaciado mayor para puntos de camino
        float walkSpacing = Mathf.Max(pointSpacing * 2f, 3f);
        
        // Limitar puntos en suelos muy grandes
        int maxPointsPerAxis = 15;
        float xStep = Mathf.Max(walkSpacing, bounds.size.x / maxPointsPerAxis);
        float zStep = Mathf.Max(walkSpacing, bounds.size.z / maxPointsPerAxis);
        
        // Generar puntos en la superficie superior del suelo
        for (float x = bounds.min.x; x <= bounds.max.x; x += xStep)
        {
            for (float z = bounds.min.z; z <= bounds.max.z; z += zStep)
            {
                Vector3 testPosition = new Vector3(x, bounds.max.y + 1f, z);
                
                // Raycast hacia abajo para encontrar la superficie
                RaycastHit hit;
                if (Physics.Raycast(testPosition, Vector3.down, out hit, 2f, sueloLayer))
                {
                    if (hit.collider == groundCollider)
                    {
                        Vector3 pointPosition = hit.point + Vector3.up * 0.1f;
                        
                        // Los puntos de suelo son fáciles
                        ClimbPoint point = new ClimbPoint(pointPosition, ClimbPointType.Foothold, 0.1f);
                        climbPoints.Add(point);
                        pointsGenerated++;
                    }
                }
                
                // Limitar total de puntos
                if (pointsGenerated > 50)
                {
                    if (debugMode)
                        Debug.LogWarning($"Límite de 50 puntos de suelo alcanzado para {groundCollider.name}");
                    return pointsGenerated;
                }
            }
        }

        return pointsGenerated;
    }

    /// <summary>
    /// Encuentra todos los colliders en una layer específica
    /// </summary>
    private Collider[] FindCollidersInLayer(LayerMask layerMask)
    {
        List<Collider> collidersInLayer = new List<Collider>();
        Collider[] allColliders = FindObjectsOfType<Collider>();

        foreach (Collider col in allColliders)
        {
            // Verificar si el objeto está en la layer especificada
            if (((1 << col.gameObject.layer) & layerMask) != 0)
            {
                collidersInLayer.Add(col);
            }
        }

        return collidersInLayer.ToArray();
    }

    /// <summary>
    /// Muestra estadísticas de los puntos detectados
    /// </summary>
    private void LogPointStatistics()
    {
        int handholds = 0, hookPoints = 0, ledges = 0, footholds = 0;
        
        foreach (ClimbPoint point in climbPoints)
        {
            switch (point.type)
            {
                case ClimbPointType.Handhold: handholds++; break;
                case ClimbPointType.HookPoint: hookPoints++; break;
                case ClimbPointType.LedgeRest: ledges++; break;
                case ClimbPointType.Foothold: footholds++; break;
            }
        }

        Debug.Log($"  • Handholds (escalada): {handholds}");
        Debug.Log($"  • HookPoints (gancho): {hookPoints}");
        Debug.Log($"  • LedgeRest (repisas): {ledges}");
        Debug.Log($"  • Footholds (suelo): {footholds}");
    }

    /// <summary>
    /// Determina la distancia máxima de conexión según tipos de punto
    /// </summary>
    private float GetMaxConnectionDistance(ClimbPoint point1, ClimbPoint point2)
    {
        // HookPoints pueden conectarse a mayor distancia
        if (point1.type == ClimbPointType.HookPoint || point2.type == ClimbPointType.HookPoint)
        {
            return maxClimbDistance * 2.5f; // Ganchos alcanzan más lejos
        }

        // Puntos de suelo pueden conectarse a mayor distancia (caminar/correr)
        if (point1.type == ClimbPointType.Foothold && point2.type == ClimbPointType.Foothold)
        {
            return maxClimbDistance * 3f; // Correr en suelo
        }

        // Conexión desde suelo a pared (inicio de escalada)
        if ((point1.type == ClimbPointType.Foothold && point2.type == ClimbPointType.Handhold) ||
            (point1.type == ClimbPointType.Handhold && point2.type == ClimbPointType.Foothold))
        {
            return maxClimbDistance * 1.5f;
        }

        // Distancia estándar para escalada
        return maxClimbDistance;
    }

    /// <summary>
    /// Encuentra la mejor ruta usando A*
    /// </summary>
    public List<ClimbPoint> FindPath(Vector3 start, Vector3 goal, float difficultyModifier = 1f)
    {
        ClimbPoint startPoint = GetNearestClimbPoint(start);
        ClimbPoint goalPoint = GetNearestClimbPoint(goal);

        if (startPoint == null || goalPoint == null)
        {
            if (debugMode) Debug.LogWarning("No se encontraron puntos de inicio o destino");
            return null;
        }

        return AStar(startPoint, goalPoint, difficultyModifier);
    }

    /// <summary>
    /// Algoritmo A* para pathfinding
    /// </summary>
    private List<ClimbPoint> AStar(ClimbPoint start, ClimbPoint goal, float difficultyModifier)
    {
        HashSet<ClimbPoint> closedSet = new HashSet<ClimbPoint>();
        HashSet<ClimbPoint> openSet = new HashSet<ClimbPoint> { start };
        Dictionary<ClimbPoint, ClimbPoint> cameFrom = new Dictionary<ClimbPoint, ClimbPoint>();

        gScore.Clear();
        fScore.Clear();

        gScore[start] = 0;
        fScore[start] = HeuristicCostEstimate(start, goal);

        while (openSet.Count > 0)
        {
            ClimbPoint current = GetLowestFScore(openSet);

            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (ClimbPoint neighbor in current.connections)
            {
                if (closedSet.Contains(neighbor)) continue;

                float tentativeGScore = gScore[current] + DistanceCost(current, neighbor, difficultyModifier);

                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                else if (tentativeGScore >= gScore.GetValueOrDefault(neighbor, float.MaxValue))
                {
                    continue;
                }

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, goal);
            }
        }

        if (debugMode) Debug.LogWarning("No se encontró camino válido");
        return null;
    }

    /// <summary>
    /// Calcula el costo heurístico (distancia al objetivo)
    /// </summary>
    private float HeuristicCostEstimate(ClimbPoint a, ClimbPoint b)
    {
        // Distancia euclidiana con penalización por altura
        float distance = Vector3.Distance(a.position, b.position);
        float heightDiff = Mathf.Abs(b.position.y - a.position.y);
        
        return distance + (heightDiff * 0.5f); // Penalizar subidas
    }

    /// <summary>
    /// Calcula el costo de movimiento entre dos puntos
    /// </summary>
    private float DistanceCost(ClimbPoint a, ClimbPoint b, float difficultyModifier)
    {
        float baseCost = Vector3.Distance(a.position, b.position);
        float difficultyCost = b.difficulty * difficultyModifier;
        
        return baseCost + difficultyCost;
    }

    /// <summary>
    /// Obtiene el punto con menor F score
    /// </summary>
    private ClimbPoint GetLowestFScore(HashSet<ClimbPoint> set)
    {
        ClimbPoint lowest = null;
        float lowestScore = float.MaxValue;

        foreach (ClimbPoint point in set)
        {
            float score = fScore.GetValueOrDefault(point, float.MaxValue);
            if (score < lowestScore)
            {
                lowestScore = score;
                lowest = point;
            }
        }

        return lowest;
    }

    /// <summary>
    /// Reconstruye el camino desde el objetivo hasta el inicio
    /// </summary>
    private List<ClimbPoint> ReconstructPath(Dictionary<ClimbPoint, ClimbPoint> cameFrom, ClimbPoint current)
    {
        List<ClimbPoint> path = new List<ClimbPoint> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }

    /// <summary>
    /// Encuentra el punto de escalada más cercano a una posición
    /// </summary>
    public ClimbPoint GetNearestClimbPoint(Vector3 position)
    {
        ClimbPoint nearest = null;
        float minDistance = float.MaxValue;

        foreach (ClimbPoint point in climbPoints)
        {
            float distance = Vector3.Distance(position, point.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = point;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Encuentra puntos de gancho cercanos
    /// </summary>
    public List<ClimbPoint> GetNearbyHookPoints(Vector3 position, float radius)
    {
        List<ClimbPoint> hookPoints = new List<ClimbPoint>();

        foreach (ClimbPoint point in climbPoints)
        {
            if (point.type == ClimbPointType.HookPoint)
            {
                if (Vector3.Distance(position, point.position) <= radius)
                {
                    hookPoints.Add(point);
                }
            }
        }

        return hookPoints;
    }

    /// <summary>
    /// Debug visual
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!debugMode || climbPoints == null) return;

        // Dibujar puntos
        foreach (ClimbPoint point in climbPoints)
        {
            switch (point.type)
            {
                case ClimbPointType.Handhold:
                    Gizmos.color = Color.green;
                    break;
                case ClimbPointType.HookPoint:
                    Gizmos.color = Color.cyan;
                    break;
                case ClimbPointType.LedgeRest:
                    Gizmos.color = Color.yellow;
                    break;
                case ClimbPointType.Foothold:
                    Gizmos.color = Color.blue;
                    break;
                case ClimbPointType.Checkpoint:
                    Gizmos.color = Color.magenta;
                    break;
                default:
                    Gizmos.color = Color.white;
                    break;
            }

            Gizmos.DrawSphere(point.position, 0.2f);

            // Dibujar conexiones
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            foreach (ClimbPoint connection in point.connections)
            {
                Gizmos.DrawLine(point.position, connection.position);
            }
        }
    }

    /// <summary>
    /// Obtiene información del tipo de superficie en una posición
    /// </summary>
    public SurfaceType GetSurfaceTypeAt(Vector3 position)
    {
        ClimbPoint nearest = GetNearestClimbPoint(position);
        if (nearest == null) return SurfaceType.Unknown;

        float distance = Vector3.Distance(position, nearest.position);
        if (distance > maxClimbDistance) return SurfaceType.Unknown;

        switch (nearest.type)
        {
            case ClimbPointType.Foothold:
                return SurfaceType.Ground;
            case ClimbPointType.Handhold:
            case ClimbPointType.LedgeRest:
                return SurfaceType.Climbable;
            case ClimbPointType.HookPoint:
                return SurfaceType.Hookable;
            default:
                return SurfaceType.Unknown;
        }
    }

    /// <summary>
    /// Verifica si el pathfinder está inicializado
    /// </summary>
    public bool IsInitialized()
    {
        return isInitialized && climbPoints.Count > 0;
    }

    /// <summary>
    /// Conecta automáticamente puntos de escalada cercanos
    /// Considera el tipo de punto para determinar conexiones válidas
    /// </summary>
    private void ConnectClimbPoints()
    {
        int connectionsCreated = 0;
        int totalPoints = climbPoints.Count;

        // Optimización: limitar búsqueda si hay muchos puntos
        if (totalPoints > 500)
        {
            Debug.LogWarning($"Muchos puntos detectados ({totalPoints}). Aumenta Point Spacing para mejor rendimiento.");
        }

        for (int i = 0; i < totalPoints; i++)
        {
            // Mostrar progreso cada 50 puntos
            if (debugMode && i % 50 == 0)
            {
                Debug.Log($"Conectando puntos... {i}/{totalPoints}");
            }

            // Optimización: solo conectar con puntos cercanos usando distancia máxima extendida
            float maxSearchDistance = maxClimbDistance * 3f;

            for (int j = i + 1; j < totalPoints; j++)
            {
                // Pre-filtro rápido por distancia antes de cálculo exacto
                Vector3 offset = climbPoints[j].position - climbPoints[i].position;
                float sqrDistance = offset.sqrMagnitude;
                float maxSqrDistance = maxSearchDistance * maxSearchDistance;

                if (sqrDistance > maxSqrDistance)
                    continue;

                float distance = Mathf.Sqrt(sqrDistance);
                
                // Determinar distancia máxima según tipos de punto
                float maxDistance = GetMaxConnectionDistance(climbPoints[i], climbPoints[j]);
                
                if (distance <= maxDistance)
                {
                    // Verificar línea de visión (sin obstáculos entre puntos)
                    if (!Physics.Linecast(climbPoints[i].position, climbPoints[j].position))
                    {
                        climbPoints[i].connections.Add(climbPoints[j]);
                        climbPoints[j].connections.Add(climbPoints[i]);
                        connectionsCreated++;
                    }
                }
            }
        }

        if (debugMode)
            Debug.Log($"→ Creadas {connectionsCreated} conexiones entre puntos");
    }
}

/// <summary>
/// Tipos de superficie para la IA
/// </summary>
public enum SurfaceType
{
    Unknown,
    Ground,      // Puede caminar/correr
    Climbable,   // Puede escalar
    Hookable     // Puede usar gancho
}

/// <summary>
/// Componente para marcar puntos de escalada en la escena
/// </summary>
public class ClimbPointMarker : MonoBehaviour
{
    public ClimbPointType pointType = ClimbPointType.Handhold;
    [Range(0f, 1f)]
    public float difficulty = 0.5f;

    private void OnDrawGizmos()
    {
        Color color = Color.white;
        switch (pointType)
        {
            case ClimbPointType.Handhold: color = Color.green; break;
            case ClimbPointType.HookPoint: color = Color.cyan; break;
            case ClimbPointType.LedgeRest: color = Color.yellow; break;
            case ClimbPointType.Foothold: color = Color.blue; break;
            case ClimbPointType.Checkpoint: color = Color.magenta; break;
        }

        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
