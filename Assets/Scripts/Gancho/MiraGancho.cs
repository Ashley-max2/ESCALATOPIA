using UnityEngine;
using UnityEngine.UI;

public class HookReticle : MonoBehaviour
{
    [Header("Elementos UI")]
    public Image reticleImage;
    public Color normalColor = Color.white;
    public Color validTargetColor = Color.green;
    public Color aimingColor = Color.yellow;

    [Header("Configuración")]
    public float animationScale = 1.2f;
    public float animationSpeed = 2f;

    private Canvas canvas;
    private Vector3 originalScale;
    private HookSystem hookSystem;

    void Start()
    {
        canvas = GetComponent<Canvas>();
        hookSystem = FindObjectOfType<HookSystem>();

        if (canvas != null)
        {
            canvas.enabled = false;
        }

        if (reticleImage != null)
        {
            reticleImage.color = normalColor;
            originalScale = reticleImage.transform.localScale;
        }
    }

    void Update()
    {
        UpdateReticleVisibility();
        UpdateReticleState();
    }

    void UpdateReticleVisibility()
    {
        CameraManager cameraManager = FindObjectOfType<CameraManager>();
        if (cameraManager != null && canvas != null)
        {
            bool shouldShow = cameraManager.EstaEnPrimeraPersona();
            canvas.enabled = shouldShow;
        }
    }

    void UpdateReticleState()
    {
        if (hookSystem == null || reticleImage == null) return;

        // SOLO parpadea cuando el raycast está sobre un HookPoint válido
        if (hookSystem.TargetFinder != null && hookSystem.TargetFinder.HasValidTarget)
        {
            ShowValidTarget(); // verde + parpadeo
            return;
        }

        // Si quieres mostrar amarillo fijo cuando estás en aiming pero sin target, descomenta:
        // if (hookSystem.IsAiming) { ShowAimingNoBlink(); return; }

        // En cualquier otro caso: blanco sin parpadeo
        HideReticle();
    }

    public void ShowValidTarget()
    {
        reticleImage.color = validTargetColor;
        AnimateReticle();
    }

    // Opción: mostrar amarillo sin parpadeo cuando apuntas pero no hay target
    public void ShowAimingNoBlink()
    {
        reticleImage.color = aimingColor;
        reticleImage.transform.localScale = originalScale;
    }

    public void HideReticle()
    {
        reticleImage.color = normalColor;
        reticleImage.transform.localScale = originalScale;
    }

    private void AnimateReticle()
    {
        float scale = originalScale.x + Mathf.PingPong(Time.time * animationSpeed, animationScale - 1f);
        reticleImage.transform.localScale = new Vector3(scale, scale, scale);
    }
}