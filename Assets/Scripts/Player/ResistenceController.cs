using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResistenceController : MonoBehaviour
{
    float MaxBar = 100;
    float ResistenceBar;
    public PlayerJump playerJump;
    public Escalada escalada;
    public float velocidadRegeneracion = 5f;
    public TextMeshProUGUI resistenciaText;
    public Slider sliderResistencia;

    private float tiempoEnSuelo = 0f;
    private float delayRegeneracion = 2f;


    // Start is called before the first frame update
    void Start()
    {
        ResistenceBar = MaxBar;

        if (playerJump == null)
        {
            playerJump = GetComponent<PlayerJump>();
        }

        if (escalada == null)
        {
            escalada = GetComponent<Escalada>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool estaEnSuelo = playerJump != null && playerJump.EstaEnSuelo();
        bool presionandoR = Input.GetKey(KeyCode.R);

        // Consumir resistencia si se presiona R
        if (Input.GetKeyDown(KeyCode.R))
        {
            ConsumirResistencia(2f);
        }

        // Control de temporizador para regeneración
        if (estaEnSuelo && !presionandoR)
        {
            tiempoEnSuelo += Time.deltaTime;

            if (tiempoEnSuelo >= delayRegeneracion)
            {
                ResistenceBar += velocidadRegeneracion * Time.deltaTime;
                ResistenceBar = Mathf.Clamp(ResistenceBar, 0, MaxBar);
            }
        }
        else
        {
            // Reiniciar temporizador si no está en suelo o se presiona R
            tiempoEnSuelo = 0f;
        }

        actulizarTexto();

        if (sliderResistencia != null)
        {
            sliderResistencia.value = ResistenceBar;
        }
    }

    void actulizarTexto()
    {
        resistenciaText.text = "Resistencia: " + ResistenceBar.ToString("F0") + "/" + MaxBar.ToString();
    }

    public void ConsumirResistencia(float cantidad)
    {
        ResistenceBar -= cantidad;
        ResistenceBar = Mathf.Clamp(ResistenceBar, 0, MaxBar);
    }

    public bool TieneResistencia(float cantidad)
    {
        return ResistenceBar >= cantidad;

    }

}
