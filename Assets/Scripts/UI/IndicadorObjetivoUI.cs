using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Coloca este script en un Canvas principal de UI o en un GameObject gestor de UI.
/// Necesitas asignarle dos imágenes de UI (Image):
/// 1. Un Punto (para cuando miras al objetivo)
/// 2. Una Flecha (para cuando el objetivo está fuera de la pantalla)
/// </summary>
public class IndicadorObjetivoUI : MonoBehaviour
{
    [Header("Referencias a Cámara y UI")]
    [Tooltip("La cámara principal del juego (si lo dejas vacío intentará buscar Camera.main)")]
    public Camera mainCamera;
    
    [Tooltip("La imagen UI que servirá como PUNTO ROJO encima del objetivo")]
    public Image iconoPunto; 
    
    [Tooltip("La imagen UI que servirá como FLECHA indicadora en los bordes")]
    public Image iconoFlecha;    
    
    [Header("Configuración Visual")]
    [Tooltip("Distancia desde el borde de la pantalla a la cual se queda la flecha")]
    public float paddingBorde = 50f; 
    [Tooltip("Altura extra sobre el objeto 3D a la que se colocará el punto rojo")]
    public float alturaExtraPunto = 1f;

    private Transform destinoActual;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        OcultarIndicador();
        
        // Te aseguras de que el punto y la flecha estén tintados de rojo si los sprites fuesen blancos
        if (iconoPunto != null) iconoPunto.color = Color.red;
        if (iconoFlecha != null) iconoFlecha.color = Color.red;
    }

    public void AsignarDestino(Transform nuevoDestino)
    {
        destinoActual = nuevoDestino;
        if (destinoActual == null)
        {
            OcultarIndicador();
        }
    }

    private void OcultarIndicador()
    {
        if (iconoPunto != null) iconoPunto.gameObject.SetActive(false);
        if (iconoFlecha != null) iconoFlecha.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (destinoActual == null)
        {
            OcultarIndicador();
            return;
        }

        if (mainCamera == null) return;

        // Calcular la posición 3D real a dibujar (un poco por encima del centro del objeto)
        Vector3 posicionApunto = destinoActual.position + (Vector3.up * alturaExtraPunto);
        Vector3 screenPos = mainCamera.WorldToScreenPoint(posicionApunto);
        
        // Z menor a 0 significa que está literalmente detrás de nuestra cámara
        bool isBehind = screenPos.z < 0;
        
        // Si además de estar detrás, sobrepasa los márgenes de nuestra pantalla de juego
        bool isOffScreen = isBehind || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 || screenPos.y > Screen.height;

        if (!isOffScreen)
        {
            // -- ESTÁ DENTRO DE LA PANTALLA: MOSTRAR PUNTO ROJO SOBRE EL ONBJETO --
            if (iconoFlecha != null) iconoFlecha.gameObject.SetActive(false);
            if (iconoPunto != null)
            {
                iconoPunto.gameObject.SetActive(true);
                iconoPunto.transform.position = screenPos; // Su imagen seguirá visualmente el modelo 3d
            }
        }
        else
        {
            // -- ESTÁ FUERA DE LA PANTALLA: MOSTRAR FLECHA EN LOS BORDES --
            if (iconoPunto != null) iconoPunto.gameObject.SetActive(false);
            if (iconoFlecha != null)
            {
                iconoFlecha.gameObject.SetActive(true);

                // Si está a nuestra espalda invertimos las coordenadas que nos da Unity 
                if (isBehind)
                {
                    screenPos.x = Screen.width - screenPos.x;
                    screenPos.y = Screen.height - screenPos.y;
                }

                // Cálculo desde el centro exacto de la pantalla
                Vector3 center = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
                Vector3 dirAngle = (screenPos - center).normalized;

                // Calcular ángulo hacia el que debe mirar
                float angle = Mathf.Atan2(dirAngle.y, dirAngle.x);
                float tanAngle = Mathf.Tan(angle); // Pendiente

                // Mantenerlo limitado al rectángulo teniendo en cuenta el padding (márgenes visuales)
                float screenSemiWidth = (Screen.width * 0.5f) - paddingBorde;
                float screenSemiHeight = (Screen.height * 0.5f) - paddingBorde;

                float flechaPosX = 0;
                float flechaPosY = 0;

                // Ver si choca antes con el borde horizontal o vertical
                if (Mathf.Abs(tanAngle) <= (screenSemiHeight / screenSemiWidth)) 
                {
                    // Choca con los bordes izquierdo o derecho de la pantalla
                    flechaPosX = (dirAngle.x > 0 ? screenSemiWidth : -screenSemiWidth);
                    flechaPosY = tanAngle * flechaPosX;
                }
                else 
                {
                    // Choca con los bordes superior o inferior
                    flechaPosY = (dirAngle.y > 0 ? screenSemiHeight : -screenSemiHeight);
                    flechaPosX = flechaPosY / tanAngle;
                }

                // Aplicar coordenadas finales (el centro de la pantalla + la distancia por X e Y)
                iconoFlecha.transform.position = new Vector3(center.x + flechaPosX, center.y + flechaPosY, 0);
                
                // Rotar para que la punta de la flecha señale hacia afuera.
                // NOTA IMPORTANTE: Para que ruede perfecto, la imagen de la Flecha tuya debe estar dibujada 
                // apuntando originalmente hacia la DERECHA.
                iconoFlecha.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
            }
        }
    }
}
