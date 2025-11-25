using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCConMisiones : MonoBehaviour
{
    [SerializeField] private string[] misionesDisponibles;
    [SerializeField] private GameObject indicadorMision;

    private void Start()
    {
        ActualizarIndicador();
        GestorMisiones.OnMisionActualizada += (mision) => ActualizarIndicador();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MostrarInterfazMisiones();
        }
    }

    private void MostrarInterfazMisiones()
    {
        // Ofrecer misiones disponibles
        foreach (string misionId in misionesDisponibles)
        {
            Mision mision = GestorMisiones.Instance.GetMision(misionId);
            if (mision != null && mision.estado == Mision.EstadoMision.Disponible)
            {
                // Aquí podrías mostrar un diálogo
                Debug.Log($"NPC ofrece misión: {mision.titulo}");
                GestorMisiones.Instance.IniciarMision(misionId);
            }
        }

        // Entregar misiones completadas
        foreach (string misionId in misionesDisponibles)
        {
            Mision mision = GestorMisiones.Instance.GetMision(misionId);
            if (mision != null && mision.estado == Mision.EstadoMision.Completada)
            {
                Debug.Log($"Puedes entregar la misión: {mision.titulo}");
                // En un sistema real, mostrarías un botón para entregar
            }
        }
    }

    private void ActualizarIndicador()
    {
        bool tieneMisionesParaEntregar = false;
        bool tieneMisionesDisponibles = false;

        foreach (string misionId in misionesDisponibles)
        {
            Mision mision = GestorMisiones.Instance.GetMision(misionId);
            if (mision != null)
            {
                if (mision.estado == Mision.EstadoMision.Completada)
                    tieneMisionesParaEntregar = true;
                else if (mision.estado == Mision.EstadoMision.Disponible)
                    tieneMisionesDisponibles = true;
            }
        }

        if (indicadorMision != null)
        {
            indicadorMision.SetActive(tieneMisionesDisponibles || tieneMisionesParaEntregar);

            if (tieneMisionesParaEntregar)
            {
                indicadorMision.GetComponent<Renderer>().material.color = Color.green;
            }
            else if (tieneMisionesDisponibles)
            {
                indicadorMision.GetComponent<Renderer>().material.color = Color.yellow;
            }
        }
    }
}
