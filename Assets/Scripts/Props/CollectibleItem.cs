using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using FMODUnity;
using System.Collections;

/// <summary>
/// Componente para objetos que se recogen automáticamente al entrar en el trigger.
/// Opcionalmente puede teleportar al jugador al boss con fade in/out.
/// </summary>
public class CollectibleItem : MonoBehaviour
{
    [Header("Item")]
    public string itemName = "Objeto de la montaña";

    [Header("Teleport (Opcional)")]
    [Tooltip("Si true, teleporta al jugador al lado del boss después de recoger")]
    public bool teleportToBoss = false;

    [Tooltip("Distancia frontal al boss donde aparecerá el jugador")]
    public float distanceFromBoss = 3f;

    [Header("Fade")]
    [Tooltip("Duración del fade out")]
    public float fadeDuration = 0.5f;
    [Tooltip("Duración del fade in")]
    public float unfadeDuration = 0.5f;

    [Header("Audio")]
    [EventRef] public string pickupSound = "event:/SFX/Buttons";

    [Header("Events")]
    public UnityEvent onCollected;

    private bool isCollected = false;
    private static Image sharedFadeImage;

    public bool IsCollected => isCollected;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Recoger automáticamente
        if (!isCollected)
        {
            Collect();
        }
    }

    public void Collect()
    {
        if (isCollected) return;

        isCollected = true;

        // Reproducir sonido de pickup
        if (!string.IsNullOrEmpty(pickupSound))
        {
            RuntimeManager.PlayOneShot(pickupSound, transform.position);
        }

        onCollected?.Invoke();

        // Si teleportToBoss está activado, iniciar la corrutina ANTES de desactivar
        if (teleportToBoss)
        {
            StartCoroutine(TeleportToBossCoroutine());
        }
        else
        {
            // Si no hay teleport, simplemente ocultar el objeto
            gameObject.SetActive(false);
        }
    }

    private IEnumerator TeleportToBossCoroutine()
    {
        // Encontrar al jugador
        PlayerStateMachine player = Object.FindObjectOfType<PlayerStateMachine>();
        if (player == null)
        {
            Debug.LogError("[CollectibleItem] ❌ No se encontró PlayerStateMachine");
            yield break;
        }

        // Encontrar el boss
        Transform bossTransform = FindBossTransform();
        if (bossTransform == null)
        {
            Debug.LogError("[CollectibleItem] ❌ No se encontró el Boss en la escena");
            yield break;
        }

        Debug.Log("[CollectibleItem] ✅ Iniciando teleport con fade");

        // Freeze del player mientras hace fade
        if (player.Rb != null)
            player.Rb.isKinematic = true;

        // Fade out
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        // Teleportar al jugador al lado del boss
        Vector3 teleportPosition = bossTransform.position + bossTransform.forward * distanceFromBoss + Vector3.up * 0.5f;
        player.transform.position = teleportPosition;
        Debug.Log($"[CollectibleItem] 📍 Teleportado a {teleportPosition}");

        // Fade in
        yield return StartCoroutine(Fade(1f, 0f, unfadeDuration));

        // Desfreeze del player
        if (player.Rb != null)
            player.Rb.isKinematic = false;

        // Desactivar el objeto de la linterna después del teleport
        gameObject.SetActive(false);
    }

    private Transform FindBossTransform()
    {
        // Buscar por tag "Boss"
        GameObject bossGO = GameObject.FindWithTag("Boss");
        if (bossGO != null)
            return bossGO.transform;

        // Si no, buscar por nombre
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.name.Contains("Boss"))
                return go.transform;
        }

        return null;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        Image fadeImage = GetOrCreateFadeImage();
        if (fadeImage == null) yield break;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            SetFadeAlpha(fadeImage, currentAlpha);
            yield return null;
        }
        SetFadeAlpha(fadeImage, endAlpha);
    }

    private void SetFadeAlpha(Image fadeImage, float alpha)
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
        }
    }

    private Image GetOrCreateFadeImage()
    {
        // Si ya existe, retornarlo
        if (sharedFadeImage != null)
            return sharedFadeImage;

        // Buscar Canvas existente
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            // Buscar o crear Image para fade
            Transform fadeObj = canvas.transform.Find("FadeScreen");
            if (fadeObj != null)
            {
                Image img = fadeObj.GetComponent<Image>();
                if (img != null)
                {
                    sharedFadeImage = img;
                    return img;
                }
            }

            // Si no existe, crear uno
            GameObject imageObj = new GameObject("FadeScreen");
            imageObj.transform.SetParent(canvas.transform, false);
            Image newFadeImage = imageObj.AddComponent<Image>();
            newFadeImage.color = new Color(0, 0, 0, 0);
            newFadeImage.raycastTarget = false;

            RectTransform rt = newFadeImage.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            sharedFadeImage = newFadeImage;
            return newFadeImage;
        }

        Debug.LogError("[CollectibleItem] ❌ No hay Canvas en la escena!");
        return null;
    }
}
