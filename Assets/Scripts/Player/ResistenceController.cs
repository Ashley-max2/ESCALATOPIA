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
    private float tiempoSinConsumir = 0f;
    private bool estaRegenerando = false;

    // Eventos para notificar cambios (opcional)
    public System.Action<float> OnResistenciaCambiada;
    public System.Action OnResistenciaAgotada;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        resistenciaActual = maxResistencia;

        ActualizarUI();
    }

    void Update()
    {
        ManejarRegeneracion();
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
        if (playerController == null) return false;

        // No regenerar si está en estados que consumen resistencia
        bool estaConsumiendoResistencia =
            playerController.EstaEnEstado<ClimbingState>() ||
            Input.GetKey(KeyCode.R); // Mantengo tu funcionalidad de R

        return !estaConsumiendoResistencia && playerController.EstaEnSuelo();
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

        // Notificar si se agotó la resistencia
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
        // Actualizar texto
        if (resistenciaText != null)
        {
            resistenciaText.text = $"Resistencia: {resistenciaActual:F0}/{maxResistencia}";
        }

        // Actualizar slider
        if (sliderResistencia != null)
        {
            sliderResistencia.value = resistenciaActual;
            sliderResistencia.maxValue = maxResistencia;
        }

        // Notificar cambio
        OnResistenciaCambiada?.Invoke(resistenciaActual);
    }

    // Métodos para debug
    public void DebugLlenarResistencia()
    {
        resistenciaActual = maxResistencia;
        ActualizarUI();
    }

    public void DebugVaciarResistencia()
    {
        resistenciaActual = 0;
        ActualizarUI();
    }
}