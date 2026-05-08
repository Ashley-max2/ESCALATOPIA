using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerVFXController : MonoBehaviour
{
    [Header("Particles")]
    public ParticleSystem walkRunParticles;
    public ParticleSystem jumpLandParticles;

    [Header("Camera Effects")]
    public Camera mainCamera;
    public Volume globalVolume;

    private PlayerStateMachine stateMachine;
    private StaminaSystem staminaSystem;

    private float originalFOV;
    private Vignette vignette;
    private LensDistortion lensDistortion;

    private bool wasGrounded;

    private void Start()
    {
        stateMachine = GetComponent<PlayerStateMachine>();
        staminaSystem = GetComponent<StaminaSystem>();

        if (mainCamera != null)
            originalFOV = mainCamera.fieldOfView;

        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out vignette);
            globalVolume.profile.TryGet(out lensDistortion);
        }

        wasGrounded = stateMachine.IsGrounded;
    }

    private void Update()
    {
        HandleParticles();
        HandleStaminaVignette();
        HandleHookCameraEffect();
    }

    private void HandleParticles()
    {
        if (stateMachine == null) return;

        // Walk/Run particles
        if (stateMachine.CurrentState != null && stateMachine.CurrentState.GetType().Name == "PlayerGroundedState")
        {
            float speed = new Vector2(stateMachine.Rb.velocity.x, stateMachine.Rb.velocity.z).magnitude;
            if (speed > 0.1f)
            {
                if (!walkRunParticles.isPlaying) walkRunParticles.Play();
                
                var emission = walkRunParticles.emission;
                emission.rateOverTime = stateMachine.Input.SprintHeld ? 20f : 5f;
            }
            else
            {
                if (walkRunParticles.isPlaying) walkRunParticles.Stop();
            }
        }
        else
        {
            if (walkRunParticles.isPlaying) walkRunParticles.Stop();
        }

        // Jump Land particles
        if (!wasGrounded && stateMachine.IsGrounded)
        {
            if (jumpLandParticles != null)
            {
                jumpLandParticles.Play();
            }
        }
        wasGrounded = stateMachine.IsGrounded;
    }

    private void HandleStaminaVignette()
    {
        if (staminaSystem == null || vignette == null) return;

        ColorAdjustments colorAdjustments = null;
        if (globalVolume != null && globalVolume.profile != null)
        {
            if (!globalVolume.profile.TryGet(out colorAdjustments))
            {
                colorAdjustments = globalVolume.profile.Add<ColorAdjustments>();
            }
        }

        // Map stamina from -5 to max
        float percent = Mathf.InverseLerp(-5f, staminaSystem.MaxStamina, staminaSystem.CurrentStamina);
        
        float targetIntensity = Mathf.Lerp(1f, 0f, percent);
        vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetIntensity, Time.deltaTime * 5f);
        vignette.color.value = Color.black;

        if (colorAdjustments != null)
        {
            colorAdjustments.active = true;
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.postExposure.overrideState = true;
            
            // Degradado a gris
            colorAdjustments.saturation.value = Mathf.Lerp(-100f, 0f, percent);
            
            // Negro al 100% cuando se gasta toda (<= -4.9)
            if (staminaSystem.CurrentStamina <= -4.9f)
                colorAdjustments.postExposure.value = Mathf.Lerp(colorAdjustments.postExposure.value, -10f, Time.deltaTime * 10f);
            else
                colorAdjustments.postExposure.value = Mathf.Lerp(colorAdjustments.postExposure.value, 0f, Time.deltaTime * 5f);
        }
    }

    private void HandleHookCameraEffect()
    {
        if (stateMachine == null || mainCamera == null) return;

        bool isHooking = stateMachine.CurrentState != null && stateMachine.CurrentState.GetType().Name == "PlayerHookState";

        float targetFOV = isHooking ? originalFOV + 20f : originalFOV;
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * 5f);

        if (lensDistortion != null)
        {
            float targetDistortion = isHooking ? -0.5f : 0f;
            lensDistortion.intensity.value = Mathf.Lerp(lensDistortion.intensity.value, targetDistortion, Time.deltaTime * 5f);
        }
    }
}