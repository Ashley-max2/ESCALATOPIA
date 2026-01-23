#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Herramienta de editor para configurar automáticamente el entorno de juego.
/// Crea Player, Cámara, y entorno de prueba con un solo click.
/// </summary>
public class PlayerSetupWizard : EditorWindow
{
    private bool createPlayer = true;
    private bool configureCamera = true;
    private bool createTestEnvironment = true;
    private bool createUI = true;
    private bool createGameManager = true;
    
    [MenuItem("Tools/Escalatopia/Player Setup Wizard")]
    public static void ShowWindow()
    {
        GetWindow<PlayerSetupWizard>("Player Setup Wizard");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("=== ESCALATOPIA - PLAYER SETUP ===", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("Selecciona qué quieres crear:", EditorStyles.label);
        GUILayout.Space(5);
        
        createPlayer = EditorGUILayout.Toggle("Crear Player completo", createPlayer);
        configureCamera = EditorGUILayout.Toggle("Configurar Cámara", configureCamera);
        createTestEnvironment = EditorGUILayout.Toggle("Crear Entorno de Prueba", createTestEnvironment);
        createUI = EditorGUILayout.Toggle("Crear UI de Estamina", createUI);
        createGameManager = EditorGUILayout.Toggle("Crear Game Manager", createGameManager);
        
        GUILayout.Space(20);
        
        EditorGUILayout.HelpBox(
            "IMPORTANTE: Antes de ejecutar, asegúrate de tener configurados los Layers:\n" +
            "- Player (Layer 8)\n" +
            "- Ground (Layer 9)\n" +
            "- Climbable (Layer 10)\n" +
            "- HookPoint (Layer 11)\n\n" +
            "Y los Tags: Player, HookPoint",
            MessageType.Warning);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("=== CREAR TODO ===", GUILayout.Height(40)))
        {
            SetupEverything();
        }
        
        GUILayout.Space(10);
        
        GUILayout.Label("O crear individualmente:", EditorStyles.miniLabel);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Solo Player")) CreatePlayerOnly();
        if (GUILayout.Button("Solo Cámara")) ConfigureCameraOnly();
        if (GUILayout.Button("Solo Entorno")) CreateEnvironmentOnly();
        GUILayout.EndHorizontal();
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Configurar Layers y Tags"))
        {
            SetupLayersAndTags();
        }
    }
    
    private void SetupEverything()
    {
        SetupLayersAndTags();
        
        if (createPlayer) CreatePlayerOnly();
        if (configureCamera) ConfigureCameraOnly();
        if (createTestEnvironment) CreateEnvironmentOnly();
        if (createUI) CreateStaminaUI();
        if (createGameManager) CreateGameManagerObject();
        
        Debug.Log("✅ Setup completo! Dale a PLAY para probar.");
    }
    
    private void SetupLayersAndTags()
    {
        // Tags
        AddTag("Player");
        AddTag("HookPoint");
        AddTag("Climbable");
        
        // Note: Layers need to be set manually in Project Settings
        Debug.Log("Tags creados. Recuerda configurar los Layers manualmente en Project Settings.");
    }
    
    private void AddTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        
        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
            {
                found = true;
                break;
            }
        }
        
        if (!found)
        {
            tagsProp.arraySize++;
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedProperties();
            Debug.Log($"Tag '{tag}' creado.");
        }
    }
    
    private void CreatePlayerOnly()
    {
        // Check if player already exists
        if (Object.FindObjectOfType<PlayerStateMachine>() != null)
        {
            Debug.LogWarning("Ya existe un Player en la escena.");
            return;
        }
        
        // Create player capsule
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0, 1, 0);
        player.tag = "Player";
        
        // Try to set layer
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer >= 0) player.layer = playerLayer;
        
        // Remove default collider (we'll use the one from RequireComponent)
        var existingCollider = player.GetComponent<CapsuleCollider>();
        
        // Add components
        player.AddComponent<PlayerStateMachine>();
        // PlayerInputHandler is added via RequireComponent
        player.AddComponent<StaminaSystem>();
        
        // Create ground check
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(player.transform);
        groundCheck.transform.localPosition = new Vector3(0, -1f, 0);
        
        // Create hook origin
        GameObject hookOrigin = new GameObject("HookOrigin");
        hookOrigin.transform.SetParent(player.transform);
        hookOrigin.transform.localPosition = new Vector3(0, 1.2f, 0.5f);
        
        // Create camera target
        GameObject cameraTarget = new GameObject("CameraTarget");
        cameraTarget.transform.SetParent(player.transform);
        cameraTarget.transform.localPosition = new Vector3(0, 1.5f, 0);
        
        // Add grappling hook
        var hook = player.AddComponent<GrapplingHook>();
        
        // Configure PlayerStateMachine
        var psm = player.GetComponent<PlayerStateMachine>();
        SerializedObject so = new SerializedObject(psm);
        
        so.FindProperty("groundCheck").objectReferenceValue = groundCheck.transform;
        so.FindProperty("groundCheckRadius").floatValue = 0.3f;
        
        // Set masks (using default layer if custom doesn't exist)
        int groundLayer = LayerMask.NameToLayer("Ground");
        int climbLayer = LayerMask.NameToLayer("Climbable");
        
        if (groundLayer >= 0)
            so.FindProperty("groundMask").intValue = 1 << groundLayer;
        else
            so.FindProperty("groundMask").intValue = 1; // Default layer
            
        if (climbLayer >= 0)
            so.FindProperty("climbableMask").intValue = 1 << climbLayer;
        
        so.ApplyModifiedProperties();
        
        // Configure hook
        SerializedObject hookSo = new SerializedObject(hook);
        hookSo.FindProperty("hookOrigin").objectReferenceValue = hookOrigin.transform;
        
        int hookLayer = LayerMask.NameToLayer("HookPoint");
        if (hookLayer >= 0)
            hookSo.FindProperty("hookableMask").intValue = 1 << hookLayer;
            
        hookSo.ApplyModifiedProperties();
        
        // Create a simple material
        var renderer = player.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material playerMat = new Material(Shader.Find("Standard"));
            playerMat.color = new Color(0.2f, 0.6f, 1f);
            renderer.material = playerMat;
        }
        
        Selection.activeGameObject = player;
        Debug.Log("✅ Player creado correctamente.");
    }
    
    private void ConfigureCameraOnly()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            mainCam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }
        
        // Add orbit camera
        OrbitCamera orbitCam = mainCam.GetComponent<OrbitCamera>();
        if (orbitCam == null)
        {
            orbitCam = mainCam.gameObject.AddComponent<OrbitCamera>();
        }
        
        // Position camera
        mainCam.transform.position = new Vector3(0, 3, -5);
        mainCam.transform.LookAt(Vector3.up);
        
        // Find player and assign
        var player = Object.FindObjectOfType<PlayerStateMachine>();
        if (player != null)
        {
            SerializedObject so = new SerializedObject(orbitCam);
            so.FindProperty("target").objectReferenceValue = player.transform;
            so.ApplyModifiedProperties();
        }
        
        Debug.Log("✅ Cámara configurada.");
    }
    
    private void CreateEnvironmentOnly()
    {
        int groundLayer = LayerMask.NameToLayer("Ground");
        int climbLayer = LayerMask.NameToLayer("Climbable");
        int hookLayer = LayerMask.NameToLayer("HookPoint");
        
        // Create parent
        GameObject environment = new GameObject("Environment");
        
        // Floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.SetParent(environment.transform);
        floor.transform.localScale = new Vector3(5, 1, 5);
        if (groundLayer >= 0) floor.layer = groundLayer;
        
        var floorMat = new Material(Shader.Find("Standard"));
        floorMat.color = new Color(0.3f, 0.3f, 0.3f);
        floor.GetComponent<Renderer>().material = floorMat;
        
        // Climbable wall
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Wall_Climbable";
        wall.transform.SetParent(environment.transform);
        wall.transform.position = new Vector3(0, 5, 8);
        wall.transform.localScale = new Vector3(6, 10, 0.5f);
        if (climbLayer >= 0) wall.layer = climbLayer;
        
        var wallMat = new Material(Shader.Find("Standard"));
        wallMat.color = new Color(0.2f, 0.4f, 0.8f);
        wall.GetComponent<Renderer>().material = wallMat;
        
        // Hook points
        CreateHookPoint(environment.transform, new Vector3(-8, 6, 0), hookLayer);
        CreateHookPoint(environment.transform, new Vector3(8, 8, 0), hookLayer);
        CreateHookPoint(environment.transform, new Vector3(0, 12, 4), hookLayer);
        
        // Elevated platforms
        CreatePlatform(environment.transform, new Vector3(-8, 4, 0), groundLayer);
        CreatePlatform(environment.transform, new Vector3(8, 6, 0), groundLayer);
        CreatePlatform(environment.transform, new Vector3(0, 10, -4), groundLayer);
        
        // Death pit (high fall)
        GameObject pit = GameObject.CreatePrimitive(PrimitiveType.Plane);
        pit.name = "DeathPit_Visual";
        pit.transform.SetParent(environment.transform);
        pit.transform.position = new Vector3(15, -20, 0);
        pit.transform.localScale = new Vector3(2, 1, 2);
        
        var pitMat = new Material(Shader.Find("Standard"));
        pitMat.color = Color.red;
        pit.GetComponent<Renderer>().material = pitMat;
        
        // Elevated platform for testing fall
        GameObject highPlatform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        highPlatform.name = "HighPlatform_TestFall";
        highPlatform.transform.SetParent(environment.transform);
        highPlatform.transform.position = new Vector3(15, 18, 0);
        highPlatform.transform.localScale = new Vector3(4, 0.5f, 4);
        if (groundLayer >= 0) highPlatform.layer = groundLayer;
        
        var highMat = new Material(Shader.Find("Standard"));
        highMat.color = Color.yellow;
        highPlatform.GetComponent<Renderer>().material = highMat;
        
        // Directional light
        GameObject light = new GameObject("Directional Light");
        light.transform.SetParent(environment.transform);
        light.transform.rotation = Quaternion.Euler(50, -30, 0);
        var lightComp = light.AddComponent<Light>();
        lightComp.type = LightType.Directional;
        lightComp.intensity = 1f;
        lightComp.shadows = LightShadows.Soft;
        
        Debug.Log("✅ Entorno de prueba creado.");
    }
    
    private void CreateHookPoint(Transform parent, Vector3 position, int layer)
    {
        GameObject hookPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hookPoint.name = "HookPoint";
        hookPoint.transform.SetParent(parent);
        hookPoint.transform.position = position;
        hookPoint.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        hookPoint.tag = "HookPoint";
        if (layer >= 0) hookPoint.layer = layer;
        
        hookPoint.AddComponent<HookPoint>();
        
        var mat = new Material(Shader.Find("Standard"));
        mat.color = Color.cyan;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.cyan * 0.3f);
        hookPoint.GetComponent<Renderer>().material = mat;
    }
    
    private void CreatePlatform(Transform parent, Vector3 position, int layer)
    {
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "Platform";
        platform.transform.SetParent(parent);
        platform.transform.position = position;
        platform.transform.localScale = new Vector3(3, 0.5f, 3);
        if (layer >= 0) platform.layer = layer;
        
        var mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.5f, 0.4f, 0.3f);
        platform.GetComponent<Renderer>().material = mat;
    }
    
    private void CreateStaminaUI()
    {
        // Check if canvas exists
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // Create stamina bar container
        GameObject staminaBar = new GameObject("StaminaBar");
        staminaBar.transform.SetParent(canvas.transform, false);
        
        var rect = staminaBar.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(20, -20);
        rect.sizeDelta = new Vector2(200, 20);
        
        staminaBar.AddComponent<CanvasGroup>();
        
        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(staminaBar.transform, false);
        var bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        var bgImage = bg.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0, 0, 0, 0.5f);
        
        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(staminaBar.transform, false);
        var fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = new Vector2(-4, -4);
        
        var fillImage = fill.AddComponent<UnityEngine.UI.Image>();
        fillImage.color = new Color(0.2f, 0.8f, 0.3f);
        fillImage.type = UnityEngine.UI.Image.Type.Filled;
        fillImage.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        
        // Add stamina UI component
        var staminaUI = staminaBar.AddComponent<StaminaUI>();
        SerializedObject so = new SerializedObject(staminaUI);
        so.FindProperty("staminaFill").objectReferenceValue = fillImage;
        so.FindProperty("staminaBackground").objectReferenceValue = bgImage;
        so.FindProperty("canvasGroup").objectReferenceValue = staminaBar.GetComponent<CanvasGroup>();
        so.ApplyModifiedProperties();
        
        Debug.Log("✅ UI de estamina creada.");
    }
    
    private void CreateGameManagerObject()
    {
        if (Object.FindObjectOfType<GameManager>() != null)
        {
            Debug.Log("GameManager ya existe en la escena.");
            return;
        }
        
        GameObject gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();
        
        Debug.Log("✅ GameManager creado.");
    }
}
#endif
