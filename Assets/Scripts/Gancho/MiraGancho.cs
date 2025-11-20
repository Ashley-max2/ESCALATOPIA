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
        if (hookSystem == null) return;

        if (hookSystem.IsHooking)
        {
            ShowAiming();
        }
        else if (hookSystem.TargetFinder.HasValidTarget)
        {
            ShowValidTarget();
        }
        else
        {
            HideReticle();
        }
    }

    public void ShowValidTarget()
    {
        if (reticleImage != null)
        {
            reticleImage.color = validTargetColor;
            AnimateReticle();
        }
    }

    public void ShowAiming()
    {
        if (reticleImage != null)
        {
            reticleImage.color = aimingColor;
            AnimateReticle();
        }
    }

    public void HideReticle()
    {
        if (reticleImage != null)
        {
            reticleImage.color = normalColor;
            reticleImage.transform.localScale = originalScale;
        }
    }

    private void AnimateReticle()
    {
        if (reticleImage != null)
        {
            float scale = originalScale.x + Mathf.PingPong(Time.time * animationSpeed, animationScale - 1f);
            reticleImage.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}