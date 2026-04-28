using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    [SerializeField] private Animator climberAnimator;   // Animaci�n de escalador (opcional pero recomendado)

    [Header("Tiempos")]
    [SerializeField] private float minimumLoadingTime = 1.2f;   // Para que no sea demasiado r�pido
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

    private const string LoadingSceneName = "LoadingScreen";

    /// <summary>
    /// Llama a esto para cambiar de nivel con pantalla de carga
    /// </summary>
    public static void LoadLevel(string sceneName)
    {
        if (Instance != null)
        {
            Instance.StartCoroutine(Instance.LoadLevelCoroutine(sceneName));
            return;
        }

        GameObject loaderObject = new GameObject("LoadingManagerSceneLoader");
        DontDestroyOnLoad(loaderObject);
        var loader = loaderObject.AddComponent<LoadingSceneLoader>();
        loader.StartCoroutine(loader.LoadLoadingSceneAndStart(sceneName));
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

        // Peque�a pausa art�stica antes de empezar a cargar
        yield return new WaitForSeconds(0.3f);

        string previousScene = SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(currentLevelScene) &&
            !string.Equals(previousScene, LoadingSceneName, StringComparison.OrdinalIgnoreCase))
        {
            currentLevelScene = previousScene;
        }

        // Cargar nueva escena de forma asíncrona con activación diferida
        AsyncOperation operation = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Single);
        operation.allowSceneActivation = false;

        float timer = 0f;
        float progress = 0f;

        while (!operation.isDone)
        {
            progress = Mathf.Clamp01(operation.progress / 0.9f);
            timer += Time.deltaTime;

            if (progressBar) progressBar.value = progress;

            // Textos tem�ticos seg�n progreso
            if (loadingText)
            {
                if (progress < 0.4f) loadingText.text = "Buscando agarre...";
                else if (progress < 0.75f) loadingText.text = "Escalando la pared...";
                else loadingText.text = "��ltimo tramo!";
            }

            // Esperar tiempo m�nimo + que Unity termine de cargar
            if (progress >= 0.9f && timer >= minimumLoadingTime)
            {
                if (loadingText) loadingText.text = "�Preparado para escalar!";
                yield return new WaitForSeconds(0.6f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        // Asegurar la nueva escena activa
        Scene newScene = SceneManager.GetSceneByName(newSceneName);
        if (newScene.IsValid() && newScene.isLoaded)
        {
            SceneManager.SetActiveScene(newScene);
        }

        currentLevelScene = newSceneName;

        // Fade out de la pantalla de carga
        canvasGroup.DOFade(0f, fadeDuration).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });

        if (SceneManager.GetSceneByName(LoadingSceneName).isLoaded)
        {
            SceneManager.UnloadSceneAsync(LoadingSceneName);
        }

        Debug.Log($"[LoadingManager] Nivel cargado: {newSceneName}");
    }

    // Método cómodo para volver al menú
    public static void LoadMainMenu()
    {
        LoadLevel("MainMenu");
    }

    private class LoadingSceneLoader : MonoBehaviour
    {
        public IEnumerator LoadLoadingSceneAndStart(string targetScene)
        {
            AsyncOperation loadingSceneOp = SceneManager.LoadSceneAsync(LoadingSceneName, LoadSceneMode.Additive);
            if (loadingSceneOp == null)
            {
                Debug.LogError($"No se pudo cargar la escena de carga '{LoadingSceneName}'. Cargando escena objetivo directamente.");
                SceneManager.LoadScene(targetScene);
                Destroy(gameObject);
                yield break;
            }

            while (!loadingSceneOp.isDone)
            {
                yield return null;
            }

            yield return null;

            if (LoadingManager.Instance == null)
            {
                Debug.LogError("LoadingManager no encontrado después de cargar la escena LoadingScreen.");
                SceneManager.LoadScene(targetScene);
                Destroy(gameObject);
                yield break;
            }

            LoadingManager.Instance.StartCoroutine(LoadingManager.Instance.LoadLevelCoroutine(targetScene));
            Destroy(gameObject);
        }
    }
}
