using UnityEngine;
using UnityEngine.UI;

public class punteia : MonoBehaviour
{
    [Header("=== IMAGENES ===")]
    [Tooltip("Imagen de fondo del indicador (cambia de color al estar en rango)")]
    public Image fondo;

    [Tooltip("Icono del gancho para ENGANCHARSE a hookpoints")]
    public Image iconoEnganchar;

    [Tooltip("Icono del gancho para ATRAER/RECOGER objetos")]
    public Image iconoAtraer;

    [Header("=== COLORES FONDO ===")]
    [Tooltip("Color del fondo cuando NO hay hookpoint en rango")]
    public Color colorFondoInactivo = new Color(0.2f, 0.2f, 0.2f, 0.6f);

    [Tooltip("Color del fondo cuando HAY hookpoint en rango (modo enganchar)")]
    public Color colorFondoEnganchar = new Color(0f, 1f, 0.5f, 0.8f);

    [Tooltip("Color del fondo cuando HAY objeto atraíble en rango (modo atraer)")]
    public Color colorFondoAtraer = new Color(1f, 0.6f, 0f, 0.8f);

    [Header("=== COLORES ICONOS ===")]
    [Tooltip("Color del icono cuando está ACTIVO")]
    public Color colorIconoActivo = Color.white;

    [Tooltip("Color del icono cuando está INACTIVO")]
    public Color colorIconoInactivo = new Color(1f, 1f, 1f, 0.25f);

    private bool modoActualEsPull = false;

    void Start()
    {
        ActualizarIconos(false);

        if (fondo != null)
            fondo.color = colorFondoInactivo;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            modoActualEsPull = !modoActualEsPull;
            ActualizarIconos(modoActualEsPull);
        }
    }

    void ActualizarIconos(bool esPull)
    {
        if (iconoEnganchar != null)
        {
            iconoEnganchar.color = esPull ? colorIconoInactivo : colorIconoActivo;
        }

        if (iconoAtraer != null)
        {
            iconoAtraer.color = esPull ? colorIconoActivo : colorIconoInactivo;
        }
    }

    public void ActivarColorHookpoint()
    {
        if (fondo != null)
        {
            fondo.color = modoActualEsPull ? colorFondoAtraer : colorFondoEnganchar;
        }
    }

    public void DesactivarColorHookpoint()
    {
        if (fondo != null)
        {
            fondo.color = colorFondoInactivo;
        }
    }
}
