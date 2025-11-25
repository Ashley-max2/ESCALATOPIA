using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GestorMisiones : MonoBehaviour
{
    public static GestorMisiones Instance;

    [SerializeField] private List<Mision> todasLasMisiones = new List<Mision>();
    private Dictionary<string, Mision> misionesPorId = new Dictionary<string, Mision>();

    // Eventos para notificar cambios
    public static event Action<Mision> OnMisionActualizada;
    public static event Action<Mision> OnMisionCompletada;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Inicializar diccionario
        foreach (var mision in todasLasMisiones)
        {
            misionesPorId[mision.id] = mision;
        }
    }

    public void IniciarMision(string misionId)
    {
        if (misionesPorId.ContainsKey(misionId))
        {
            Mision mision = misionesPorId[misionId];
            if (mision.estado == Mision.EstadoMision.Disponible)
            {
                mision.estado = Mision.EstadoMision.EnProgreso;
                Debug.Log($"Misión iniciada: {mision.titulo}");
                OnMisionActualizada?.Invoke(mision);
            }
        }
    }

    public void ActualizarProgresoMision(string misionId, int cantidad = 1)
    {
        if (misionesPorId.ContainsKey(misionId))
        {
            Mision mision = misionesPorId[misionId];
            if (mision.estado == Mision.EstadoMision.EnProgreso)
            {
                mision.objetivoActual += cantidad;
                Debug.Log($"Progreso misión {mision.titulo}: {mision.objetivoActual}/{mision.objetivoRequerido}");

                // Verificar si se completó
                if (mision.objetivoActual >= mision.objetivoRequerido)
                {
                    mision.estado = Mision.EstadoMision.Completada;
                    Debug.Log($"¡Misión completada: {mision.titulo}!");
                    OnMisionCompletada?.Invoke(mision);
                }

                OnMisionActualizada?.Invoke(mision);
            }
        }
    }

    public void EntregarMision(string misionId)
    {
        if (misionesPorId.ContainsKey(misionId))
        {
            Mision mision = misionesPorId[misionId];
            if (mision.estado == Mision.EstadoMision.Completada)
            {
                mision.estado = Mision.EstadoMision.Entregada;
                OtorgarRecompensa(mision);
                Debug.Log($"Misión entregada: {mision.titulo}");
                OnMisionActualizada?.Invoke(mision);
            }
        }
    }

    private void OtorgarRecompensa(Mision mision)
    {
        Debug.Log($"¡Recompensa obtenida! {mision.experiencia} EXP y {mision.oro} Oro");
        // Aquí puedes conectar con tu sistema de experiencia y dinero del jugador
    }

    public Mision GetMision(string misionId)
    {
        return misionesPorId.ContainsKey(misionId) ? misionesPorId[misionId] : null;
    }

    public List<Mision> GetMisionesDisponibles()
    {
        return todasLasMisiones.Where(m => m.estado == Mision.EstadoMision.Disponible).ToList();
    }

    public List<Mision> GetMisionesEnProgreso()
    {
        return todasLasMisiones.Where(m => m.estado == Mision.EstadoMision.EnProgreso).ToList();
    }

    public List<Mision> GetMisionesCompletadas()
    {
        return todasLasMisiones.Where(m => m.estado == Mision.EstadoMision.Completada).ToList();
    }
}
