using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Service for scene management operations.
/// Single Responsibility: Scene loading and transitions.
/// Follows Dependency Inversion - UI depends on this service.
/// </summary>
public class SceneService : MonoBehaviour
{
    private static SceneService instance;
    public static SceneService Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("SceneService");
                instance = go.AddComponent<SceneService>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    /// <summary>
    /// Load a scene by name.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        Debug.Log($"[SceneService] Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Load a scene asynchronously.
    /// </summary>
    public AsyncOperation LoadSceneAsync(string sceneName)
    {
        Debug.Log($"[SceneService] Loading scene async: {sceneName}");
        return SceneManager.LoadSceneAsync(sceneName);
    }

    /// <summary>
    /// Reload the current scene.
    /// </summary>
    public void ReloadCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        LoadScene(currentScene);
    }

    /// <summary>
    /// Get the current scene name.
    /// </summary>
    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// Quit the application.
    /// </summary>
    public void QuitApplication()
    {
        Debug.Log("[SceneService] Quitting application");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
