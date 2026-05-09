using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum TerrainBrush
{
    Raise,
    Dig,
    Path,
    Ramp,
    Eraser
}

/// <summary>
/// Grid terrain editor component. Place on a GameObject in the scene.
/// Provides brush tools for painting terrain via the custom inspector and SceneView.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class TerrainEditor : MonoBehaviour
{
    [Header("Grid Data")]
    public GridData gridData;

    [Header("Brush Settings")]
    public TerrainBrush currentBrush = TerrainBrush.Raise;
    [Range(1, 5)]
    public int brushSize = 1;

    [Header("Mesh Settings")]
    public float cellSize = 1f;
    public float heightStep = 1f;

    [Header("Save / Load")]
    public string binaryFilePath = "terrain_grid.bin";

    // Runtime
    [System.NonSerialized] public int hoveredX = -1;
    [System.NonSerialized] public int hoveredZ = -1;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    void OnEnable()
    {
        meshFilter   = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        EnsureMaterial();
        RebuildMesh();
    }

    /// <summary>Creates a vertex-color material using the custom terrain shader.</summary>
    void EnsureMaterial()
    {
        // Force recreation if material is missing, uses wrong shader, or shader is broken
        bool needsNew = meshRenderer.sharedMaterial == null;
        if (!needsNew)
        {
            Shader current = meshRenderer.sharedMaterial.shader;
            needsNew = current == null ||
                       current.name == "Hidden/InternalErrorShader" ||
                       current.name == "Standard" ||
                       current.name != "Custom/VertexColorTerrain";
        }

        if (!needsNew) return;

        // Try our custom URP shader first, then URP fallbacks
        Shader shader = Shader.Find("Custom/VertexColorTerrain");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        Material mat = new Material(shader);
        mat.name = "TerrainVertexColor";
        mat.color = Color.white;

        meshRenderer.sharedMaterial = mat;
    }

    // ─── Mesh Rebuild ────────────────────────────────────────────

    public void RebuildMesh()
    {
        if (gridData == null) return;

        MeshGenerator.cellSize   = cellSize;
        MeshGenerator.heightStep = heightStep;

        Mesh newMesh = MeshGenerator.BuildFullMesh(gridData);
        meshFilter.sharedMesh   = newMesh;
        meshCollider.sharedMesh = newMesh; // Keep collider in sync for raycasting
    }

    // ─── Brush Application ───────────────────────────────────────

    public void ApplyBrush(int centerX, int centerZ)
    {
        if (gridData == null) return;

#if UNITY_EDITOR
        Undo.RecordObject(gridData, "Terrain Brush");
#endif

        int radius = brushSize - 1;

        for (int dz = -radius; dz <= radius; dz++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                int x = centerX + dx;
                int z = centerZ + dz;
                if (!gridData.InBounds(x, z)) continue;

                switch (currentBrush)
                {
                    case TerrainBrush.Raise:  ApplyRaise(x, z);  break;
                    case TerrainBrush.Dig:    ApplyDig(x, z);    break;
                    case TerrainBrush.Path:   ApplyPath(x, z);   break;
                    case TerrainBrush.Ramp:   ApplyRamp(x, z);   break;
                    case TerrainBrush.Eraser: ApplyEraser(x, z); break;
                }
            }
        }

        gridData.RecalculateAllBitmasks();
        gridData.FlushToSerialized();

#if UNITY_EDITOR
        EditorUtility.SetDirty(gridData);
#endif

        RebuildMesh();
    }

    void ApplyRaise(int x, int z)
    {
        int h = gridData.GetHeight(x, z);
        gridData.SetHeight(x, z, h + 1);
        CellType t = gridData.GetCellType(x, z);
        if (t == CellType.Lake || t == CellType.Water)
            gridData.SetCellType(x, z, CellType.Grass);
    }

    void ApplyDig(int x, int z)
    {
        int h = gridData.GetHeight(x, z);
        gridData.SetHeight(x, z, h - 1);
        if (h - 1 < 0)
            gridData.SetCellType(x, z, CellType.Lake);
    }

    void ApplyPath(int x, int z)
    {
        CellType current = gridData.GetCellType(x, z);
        if (current == CellType.Path)
            gridData.SetCellType(x, z, CellType.Grass); // Toggle off
        else
            gridData.SetCellType(x, z, CellType.Path);
    }

    void ApplyRamp(int x, int z)
    {
        int h = gridData.GetHeight(x, z);
        int[] dx = { 0, 1, 0, -1 };
        int[] dz = { 1, 0, -1, 0 };

        // Try descending first (this cell is higher)
        for (int d = 0; d < 4; d++)
        {
            int nx = x + dx[d];
            int nz = z + dz[d];
            int nh = gridData.GetHeight(nx, nz);
            if (h - nh == 1)
            {
                gridData.SetCellType(x, z, CellType.Ramp);
                gridData.SetDirection(x, z, (byte)d);
                return;
            }
        }

        // Try ascending (this cell is lower — set height to neighbor's so ramp descends properly)
        for (int d = 0; d < 4; d++)
        {
            int nx = x + dx[d];
            int nz = z + dz[d];
            int nh = gridData.GetHeight(nx, nz);
            if (nh - h == 1)
            {
                // Opposite direction: ramp is placed at neighbor height, descending toward us
                int oppositeDir = (d + 2) % 4;
                gridData.SetHeight(x, z, nh);
                gridData.SetCellType(x, z, CellType.Ramp);
                gridData.SetDirection(x, z, (byte)oppositeDir);
                return;
            }
        }

        // No ramp possible — toggle off if already a ramp
        if (gridData.GetCellType(x, z) == CellType.Ramp)
        {
            gridData.SetCellType(x, z, CellType.Grass);
            gridData.SetDirection(x, z, 0);
        }
    }

    void ApplyEraser(int x, int z)
    {
        gridData.SetHeight(x, z, 0);
        gridData.SetCellType(x, z, CellType.Grass);
        gridData.SetDirection(x, z, 0);
        gridData.SetBitmask(x, z, 0);
    }

    // ─── Persistence ─────────────────────────────────────────────

    public void SaveGrid()
    {
        if (gridData == null) return;
        string path = System.IO.Path.Combine(Application.dataPath, binaryFilePath);
        gridData.SaveBinary(path);
    }

    public void LoadGrid()
    {
        if (gridData == null) return;
        string path = System.IO.Path.Combine(Application.dataPath, binaryFilePath);
        if (gridData.LoadBinary(path))
            RebuildMesh();
    }

    // ─── Gizmos ──────────────────────────────────────────────────

    void OnDrawGizmos()
    {
        if (gridData == null) return;

        // Draw grid outline
        Gizmos.color = new Color(1, 1, 1, 0.12f);
        float totalX = gridData.gridWidth * cellSize;
        float totalZ = gridData.gridDepth * cellSize;
        Vector3 origin = transform.position;

        for (int x = 0; x <= gridData.gridWidth; x++)
        {
            Vector3 start = origin + new Vector3(x * cellSize, 0.01f, 0);
            Vector3 end   = origin + new Vector3(x * cellSize, 0.01f, totalZ);
            Gizmos.DrawLine(start, end);
        }
        for (int z = 0; z <= gridData.gridDepth; z++)
        {
            Vector3 start = origin + new Vector3(0, 0.01f, z * cellSize);
            Vector3 end   = origin + new Vector3(totalX, 0.01f, z * cellSize);
            Gizmos.DrawLine(start, end);
        }

        // Grid boundary box
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawWireCube(
            origin + new Vector3(totalX * 0.5f, 0, totalZ * 0.5f),
            new Vector3(totalX, 0.02f, totalZ));

        // Highlight hovered cell(s)
        if (hoveredX < 0 || hoveredZ < 0) return;

        Color brushColor;
        switch (currentBrush)
        {
            case TerrainBrush.Raise:  brushColor = new Color(0, 1, 0, 0.45f);        break;
            case TerrainBrush.Dig:    brushColor = new Color(1, 0.5f, 0, 0.45f);      break;
            case TerrainBrush.Path:   brushColor = new Color(0.72f, 0.53f, 0.3f, 0.45f); break;
            case TerrainBrush.Ramp:   brushColor = new Color(0.3f, 0.6f, 1, 0.45f);   break;
            case TerrainBrush.Eraser: brushColor = new Color(1, 0, 0, 0.35f);         break;
            default: brushColor = Color.white; break;
        }

        Gizmos.color = brushColor;
        int radius = brushSize - 1;
        for (int dz = -radius; dz <= radius; dz++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                int px = hoveredX + dx;
                int pz = hoveredZ + dz;
                if (!gridData.InBounds(px, pz)) continue;

                float py = gridData.GetHeight(px, pz) * heightStep;
                Vector3 center = origin + new Vector3(
                    (px + 0.5f) * cellSize,
                    py + 0.5f * heightStep,
                    (pz + 0.5f) * cellSize
                );
                Gizmos.DrawWireCube(center, new Vector3(cellSize, heightStep, cellSize));
                // Filled translucent cube
                Gizmos.color = new Color(brushColor.r, brushColor.g, brushColor.b, 0.15f);
                Gizmos.DrawCube(center, new Vector3(cellSize * 0.9f, heightStep * 0.3f, cellSize * 0.9f));
                Gizmos.color = brushColor;
            }
        }
    }
}

// ═══════════════════════════════════════════════════════════════════
// Custom Editor (Inspector + SceneView interaction)
// ═══════════════════════════════════════════════════════════════════
#if UNITY_EDITOR
[CustomEditor(typeof(TerrainEditor))]
public class TerrainEditorInspector : Editor
{
    static readonly string[] BRUSH_NAMES = {
        "\u25b2 Levantar", "\u26cf Excavar", "\u2550 Camino", "\u2571 Rampa", "\u2718 Borrar"
    };
    static readonly string[] BRUSH_TOOLTIPS = {
        "Sube la altura del terreno (+1). Genera paredes laterales.",
        "Baja la altura (-1). Valores negativos crean lecho de lago.",
        "Alterna la superficie entre hierba y camino. Auto-conecta con vecinos.",
        "Crea rampa entre dos niveles de altura adyacentes.",
        "Resetea la celda a hierba con altura 0."
    };

    bool isDragging = false;
    int lastPaintedX = -1, lastPaintedZ = -1;

    public override void OnInspectorGUI()
    {
        TerrainEditor te = (TerrainEditor)target;

        // ─── Header ───
        EditorGUILayout.Space(5);
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };
        EditorGUILayout.LabelField("Grid Terrain Editor", headerStyle);
        EditorGUILayout.Space(3);

        // ─── Grid Data ───
        te.gridData = (GridData)EditorGUILayout.ObjectField("Grid Data", te.gridData, typeof(GridData), false);

        if (te.gridData == null)
        {
            EditorGUILayout.HelpBox(
                "Asigna un GridData ScriptableObject. Crea uno con:\nAssets > Create > Terrain > Grid Data",
                MessageType.Info);
            return;
        }

        // ─── Brush Toolbar ───
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Pincel", EditorStyles.boldLabel);

        int currentIdx = (int)te.currentBrush;
        int newIdx = GUILayout.Toolbar(currentIdx, BRUSH_NAMES, GUILayout.Height(30));
        if (newIdx != currentIdx)
        {
            Undo.RecordObject(te, "Change Brush");
            te.currentBrush = (TerrainBrush)newIdx;
        }

        EditorGUILayout.HelpBox(BRUSH_TOOLTIPS[newIdx], MessageType.None);

        // ─── Brush Size ───
        int newSize = EditorGUILayout.IntSlider("Tamano Pincel", te.brushSize, 1, 5);
        if (newSize != te.brushSize)
        {
            Undo.RecordObject(te, "Change Brush Size");
            te.brushSize = newSize;
        }

        // ─── Mesh Settings ───
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Configuracion de Malla", EditorStyles.boldLabel);

        float newCellSize    = EditorGUILayout.FloatField("Tamano Celda", te.cellSize);
        float newHeightStep  = EditorGUILayout.FloatField("Paso de Altura", te.heightStep);
        if (!Mathf.Approximately(newCellSize, te.cellSize) ||
            !Mathf.Approximately(newHeightStep, te.heightStep))
        {
            Undo.RecordObject(te, "Change Mesh Settings");
            te.cellSize    = Mathf.Max(0.1f, newCellSize);
            te.heightStep  = Mathf.Max(0.1f, newHeightStep);
            te.RebuildMesh();
        }

        // ─── Grid Info ───
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField(
            $"Rejilla: {te.gridData.gridWidth} x {te.gridData.gridDepth}",
            EditorStyles.miniLabel);

        if (te.hoveredX >= 0)
        {
            CellData hovered = te.gridData.GetCell(te.hoveredX, te.hoveredZ);
            EditorGUILayout.LabelField(
                $"Celda ({te.hoveredX}, {te.hoveredZ})  |  Altura: {hovered.height}  |  Tipo: {hovered.type}  |  Bitmask: 0x{hovered.bitmask:X2}",
                EditorStyles.miniLabel);
        }

        // ─── Action Buttons ───
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Acciones", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reconstruir Malla", GUILayout.Height(26)))
            te.RebuildMesh();
        if (GUILayout.Button("Limpiar Rejilla", GUILayout.Height(26)))
        {
            if (EditorUtility.DisplayDialog(
                "Limpiar Rejilla",
                "Se resetearan todas las celdas. Continuar?",
                "Si", "Cancelar"))
            {
                Undo.RecordObject(te.gridData, "Clear Grid");
                te.gridData.ClearGrid();
                te.RebuildMesh();
                EditorUtility.SetDirty(te.gridData);
            }
        }
        EditorGUILayout.EndHorizontal();

        // ─── Save / Load ───
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Persistencia Binaria", EditorStyles.boldLabel);
        te.binaryFilePath = EditorGUILayout.TextField("Nombre Archivo", te.binaryFilePath);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Guardar (.bin)", GUILayout.Height(26)))
            te.SaveGrid();
        if (GUILayout.Button("Cargar (.bin)", GUILayout.Height(26)))
        {
            te.LoadGrid();
            EditorUtility.SetDirty(te.gridData);
        }
        EditorGUILayout.EndHorizontal();

        // Force SceneView repaint
        SceneView.RepaintAll();
    }

    // ─── Scene View Interaction ──────────────────────────────────

    void OnSceneGUI()
    {
        TerrainEditor te = (TerrainEditor)target;
        if (te.gridData == null) return;

        Event e = Event.current;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        // ── Raycast using the MeshCollider for accurate hit detection ──
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        int bestX = -1, bestZ = -1;

        RaycastHit hit;
        MeshCollider col = te.GetComponent<MeshCollider>();

        if (col != null && col.Raycast(ray, out hit, 500f))
        {
            // Convert world hit to grid coords
            Vector3 local = hit.point - te.transform.position;
            bestX = Mathf.FloorToInt(local.x / te.cellSize);
            bestZ = Mathf.FloorToInt(local.z / te.cellSize);

            if (!te.gridData.InBounds(bestX, bestZ))
            {
                bestX = -1;
                bestZ = -1;
            }
        }
        else
        {
            // Fallback: plane raycast for empty/new grids
            Plane plane = new Plane(Vector3.up, te.transform.position);
            float enter;
            if (plane.Raycast(ray, out enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter) - te.transform.position;
                bestX = Mathf.FloorToInt(hitPoint.x / te.cellSize);
                bestZ = Mathf.FloorToInt(hitPoint.z / te.cellSize);

                if (!te.gridData.InBounds(bestX, bestZ))
                {
                    bestX = -1;
                    bestZ = -1;
                }
            }
        }

        te.hoveredX = bestX;
        te.hoveredZ = bestZ;

        // ── Handle mouse events ──
        if (bestX >= 0 && bestZ >= 0)
        {
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                isDragging = true;
                lastPaintedX = -1;
                lastPaintedZ = -1;
                te.ApplyBrush(bestX, bestZ);
                lastPaintedX = bestX;
                lastPaintedZ = bestZ;
                GUIUtility.hotControl = controlID;
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && e.button == 0 && isDragging)
            {
                // Avoid re-painting same cell on drag
                if (bestX != lastPaintedX || bestZ != lastPaintedZ)
                {
                    te.ApplyBrush(bestX, bestZ);
                    lastPaintedX = bestX;
                    lastPaintedZ = bestZ;
                }
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                isDragging = false;
                lastPaintedX = -1;
                lastPaintedZ = -1;
                GUIUtility.hotControl = 0;
                e.Use();
            }
        }

        // Prevent default selection behavior
        if (e.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(controlID);
        }

        // Force gizmo repaint
        HandleUtility.Repaint();
    }
}
#endif
