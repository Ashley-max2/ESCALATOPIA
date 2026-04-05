using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PiezaMecanismoA : MonoBehaviour
{
    [Header("Timeline")]
    public PlayableDirector timeline; // Timeline que se reproducirá

    [Header("Jugador")]
    public MonoBehaviour scriptMovimientoJugador; // Script de movimiento del jugador

    [Header("Objeto a activar")]
    public GameObject objetoOculto; // Objeto en escena que empieza desactivado

    void Start()
    {
        // Asegurar que la timeline no se reproduce automáticamente
        if (timeline != null)
            timeline.Stop();
    }

    void OnEnable()
    {
        // Este método se ejecuta cuando el objeto pasa de SetActive(false) a true
        IniciarTimeline();
    }

    void IniciarTimeline()
    {
        // Desactivar el control del jugador
        if (scriptMovimientoJugador != null)
            scriptMovimientoJugador.enabled = false;

        // Reproducir la timeline
        if (timeline != null)
            timeline.Play();
    }

    // Esta función se llamará desde la Timeline
    public void ActivarObjeto()
    {
        // Activar el objeto que estaba oculto en la escena
        if (objetoOculto != null)
            objetoOculto.SetActive(true);
    }

    // Esta función se llamará al final de la Timeline
    public void FinTimeline()
    {
        // Reactivar el control del jugador
        if (scriptMovimientoJugador != null)
            scriptMovimientoJugador.enabled = true;
    }
}