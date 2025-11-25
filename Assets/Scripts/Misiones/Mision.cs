using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Mision
{
    public string id;
    public string titulo;
    public string descripcion;
    public TipoMision tipo;
    public EstadoMision estado;

    // Objetivos
    public int objetivoActual;
    public int objetivoRequerido;

    // Recompensas
    public int experiencia;
    public int oro;
    public string itemRecompensa;

    public enum TipoMision
    {
        Recoleccion,
        Eliminacion,
        Entrega,
        Exploracion
    }

    public enum EstadoMision
    {
        NoDisponible,
        Disponible,
        EnProgreso,
        Completada,
        Entregada
    }
}