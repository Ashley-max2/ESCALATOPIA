using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Escalatopia.UI;
using System.IO;

public class TutorialBuilderWindow : EditorWindow
{
    private string sceneName = "Tutorial_Generado";

    [MenuItem("Tools/Escalatopia/Tutorial Builder")]
    public static void ShowWindow()
    {
        GetWindow<TutorialBuilderWindow>("Tutorial Builder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generador de Tutorial", EditorStyles.boldLabel);

        sceneName = EditorGUILayout.TextField("Nombre Escena", sceneName);

        if (GUILayout.Button("1. Crear Escena Básica"))
        {
            CreateBasicScene();
        }

        GUILayout.Space(10);
        
        if (GUILayout.Button("2. Configurar Managers & UI"))
        {
            SetupManagers();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("3. Generar Montaña Tutorial"))
        {
            GenerateMountain();
        }
        
        GUILayout.Space(10);

        if (GUILayout.Button("4. Spawneado Player"))
        {
            SpawnPlayer();
        }
    }

    private Material GetOrCreateMaterial(string name, Color color)
    {
        string path = $"Assets/Materials/Generated/{name}.mat";
        
        // Ensure directory exists
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();
        }

        // Try load existing
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.color = color;
            EditorUtility.SetDirty(mat);
        }

        return mat;
    }

    private void CreateBasicScene()
    {
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // Setup Lights
        GameObject lightGO = new GameObject("Directional Light");
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

        // Setup Ground
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Suelo_Base";
        floor.transform.localScale = new Vector3(50, 1, 50);
        
        Renderer rend = floor.GetComponent<MeshRenderer>();
        rend.sharedMaterial = GetOrCreateMaterial("Tutorial_Floor", new Color(0.2f, 0.2f, 0.2f));
        
        // Assuming "Default" layer
        floor.layer = LayerMask.NameToLayer("Default");

        EditorSceneManager.SaveScene(newScene, "Assets/Scenes/" + sceneName + ".unity");
        Debug.Log("Escena creada.");
    }

    private void SetupManagers()
    {
        // Add GameManager
        if (FindObjectOfType<GameManager>() == null)
        {
            new GameObject("GameManager").AddComponent<GameManager>();
        }

        // Run UI Tools setup
        UITools.SetupUISystem();
        
        // Update DialogueView
        Transform uiRoot = FindObjectOfType<UIManager>().transform;
        Transform dialogue = uiRoot.Find("Dialogue_View");
        if (dialogue != null)
        {
            BasicView old = dialogue.GetComponent<BasicView>();
            if (old != null) DestroyImmediate(old);
            
            if (dialogue.GetComponent<DialogueView>() == null)
            {
                dialogue.gameObject.AddComponent<DialogueView>();
            }
        }
    }

    private void GenerateMountain()
    {
        GameObject mountainRoot = new GameObject("Tutorial_Mountain");
        
        // Start Platform
        CreatePlatform(mountainRoot, new Vector3(0, 0, 0), new Vector3(10, 1, 10));

        // Step 1: Jump
        CreatePlatform(mountainRoot, new Vector3(0, 2, 8), new Vector3(5, 1, 5));
        CreateTrigger(new Vector3(0, 1, 4), "Usa ESPACIO para saltar!", 4);

        // Step 2: High Wall (Climb)
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Pared_Escalada";
        wall.transform.parent = mountainRoot.transform;
        wall.transform.position = new Vector3(0, 5, 15);
        wall.transform.localScale = new Vector3(8, 10, 2);
        
        // Material for wall
        wall.GetComponent<Renderer>().sharedMaterial = GetOrCreateMaterial("Tutorial_Wall", new Color(0.2f, 0.2f, 0.8f));
        
        wall.tag = "escalable";
        wall.layer = LayerMask.NameToLayer("Default");
        
        // Top of wall
        CreatePlatform(mountainRoot, new Vector3(0, 10, 16), new Vector3(8, 1, 4));
        
        CreateTrigger(new Vector3(0, 3, 12), "Mantén 'E' para escalar paredes azules!", 5);

        // Step 3: Hook Gap
        CreatePlatform(mountainRoot, new Vector3(0, 15, 30), new Vector3(8, 1, 8)); // Far platform
        
        // Create Hook Point
        GameObject hookPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hookPoint.name = "HookPoint";
        hookPoint.transform.parent = mountainRoot.transform;
        hookPoint.transform.position = new Vector3(0, 18, 23);
        hookPoint.transform.localScale = Vector3.one * 0.5f;
        
        hookPoint.GetComponent<Renderer>().sharedMaterial = GetOrCreateMaterial("Tutorial_HookPoint", Color.cyan);

        hookPoint.tag = "HookPoint"; 
        hookPoint.AddComponent<BasicHookPoint>();
        
        CreateTrigger(new Vector3(0, 11, 18), "Usa Click Derecho para usar el Gancho!", 5);

        // Step 4: Bonfire
        GameObject bonfire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bonfire.name = "Hoguera_Final";
        bonfire.transform.parent = mountainRoot.transform;
        bonfire.transform.position = new Vector3(0, 15.5f, 30);
        bonfire.transform.localScale = new Vector3(1, 0.5f, 1);
        
        bonfire.GetComponent<Renderer>().sharedMaterial = GetOrCreateMaterial("Tutorial_Bonfire", Color.red);
        
        bonfire.AddComponent<BonfireCheckpoint>();
        
        Undo.RegisterCreatedObjectUndo(mountainRoot, "Create Mountain");
    }

    private void CreatePlatform(GameObject root, Vector3 pos, Vector3 scale)
    {
        GameObject plat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plat.transform.parent = root.transform;
        plat.transform.position = pos;
        plat.transform.localScale = scale;
        plat.name = "Platform";
    }

    private void CreateTrigger(Vector3 pos, string msg, float size)
    {
        GameObject trig = new GameObject($"Trigger_{msg.Substring(0, 5)}");
        trig.transform.position = pos;
        BoxCollider bc = trig.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = Vector3.one * size;
        
        TutorialTrigger tt = trig.AddComponent<TutorialTrigger>();
        tt.message = msg;
    }

    private void SpawnPlayer()
    {
        // Try to find existing prefab or create one
        GameObject playerGO = new GameObject("Player");
        playerGO.tag = "Player";
        playerGO.transform.position = new Vector3(0, 2, 0);

        // Add correct components
        CapsuleCollider col = playerGO.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 1, 0);
        col.height = 2f;
        
        Rigidbody rb = playerGO.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        playerGO.AddComponent<PlayerInputHandler>();
        playerGO.AddComponent<ResistenceController>();
        
        // Add State Machine
        PlayerStateMachine psm = playerGO.AddComponent<PlayerStateMachine>();
        psm.groundCheck = playerGO.transform; 
        
        // Fix ground check
        GameObject gc = new GameObject("GroundCheck");
        gc.transform.parent = playerGO.transform;
        gc.transform.localPosition = new Vector3(0, 0.1f, 0);
        psm.groundCheck = gc.transform;
        
        // Add Camera
        GameObject cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        cam.AddComponent<Camera>();
        cam.AddComponent<AudioListener>();
        cam.transform.parent = playerGO.transform;
        cam.transform.localPosition = new Vector3(0, 1.7f, 0);
        
        // Hook System
        GameObject hookSys = new GameObject("HookSystem");
        hookSys.transform.parent = playerGO.transform;
        hookSys.transform.localPosition = new Vector3(0, 1.4f, 0.5f);
        
        // Add Hook components 
        hookSys.AddComponent<HookSystem>();
        hookSys.AddComponent<HookTargetFinder>();
        hookSys.AddComponent<HookMovementController>();
        hookSys.AddComponent<HookVisualController>();
        hookSys.AddComponent<HookInputController>();

        Undo.RegisterCreatedObjectUndo(playerGO, "Spawn Player");
    }
}
