using UnityEngine;
using UnityEditor;

public class WaterMeshGeneratorWindow : EditorWindow
{
    Terrain terrain;
    Transform waterPlane;

    int resolution = 256;

    bool useWaterTransform = true;
    float waterHeight = 0f;

    float width = 2f;
    float heightOffset = 0.05f;
    float extrudeDepth = 5f;

    [MenuItem("Tools/Water Mesh Generator")]
    public static void Open()
    {
        GetWindow<WaterMeshGeneratorWindow>("Water Mesh Generator");
    }

    void OnGUI()
    {
        terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true);

        useWaterTransform = EditorGUILayout.Toggle("Use Water Transform", useWaterTransform);

        if (useWaterTransform)
        {
            waterPlane = (Transform)EditorGUILayout.ObjectField("Water Transform", waterPlane, typeof(Transform), true);
        }
        else
        {
            waterHeight = EditorGUILayout.FloatField("Water Height", waterHeight);
        }

        resolution = EditorGUILayout.IntSlider("Resolution", resolution, 32, 1024);
        width = EditorGUILayout.FloatField("Shore Width", width);
        heightOffset = EditorGUILayout.FloatField("Height Offset", heightOffset);
        extrudeDepth = EditorGUILayout.FloatField("Extrude Depth", extrudeDepth);

        if (GUILayout.Button("Generate Water Mesh"))
        {
            GenerateMesh();
        }
    }

    void GenerateMesh()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain no asignado");
            return;
        }

        float finalHeight = useWaterTransform && waterPlane != null
            ? waterPlane.position.y
            : waterHeight;

        WaterMeshBuilder.BuildShoreMesh(
            terrain,
            finalHeight,
            resolution,
            width,
            heightOffset,
            extrudeDepth
        );
    }
}