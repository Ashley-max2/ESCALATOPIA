using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResistenceController : MonoBehaviour
{
    [Header("Configuración Resistencia")]
    [SerializeField] private float maxResistencia = 100f;
    [SerializeField] private float resistenciaActual;
    [SerializeField] private float velocidadRegeneracion = 5f;
    [SerializeField] private float delayRegeneracion = 2f;

    [Header("Referencias UI")]
    public TextMeshProUGUI resistenciaText;
    public Slider sliderResistencia;

    private PlayerController playerController;
    private PlayerStateMachine playerStateMachine;
    private float tiempoSinConsumir = 0f;
    private bool estaRegenerando = false;

    // Eventos para notificar cambios
    public System.Action<float> OnResistenciaCambiada;
    public System.Action OnResistenciaAgotada;

    // Public property for Max Resistence
    public float MaxResistencia => maxResistencia;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerStateMachine = GetComponent<PlayerStateMachine>();
        resistenciaActual = maxResistencia;

        ActualizarUI();
    }

    void Update()
    {
        ManejarRegeneracion();

        // Testing input
        if (Input.GetKeyDown(KeyCode.R))
        {
            ConsumirResistencia(2f);
        }
    }

    private void ManejarRegeneracion()
    {
        bool puedeRegenerar = PuedeRegenerar();

        if (puedeRegenerar)
        {
            tiempoSinConsumir += Time.deltaTime;

            if (tiempoSinConsumir >= delayRegeneracion && resistenciaActual < maxResistencia)
            {
                RegenerarResistencia();
            }
        }
        else
        {
            tiempoSinConsumir = 0f;
            estaRegenerando = false;
        }
    }

    private bool PuedeRegenerar()
    {
        // Support for new State Machine
        if (playerStateMachine != null)
        {
            return !playerStateMachine.isClimbing && playerStateMachine.isGrounded;
        }

        // Support for old Controller (if present)
        if (playerController != null)
        {
            bool estaConsumiendoResistencia =
                playerController.EstaEnEstado<ClimbingState>() ||
                Input.GetKey(KeyCode.R);
            return !estaConsumiendoResistencia && playerController.EstaEnSuelo();
        }

        return true;
    }

    public void ForceRegenerate()
    {
        RegenerarResistencia();
    }

    private void RegenerarResistencia()
    {
        estaRegenerando = true;
        float regeneracion = velocidadRegeneracion * Time.deltaTime;
        resistenciaActual = Mathf.Clamp(resistenciaActual + regeneracion, 0, maxResistencia);

        ActualizarUI();
    }

    public void ConsumirResistencia(float cantidad)
    {
        if (cantidad <= 0) return;

        resistenciaActual = Mathf.Clamp(resistenciaActual - cantidad, 0, maxResistencia);
        tiempoSinConsumir = 0f;
        estaRegenerando = false;

        ActualizarUI();

        if (resistenciaActual <= 0)
        {
            OnResistenciaAgotada?.Invoke();
        }
    }

    public bool TieneResistencia(float cantidad)
    {
        return resistenciaActual >= cantidad;
    }

    public bool TieneResistenciaSuficiente()
    {
        return resistenciaActual > 0;
    }

    public float GetResistenciaActual()
    {
        return resistenciaActual;
    }

    public float GetResistenciaPorcentaje()
    {
        return resistenciaActual / maxResistencia;
    }

    private void ActualizarUI()
    {
        if (resistenciaText != null)
        {
            resistenciaText.text = $"Resistencia: {resistenciaActual:F0}/{maxResistencia}";
        }

        if (sliderResistencia != null)
        {
            sliderResistencia.value = resistenciaActual;
            sliderResistencia.maxValue = maxResistencia;
        }

        OnResistenciaCambiada?.Invoke(resistenciaActual);
    }

    // Debug methods
    public void DebugLlenarResistencia()
    {
        resistenciaActual = maxResistencia;
        ActualizarUI();
        Debug.Log("Resistencia llenada");
    }

    public void DebugVaciarResistencia()
    {
        resistenciaActual = 0;
        ActualizarUI();
        Debug.Log("Resistencia vaciada");
    }

    public void DebugEstadoResistencia()
    {
        Debug.Log($"Resistencia: {resistenciaActual:F1}/{maxResistencia} | " +
                  $"Puede escalar: {TieneResistencia(1f)} | " +
                  $"Puede WallJump: {TieneResistencia(10f)}");
    }
}