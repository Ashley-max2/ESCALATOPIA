using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class ObjetivoGuia
{
    [Tooltip("Nombre interno para organizarte en el inspector")]
    public string nombreObjetivo = "Nuevo Objetivo";
    
    [Tooltip("Textos que dice la bola en pantalla cuando empieza este objetivo")]
    [TextArea(2, 4)]
    public List<string> textosAlEmpezar;
    
    [Tooltip("El sitio al que debes ir. Se completará SIEMPRE de forma automática al ponerte encima/llegar.")]
    public Transform destinoAMarcar;
    
    [Tooltip("Distancia a la redonda (en metros) a la que cuenta como que has llegado.")]
    public float distanciaParaCompletar = 3f;
}

public class BolaGuiaNPC : MonoBehaviour
{
    [Header("Referencias Player")]
    [Tooltip("Arrastra al jugador o a la cámara del jugador aquí")]
    public Transform player;

    [Header("Configuración de Vuelo")]
    [Tooltip("Distancia a la que flota alejado del jugador hacia adelante o atrás")]
    public float distanciaDelPlayer = 2f;
    [Tooltip("Distancia hacia la DERECHA del jugador a la que se posiciona (usa negativo para ponerlo a la izquierda)")]
    public float distanciaDerecha = 1.5f;
    [Tooltip("Altura a la que flota sobre el suelo / jugador")]
    public float alturaDeVuelo = 1.5f;
    [Tooltip("Rapidez con la que sigue al Player de forma fluida")]
    public float velocidadSeguimiento = 5f;
    [Tooltip("Rapidez con la que gira para mirar a lugares")]
    public float velocidadRotacion = 8f;

    [Header("UI Dialogo")]
    [Tooltip("Panel (ej. fondo negro) que contiene el texto del NPC")]
    public GameObject panelDialogo;
    [Tooltip("Componente TextMeshProUGUI donde se mostrarán las frases")]
    public TextMeshProUGUI textoDialogo;
    [Tooltip("Segundos que se muestra cada texto antes de pasar al siguiente")]
    public float tiempoPorTexto = 4f;

    [Header("Indicadores en Pantalla (Opcional)")]
    [Tooltip("Arrastra aquí el objeto que tiene el script IndicadorObjetivoUI")]
    public IndicadorObjetivoUI indicadorPuntoYFlecha;

    [Header("Sistema de Objetivos Secuenciales")]
    [Tooltip("Agrega aquí la lista de pasos que quieres que haga el jugador en orden.")]
    public List<ObjetivoGuia> objetivos = new List<ObjetivoGuia>();
    
    // Índice público para que puedas probar en el inspector
    public int indiceObjetivoActual = 0;

    private Coroutine corrutinaDialogo;
    private float tiempoProteccionCambio = 0f;

    void Start()
    {
        if (panelDialogo != null) panelDialogo.SetActive(false);

        // Al iniciar, si hay objetivos en la lista arranca el primero
        if (objetivos.Count > 0)
        {
            IniciarObjetivo();
        }
    }

    void Update()
    {
        if (player == null) return;

        MoverYRotarBola();
        ComprobarCompletadoPorDistancia();
    }

    private void MoverYRotarBola()
    {
        Vector3 posicionDestino;
        Vector3 direccionMirada;

        ObjetivoGuia objetivoActual = ObtenerObjetivoActual();

        // 1. Calcular a dónde debe volar la bola y a dónde debe mirar
        if (objetivoActual != null && objetivoActual.destinoAMarcar != null)
        {
            // Se coloca entre el jugador y el destino para indicar la dirección
            Vector3 dirHaciaDestino = (objetivoActual.destinoAMarcar.position - player.position).normalized;
            // Aplanamos la dirección para que no intente meterse bajo tierra si el objeto está abajo
            // Se coloca mirando hacia allí pero mantenemos tu configuración de altura y distancia a la derecha
            Vector3 despLateral = player.transform.right * distanciaDerecha;
            posicionDestino = player.position + despLateral + (dirHaciaDestino * distanciaDelPlayer) + (Vector3.up * alturaDeVuelo);
            
            // La bola mira fijamente hacia donde tenemos que ir
            direccionMirada = (objetivoActual.destinoAMarcar.position - transform.position).normalized;
        }
        else
        {
            // Si no hay destino, flota atrás y a la distancia derecha configurada
            Vector3 offsetClasico = new Vector3(distanciaDerecha, alturaDeVuelo, -distanciaDelPlayer * 0.5f);
            posicionDestino = player.position + player.TransformDirection(offsetClasico);
            
            // Mira en la misma dirección hacia la que va el jugador
            direccionMirada = player.forward;
        }

        // 2. Movimiento fluido mediante Lerp
        transform.position = Vector3.Lerp(transform.position, posicionDestino, Time.deltaTime * velocidadSeguimiento);

        // 3. Rotación fluida
        if (direccionMirada != Vector3.zero)
        {
            Quaternion rotacionDestino = Quaternion.LookRotation(direccionMirada);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDestino, Time.deltaTime * velocidadRotacion);
        }
    }

    private void ComprobarCompletadoPorDistancia()
    {
        // Un seguro de tiempo para evitar que salten 2 seguidos de golpe por error
        if (Time.time < tiempoProteccionCambio) return;

        ObjetivoGuia objetivoActual = ObtenerObjetivoActual();
        if (objetivoActual != null && objetivoActual.destinoAMarcar != null)
        {
            float distancia = 0f;
            
            // Si el objeto destino tiene un Collider, calculamos la distancia al borde de la caja
            Collider colDestino = objetivoActual.destinoAMarcar.GetComponentInChildren<Collider>();
            if (colDestino != null)
            {
                Vector3 puntoMasCercano = colDestino.ClosestPoint(player.position);
                distancia = Vector3.Distance(player.position, puntoMasCercano);
            }
            else
            {
                // Si no tiene Collider, miramos la distancia 3D normal entre centros (incluyendo altura Y)
                distancia = Vector3.Distance(player.position, objetivoActual.destinoAMarcar.position);
            }

            if (distancia <= objetivoActual.distanciaParaCompletar)
            {
                Debug.Log($"<color=green>[BolaGuia]</color> Objetivo superado. Distancia del jugador a {objetivoActual.destinoAMarcar.gameObject.name}: {distancia}m.");
                CompletarObjetivoActual();
            }
        }
    }

    /// <summary>
    /// Llama a esta función desde otros scripts (ej: un Trigger o recoger un Item) para avanzar la misión
    /// </summary>
    public void CompletarObjetivoActual()
    {
        if (indiceObjetivoActual < objetivos.Count)
        {
            indiceObjetivoActual++;
            if (indiceObjetivoActual < objetivos.Count)
            {
                // Hay un siguiente objetivo
                IniciarObjetivo();
            }
            else
            {
                // Has terminado el recorrido completo
                if (corrutinaDialogo != null) StopCoroutine(corrutinaDialogo);
                if (panelDialogo != null) panelDialogo.SetActive(false);
                if (indicadorPuntoYFlecha != null) indicadorPuntoYFlecha.AsignarDestino(null);
                Debug.Log("Bola Guía: ¡Todos los objetivos del recorrido actual han sido completados!");
            }
        }
    }

    private void IniciarObjetivo()
    {
        ObjetivoGuia obj = ObtenerObjetivoActual();
        if (obj != null)
        {
            // Seguro para que no se autocompleten de inmediato (da 1.5s de buffer)
            tiempoProteccionCambio = Time.time + 1.5f;

            // Asignar al marcador UI
            if (indicadorPuntoYFlecha != null)
            {
                indicadorPuntoYFlecha.AsignarDestino(obj.destinoAMarcar);
            }

            // Iniciar sus textos si tiene
            if (obj.textosAlEmpezar != null && obj.textosAlEmpezar.Count > 0)
            {
                if (corrutinaDialogo != null)
                {
                    StopCoroutine(corrutinaDialogo);
                }
                corrutinaDialogo = StartCoroutine(MostrarDialogos(obj.textosAlEmpezar));
            }
        }
    }

    private IEnumerator MostrarDialogos(List<string> textos)
    {
        if (panelDialogo != null) panelDialogo.SetActive(true);

        foreach (string texto in textos)
        {
            if (textoDialogo != null) textoDialogo.text = texto;
            yield return new WaitForSeconds(tiempoPorTexto);
        }

        if (panelDialogo != null) panelDialogo.SetActive(false);
    }

    private ObjetivoGuia ObtenerObjetivoActual()
    {
        if (indiceObjetivoActual >= 0 && indiceObjetivoActual < objetivos.Count)
        {
            return objetivos[indiceObjetivoActual];
        }
        return null;
    }
}
