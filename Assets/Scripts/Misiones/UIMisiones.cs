using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMisiones : MonoBehaviour
{
    [SerializeField] private Transform contentMisiones;
    [SerializeField] private GameObject prefabElementoMision;

    private void Start()
    {
        GestorMisiones.OnMisionActualizada += ActualizarUI;
        ActualizarUI(null);
    }

    private void OnDestroy()
    {
        GestorMisiones.OnMisionActualizada -= ActualizarUI;
    }

    private void ActualizarUI(Mision mision)
    {
        // Limpiar UI
        foreach (Transform child in contentMisiones)
        {
            Destroy(child.gameObject);
        }

        // Mostrar misiones en progreso
        var misionesEnProgreso = GestorMisiones.Instance.GetMisionesEnProgreso();
        foreach (var misionActual in misionesEnProgreso)
        {
            GameObject elemento = Instantiate(prefabElementoMision, contentMisiones);
            elemento.GetComponent<UIElementoMision>().Configurar(misionActual);
        }

        // Mostrar misiones completadas
        var misionesCompletadas = GestorMisiones.Instance.GetMisionesCompletadas();
        foreach (var misionActual in misionesCompletadas)
        {
            GameObject elemento = Instantiate(prefabElementoMision, contentMisiones);
            elemento.GetComponent<UIElementoMision>().Configurar(misionActual);
        }
    }
}