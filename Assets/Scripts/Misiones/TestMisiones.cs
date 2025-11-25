using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMisiones : MonoBehaviour
{
    [Header("Configuración de Test")]
    public string misionId = "mision_1";
    public int cantidadProgreso = 1;

    [Header("Detección de Items")]
    public string tagItem = "ItemMision"; // Puedes cambiarlo en el Inspector

    void Update()
    {
        // SIMULAR PROGRESO DE MISIÓN - Tecla C
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log($"Añadiendo progreso a misión: {misionId}");
            GestorMisiones.Instance.ActualizarProgresoMision(misionId, cantidadProgreso);
        }

        // SIMULAR ENTREGA DE MISIÓN - Tecla E
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"Intentando entregar misión: {misionId}");
            GestorMisiones.Instance.EntregarMision(misionId);
        }

        // REINICIAR MISIÓN - Tecla R (para testing)
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log($"Reiniciando misión: {misionId}");
            ReiniciarMision(misionId);
        }
    }

    private void ReiniciarMision(string idMision)
    {
        Mision mision = GestorMisiones.Instance.GetMision(idMision);
        if (mision != null)
        {
            mision.estado = Mision.EstadoMision.Disponible;
            mision.objetivoActual = 0;
            GestorMisiones.Instance.ActualizarProgresoMision(idMision, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // SOLUCIÓN: Verificar si el tag existe antes de usarlo
        if (!string.IsNullOrEmpty(tagItem) && other.CompareTag(tagItem))
        {
            Debug.Log("Item de misión recolectado");
            GestorMisiones.Instance.ActualizarProgresoMision(misionId, 1);
            Destroy(other.gameObject);
        }

        // DETECCIÓN ALTERNATIVA si no quieres usar tags
        else if (other.gameObject.name.Contains("Item") || other.gameObject.name.Contains("Coin"))
        {
            Debug.Log("Item recolectado por nombre: " + other.gameObject.name);
            GestorMisiones.Instance.ActualizarProgresoMision(misionId, 1);
            Destroy(other.gameObject);
        }
    }
}