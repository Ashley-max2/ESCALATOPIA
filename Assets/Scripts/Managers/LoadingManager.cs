using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance { get; private set; }

    [Header("=== UI de Carga ===")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Text loadingText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Animator climberAnimator;   // Animación de escalador (opcional pero recomendado)

    [Header("Tiempos")]
    [SerializeField] private float minimumLoadingTime = 1.2f;   // Para que no sea demasiado rápido
    [SerializeField] private float fadeDuration = 0.6f;

    private string currentLevelScene = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        gameObject.SetActive(false); // Empieza desactivada
    }

    /// <summary>
    /// Llama a esto para cambiar de nivel con pantalla de carga
    /// </summary>
    public static void LoadLevel(string sceneName)
    {
        if (Instance == null)
        {
            Debug.LogError("LoadingManager no encontrado. Asegúrate de tener la escena LoadingScreen cargada.");
            SceneManager.LoadScene(sceneName);
            return;
        }

        Instance.StartCoroutine(Instance.LoadLevelCoroutine(sceneName));
    }

    private IEnumerator LoadLevelCoroutine(string newSceneName)
    {
        // Mostrar pantalla de carga
        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, fadeDuration);

        if (climberAnimator) climberAnimator.SetTrigger("Climb");

        if (loadingText) loadingText.text = "Asegurando mosquetones...";
        if (progressBar) progressBar.value = 0f;

        // Pequeña pausa artística antes de empezar a cargar
        yield return new WaitForSeconds(0.3f);

        // Descargar escena anterior si existe
        if (!string.IsNullOrEmpty(currentLevelScene))
        {
            yield return SceneManager.UnloadSceneAsync(currentLevelScene);
        }

        // Cargar nueva escena de forma asíncrona
        AsyncOperation operation = SceneManager.LoadSceneAsync(newSceneName);
        operation.allowSceneActivation = false;

        float timer = 0f;
        float progress = 0f;

        while (!operation.isDone)
        {
            progress = Mathf.Clamp01(operation.progress / 0.9f);
            timer += Time.deltaTime;

            if (progressBar) progressBar.value = progress;

            // Textos temáticos según progreso
            if (loadingText)
            {
                if (progress < 0.4f) loadingText.text = "Buscando agarre...";
                else if (progress < 0.75f) loadingText.text = "Escalando la pared...";
                else loadingText.text = "¡Último tramo!";
            }

            // Esperar tiempo mínimo + que Unity termine de cargar
            if (progress >= 0.9f && timer >= minimumLoadingTime)
            {
                if (loadingText) loadingText.text = "¡Preparado para escalar!";
                yield return new WaitForSeconds(0.6f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        // Actualizar escena actual
        currentLevelScene = newSceneName;

        // Fade out de la pantalla de carga
        canvasGroup.DOFade(0f, fadeDuration).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });

        Debug.Log($"[LoadingManager] Nivel cargado: {newSceneName}");
    }

    // Método cómodo para volver al menú
    public static void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // o el nombre que uses
    }
}
}
