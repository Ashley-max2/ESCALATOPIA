using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI de estamina simple que muestra una barra de progreso.
/// Se conecta automáticamente al StaminaSystem del jugador.
/// </summary>
public class StaminaUI : MonoBehaviour
{
    [Header("=== REFERENCES ===")]
    [SerializeField] private Image staminaFill;
    [SerializeField] private Image staminaBackground;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI staminaText;
    
    [Header("=== COLORS ===")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.8f, 0.3f);
    [SerializeField] private Color warningColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color criticalColor = new Color(1f, 0.3f, 0.2f);
    
    [Header("=== ANIMATION ===")]
    [SerializeField] private float fadeSpeed = 3f;
    [SerializeField] private float showDuration = 2f;
    [SerializeField] private bool alwaysShow = false;
    
    private StaminaSystem _staminaSystem;
    private float _hideTimer;
    private float _targetAlpha;
    
    private void Start()
    {
        // Find stamina system on player
        var player = FindObjectOfType<PlayerStateMachine>();
        if (player != null)
        {
            _staminaSystem = player.GetComponent<StaminaSystem>();
            if (_staminaSystem != null)
            {
                _staminaSystem.OnStaminaChanged += OnStaminaChanged;
                _staminaSystem.OnStaminaWarning += OnStaminaWarning;
                _staminaSystem.OnStaminaDepleted += OnStaminaDepleted;
            }
        }
        
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // Start hidden
        if (!alwaysShow)
        {
            _targetAlpha = 0;
            canvasGroup.alpha = 0;
        }
    }
    
    private void Update()
    {
        // Fade animation
        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, _targetAlpha, Time.deltaTime * fadeSpeed);
        }
        
        // Hide timer
        if (!alwaysShow && _targetAlpha > 0)
        {
            _hideTimer -= Time.deltaTime;
            if (_hideTimer <= 0 && _staminaSystem != null && _staminaSystem.StaminaPercent >= 0.99f)
            {
                _targetAlpha = 0;
            }
        }
    }
    
    private void OnStaminaChanged(float current, float max)
    {
        if (staminaFill == null) return;
        
        float percent = current / max;
        staminaFill.fillAmount = percent;
        
        // Update color based on level
        if (percent <= 0.1f)
        {
            staminaFill.color = criticalColor;
        }
        else if (percent <= 0.25f)
        {
            staminaFill.color = warningColor;
        }
        else
        {
            staminaFill.color = normalColor;
        }
        
        // Update text with percentage
        if (staminaText != null)
        {
            staminaText.text = Mathf.CeilToInt(percent * 100).ToString() + "%";
        }
        
        // Show UI
        ShowTemporarily();
    }
    
    private void OnStaminaWarning()
    {
        // Pulse effect could be added here
        ShowTemporarily();
    }
    
    private void OnStaminaDepleted()
    {
        // Flash effect could be added here
        ShowTemporarily();
    }
    
    private void ShowTemporarily()
    {
        _targetAlpha = 1;
        _hideTimer = showDuration;
    }
    
    private void OnDestroy()
    {
        if (_staminaSystem != null)
        {
            _staminaSystem.OnStaminaChanged -= OnStaminaChanged;
            _staminaSystem.OnStaminaWarning -= OnStaminaWarning;
            _staminaSystem.OnStaminaDepleted -= OnStaminaDepleted;
        }
    }
}
