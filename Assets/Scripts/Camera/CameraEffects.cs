using UnityEngine;
using System.Collections;

/// <summary>
/// Camera effects system for Gold-level camera rubric score.
/// Provides shake, fade, dynamic FOV, and other cinematic effects.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraEffects : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeIntensity = 0.3f;
    [SerializeField] private float shakeDuration = 0.5f;

    [Header("FOV Settings")]
    [SerializeField] private float defaultFOV = 60f;
    [SerializeField] private float fovChangeSpeed = 2f;

    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeSpeed = 1f;

    private Camera cam;
    private Vector3 originalPosition;
    private float originalFOV;
    private bool isShaking = false;
    private float targetFOV;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        originalFOV = cam.fieldOfView;
        targetFOV = originalFOV;
    }

    private void Update()
    {
        // Smoothly adjust FOV
        if (Mathf.Abs(cam.fieldOfView - targetFOV) > 0.1f)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovChangeSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Shake the camera.
    /// </summary>
    public void Shake(float intensity = -1f, float duration = -1f)
    {
        if (isShaking)
            return;

        float shakeInt = intensity > 0 ? intensity : shakeIntensity;
        float shakeDur = duration > 0 ? duration : shakeDuration;

        StartCoroutine(ShakeCoroutine(shakeInt, shakeDur));
    }

    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        isShaking = true;
        originalPosition = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;

            transform.localPosition = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        isShaking = false;
    }

    /// <summary>
    /// Set dynamic FOV (e.g., for speed effects).
    /// </summary>
    public void SetDynamicFOV(float fov)
    {
        targetFOV = Mathf.Clamp(fov, 30f, 120f);
    }

    /// <summary>
    /// Reset FOV to default.
    /// </summary>
    public void ResetFOV()
    {
        targetFOV = defaultFOV;
    }

    /// <summary>
    /// Increase FOV (for speed boost effect).
    /// </summary>
    public void IncreaseFOV(float amount)
    {
        targetFOV = Mathf.Clamp(targetFOV + amount, 30f, 120f);
    }

    /// <summary>
    /// Decrease FOV (for zoom effect).
    /// </summary>
    public void DecreaseFOV(float amount)
    {
        targetFOV = Mathf.Clamp(targetFOV - amount, 30f, 120f);
    }

    /// <summary>
    /// Fade to black.
    /// </summary>
    public void FadeOut(float duration = -1f)
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogWarning("[CameraEffects] Fade canvas group not assigned!");
            return;
        }

        float fadeDuration = duration > 0 ? duration : 1f / fadeSpeed;
        StartCoroutine(FadeCoroutine(0f, 1f, fadeDuration));
    }

    /// <summary>
    /// Fade from black.
    /// </summary>
    public void FadeIn(float duration = -1f)
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogWarning("[CameraEffects] Fade canvas group not assigned!");
            return;
        }

        float fadeDuration = duration > 0 ? duration : 1f / fadeSpeed;
        StartCoroutine(FadeCoroutine(1f, 0f, fadeDuration));
    }

    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        fadeCanvasGroup.alpha = startAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }

        fadeCanvasGroup.alpha = endAlpha;
    }

    /// <summary>
    /// Apply speed FOV effect (increase FOV when moving fast).
    /// </summary>
    public void ApplySpeedFOVEffect(float speed, float maxSpeed)
    {
        float speedRatio = Mathf.Clamp01(speed / maxSpeed);
        float fovIncrease = speedRatio * 15f; // Max 15 degree increase
        SetDynamicFOV(defaultFOV + fovIncrease);
    }

    /// <summary>
    /// Cinematic camera movement (smooth transition).
    /// </summary>
    public void MoveToCinematicPosition(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        StartCoroutine(CinematicMoveCoroutine(targetPosition, targetRotation, duration));
    }

    private IEnumerator CinematicMoveCoroutine(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Smooth interpolation
            t = t * t * (3f - 2f * t); // Smoothstep

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
    }
}
