using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Controlador VFX del jugador.
/// Gestiona 3 sistemas de polvo: caminar, correr y aterrizaje de salto.
/// También controla viñeta de stamina y FOV del gancho.
/// </summary>
public class PlayerVFXController : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    //  REFERENCIAS DE PARTÍCULAS
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Dust Particles ──")]
    [Tooltip("Polvo suave al caminar")]
    public ParticleSystem walkDust;

    [Tooltip("Polvo más denso al correr")]
    public ParticleSystem runDust;

    [Tooltip("Burst de polvo al caer de 3 m o más")]
    public ParticleSystem landDust;

    [Tooltip("Distancia mínima de caída (metros) para disparar el burst de aterrizaje")]
    [Range(1f, 10f)]
    public float minLandBurstHeight = 3f;

    // ─────────────────────────────────────────────────────────────────────────
    //  EFECTOS DE CÁMARA
    // ─────────────────────────────────────────────────────────────────────────
    [Header("── Camera Effects ──")]
    public Camera mainCamera;
    public Volume globalVolume;

    // ─────────────────────────────────────────────────────────────────────────
    //  PRIVADOS
    // ─────────────────────────────────────────────────────────────────────────
    private PlayerStateMachine _sm;
    private StaminaSystem _stamina;

    private float _originalFOV;
    private Vignette _vignette;
    private LensDistortion _lensDistortion;

    // Estado previo para detectar transición aire→suelo
    private bool _wasGrounded;

    // ─────────────────────────────────────────────────────────────────────────
    //  UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _sm      = GetComponent<PlayerStateMachine>();
        _stamina = GetComponent<StaminaSystem>();
    }

    private void Start()
    {
        if (mainCamera != null)
            _originalFOV = mainCamera.fieldOfView;

        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out _vignette);
            globalVolume.profile.TryGet(out _lensDistortion);
        }

        if (_sm != null)
            _wasGrounded = _sm.IsGrounded;

        // Asegurarse de que los bursts están parados al inicio
        if (landDust  != null) landDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (walkDust  != null) walkDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (runDust   != null) runDust.Stop(true,  ParticleSystemStopBehavior.StopEmittingAndClear);

        // Suscribir al evento global de aterrizaje
        GameEvents.OnPlayerLanded += HandleLandedEvent;
    }

    private void OnDestroy()
    {
        GameEvents.OnPlayerLanded -= HandleLandedEvent;
    }

    private void Update()
    {
        HandleDustParticles();
        HandleStaminaVignette();
        HandleHookCameraEffect();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  POLVO – CAMINAR / CORRER
    // ─────────────────────────────────────────────────────────────────────────
    private void HandleDustParticles()
    {
        if (_sm == null) return;

        bool grounded = _sm.IsGrounded;
        bool inGroundedState = _sm.CurrentState is PlayerGroundedState;

        float speed = new Vector2(_sm.Rb.velocity.x, _sm.Rb.velocity.z).magnitude;
        bool moving  = speed > 0.4f;
        bool running = moving && _sm.Input.SprintHeld;
        bool walking = moving && !running;

        // ── Walk dust ────────────────────────────────────────────────────────
        if (walkDust != null)
        {
            bool shouldWalk = grounded && inGroundedState && walking;
            if (shouldWalk && !walkDust.isPlaying) walkDust.Play();
            if (!shouldWalk && walkDust.isPlaying) walkDust.Stop();
        }

        // ── Run dust ─────────────────────────────────────────────────────────
        if (runDust != null)
        {
            bool shouldRun = grounded && inGroundedState && running;
            if (shouldRun && !runDust.isPlaying) runDust.Play();
            if (!shouldRun && runDust.isPlaying) runDust.Stop();
        }

        _wasGrounded = grounded;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  BURST AL ATERRIZAR
    // ─────────────────────────────────────────────────────────────────────────
    private void HandleLandedEvent(float fallDistance)
    {
        if (landDust == null) return;
        if (fallDistance < minLandBurstHeight) return;

        // Escalar el burst según altura: más polvo cuanto mayor la caída
        float scale = Mathf.Clamp(fallDistance / minLandBurstHeight, 1f, 4f);
        var burst = landDust.emission;

        // Ajustar el conteo del primer burst en tiempo de ejecución
        var bursts = new ParticleSystem.Burst[burst.burstCount];
        burst.GetBursts(bursts);
        if (bursts.Length > 0)
        {
            bursts[0].count = new ParticleSystem.MinMaxCurve(
                Mathf.RoundToInt(30 * scale),
                Mathf.RoundToInt(50 * scale)
            );
            burst.SetBursts(bursts);
        }

        landDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        landDust.Play();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  VIÑETA DE STAMINA
    // ─────────────────────────────────────────────────────────────────────────
    private void HandleStaminaVignette()
    {
        if (_stamina == null || _vignette == null) return;

        ColorAdjustments colorAdjustments = null;
        if (globalVolume != null && globalVolume.profile != null)
        {
            if (!globalVolume.profile.TryGet(out colorAdjustments))
                colorAdjustments = globalVolume.profile.Add<ColorAdjustments>();
        }

        float percent = Mathf.InverseLerp(-5f, _stamina.MaxStamina, _stamina.CurrentStamina);

        float targetIntensity = Mathf.Lerp(1f, 0f, percent);
        _vignette.intensity.value = Mathf.Lerp(_vignette.intensity.value, targetIntensity, Time.deltaTime * 5f);
        _vignette.color.value = Color.black;

        if (colorAdjustments != null)
        {
            colorAdjustments.active = true;
            colorAdjustments.saturation.overrideState  = true;
            colorAdjustments.postExposure.overrideState = true;

            colorAdjustments.saturation.value = Mathf.Lerp(-100f, 0f, percent);

            if (_stamina.CurrentStamina <= -4.9f)
                colorAdjustments.postExposure.value = Mathf.Lerp(colorAdjustments.postExposure.value, -10f, Time.deltaTime * 10f);
            else
                colorAdjustments.postExposure.value = Mathf.Lerp(colorAdjustments.postExposure.value, 0f, Time.deltaTime * 5f);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  EFECTO FOV – GANCHO
    // ─────────────────────────────────────────────────────────────────────────
    private void HandleHookCameraEffect()
    {
        if (_sm == null || mainCamera == null) return;

        bool isHooking = _sm.CurrentState is PlayerHookState;

        float targetFOV = isHooking ? _originalFOV + 20f : _originalFOV;
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * 5f);

        if (_lensDistortion != null)
        {
            float targetDistortion = isHooking ? -0.5f : 0f;
            _lensDistortion.intensity.value = Mathf.Lerp(
                _lensDistortion.intensity.value, targetDistortion, Time.deltaTime * 5f);
        }
    }
}
