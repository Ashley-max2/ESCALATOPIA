using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Escalatopia.UI;

public class UITools : EditorWindow
{
    [MenuItem("Tools/Escalatopia/Setup UI System")]
    public static void SetupUISystem()
    {
        // 1. Check for EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(esGO, "Create EventSystem");
        }

        // 2. Check for UIManager
        UIManager existingManager = FindObjectOfType<UIManager>();
        GameObject uiRoot;

        if (existingManager != null)
        {
            uiRoot = existingManager.gameObject;
        }
        else
        {
            uiRoot = new GameObject("UI_Manager");
            uiRoot.AddComponent<Canvas>();
            uiRoot.AddComponent<CanvasScaler>();
            uiRoot.AddComponent<GraphicRaycaster>();
            uiRoot.AddComponent<UIManager>();
            
            // Setup Canvas
            Canvas canvas = uiRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = uiRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            Undo.RegisterCreatedObjectUndo(uiRoot, "Create UI Manager");
        }

        // 3. Create Standard Views
        CreateView(uiRoot, "HUD_View", false);
        CreateView(uiRoot, "Pause_View", true);
        CreateView(uiRoot, "Shop_View", true);
        CreateView(uiRoot, "Dialogue_View", true);

        Debug.Log("UI System Setup Complete!");
    }

    private static void CreateView(GameObject root, string name, bool startHidden)
    {
        Transform existing = root.transform.Find(name);
        if (existing != null) return;

        GameObject viewGO = new GameObject(name);
        viewGO.transform.SetParent(root.transform, false);
        
        // Add full screen rect transform
        RectTransform rt = viewGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Add BasicView
        BasicView view = viewGO.AddComponent<BasicView>();
        view.viewId = name;
        view.startHidden = startHidden;
        
        Undo.RegisterCreatedObjectUndo(viewGO, "Create View " + name);
    }
}
