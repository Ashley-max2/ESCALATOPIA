using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIElementoMision : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textoTitulo;
    [SerializeField] private TextMeshProUGUI textoDescripcion;
    [SerializeField] private TextMeshProUGUI textoProgreso;
    [SerializeField] private Button botonEntregar;

    private Mision mision;

    public void Configurar(Mision mision)
    {
        this.mision = mision;

        textoTitulo.text = mision.titulo;
        textoDescripcion.text = mision.descripcion;

        if (mision.estado == Mision.EstadoMision.Completada)
        {
            textoProgreso.text = "¡COMPLETADA!";
            textoProgreso.color = Color.green;
            botonEntregar.gameObject.SetActive(true);
        }
        else if (mision.estado == Mision.EstadoMision.EnProgreso)
        {
            textoProgreso.text = $"{mision.objetivoActual}/{mision.objetivoRequerido}";
            textoProgreso.color = Color.yellow;
            botonEntregar.gameObject.SetActive(false);
        }
        else
        {
            botonEntregar.gameObject.SetActive(false);
        }
    }

    public void EntregarMision()
    {
        if (mision != null)
        {
            GestorMisiones.Instance.EntregarMision(mision.id);
        }
    }
}