using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class HazardTeleporter : MonoBehaviour
{
    [Header("Configuración de Teletransporte")]
    [Tooltip("El checkpoint a donde el jugador será teletransportado.")]
    public Transform checkpoint;
    
    [Tooltip("El tag que debe tener el jugador para activar esto.")]
    public string playerTag = "Player";

    [Header("Tiempos (Segundos)")]
    [Tooltip("Tiempo de espera desde que choca hasta que empieza el fade out.")]
    public float timeBeforeFade = 0.5f;
    
    [Tooltip("Duración del efecto de fundido a negro (Fade Out).")]
    public float fadeDuration = 0.5f;

    [Tooltip("Duración del efecto de vuelta a la normalidad (Fade In) tras el TP.")]
    public float unfadeDuration = 0.5f;

    [Header("UI de Fade (Opcional)")]
    [Tooltip("Imagen negra a pantalla completa para hacer el fundido. Si se deja en blanco, el script la buscará o creará automáticamente.")]
    public Image fadeImage;

    // Static flag para evitar múltiples activaciones simultáneas
    private static bool isHandlingTeleport = false;
    private static Image sharedFadeImage;

    private void Start()
    {
        if (fadeImage == null)
        {
            if (sharedFadeImage == null)
            {
                CreateFadeUI();
            }
            fadeImage = sharedFadeImage;
        }
        else
        {
            sharedFadeImage = fadeImage;
            SetFadeAlpha(0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !isHandlingTeleport)
        {
            StartCoroutine(TeleportRoutine(other));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag) && !isHandlingTeleport)
        {
            StartCoroutine(TeleportRoutine(collision.collider));
        }
    }

    private IEnumerator TeleportRoutine(Collider playerCollider)
    {
        isHandlingTeleport = true;

        if (timeBeforeFade > 0f)
            yield return new WaitForSeconds(timeBeforeFade);

        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        if (checkpoint != null)
        {
            TeleportPlayer(playerCollider, checkpoint.position);
        }
        else
        {
            Debug.LogWarning("¡Falta asignar el Checkpoint en el script HazardTeleporter de " + gameObject.name + "!");
        }

        yield return StartCoroutine(Fade(1f, 0f, unfadeDuration));

        isHandlingTeleport = false;
    }

    private void TeleportPlayer(Collider playerCollider, Vector3 destination)
    {
        // Encontrar los componentes correctos subiendo en la jerarquía (ignora colisionadores secundarios)
        Rigidbody rb = playerCollider.GetComponentInParent<Rigidbody>();
        CharacterController cc = playerCollider.GetComponentInParent<CharacterController>();
        PlayerStateMachine psm = playerCollider.GetComponentInParent<PlayerStateMachine>();

        // Usamos el transform base del physics principal, no simplemente el "root" para no desplazar hijos localmente
        Transform targetTransform = rb != null ? rb.transform : (cc != null ? cc.transform : playerCollider.transform.root);

        if (cc != null) cc.enabled = false;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Mover una sola vez el master transform
        targetTransform.position = destination;
        if (checkpoint != null) 
        {
            targetTransform.rotation = checkpoint.rotation; // Copiar rotación del checkpoint
        }

        Physics.SyncTransforms();

        if (cc != null) cc.enabled = true;

        // Resetear la máquina de estados pidiendo estado Grounded
        // (Esto evita que si el personaje estaba haciendo Mantle o Escalando se quede "congelado" mentalmente allí)
        if (psm != null)
        {
            psm.TransitionToState(psm.States.Grounded());
            psm.LastGroundedPosition = destination;
        }
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        if (fadeImage == null) yield break;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            SetFadeAlpha(currentAlpha);
            yield return null;
        }
        SetFadeAlpha(endAlpha);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
        }
    }

    private void CreateFadeUI()
    {
        GameObject canvasObj = new GameObject("HazardFadeCanvas_Procedural");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; 

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject imageObj = new GameObject("FadeScreen");
        imageObj.transform.SetParent(canvasObj.transform, false);
        Image newFadeImage = imageObj.AddComponent<Image>();
        newFadeImage.color = new Color(0, 0, 0, 0); 
        newFadeImage.raycastTarget = false; 

        RectTransform rt = newFadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        sharedFadeImage = newFadeImage;
        DontDestroyOnLoad(canvasObj);
    }
}
