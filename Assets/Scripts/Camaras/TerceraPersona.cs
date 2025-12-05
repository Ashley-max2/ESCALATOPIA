using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Configuraci�n de C�mara")]
    public Transform objetivo;
    public float distancia = 4f;
    public float altura = 1.5f;
    public float sensibilidad = 2f;

    [Header("Suavizado Anti-Jitter")]
    public float suavizadoPosicion = 12f;
    public float suavizadoRotacion = 10f;

    [Header("Detecci�n de Colisiones")]
    public LayerMask capasColision;
    public float radioColision = 0.3f;
    public float distanciaMinima = 1f;

    [Header("Efectos de C�mara")]
    public CanvasGroup fadeCanvasGroup;
    public float duracionFade = 0.5f;

    private float rotacionX = 0f;
    private float rotacionY = 0f;
    private Vector3 posicionDeseada;
    private float distanciaActual;
    private Quaternion rotacionDeseada;
    
    // Fade
    private Coroutine fadeCoroutine;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        distanciaActual = distancia;
        
        // Fade in al inicio
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            FadeOut(duracionFade);
        }
    }

    void OnEnable()
    {
        // Fade out cuando se activa la c�mara
        if (fadeCanvasGroup != null && gameObject.activeInHierarchy)
        {
            FadeOut(duracionFade);
        }
    }

    void LateUpdate()
    {
    if (objetivo == null) return;

    // BLOQUEAR INPUT si el jugador está en gancho
    PlayerController player = objetivo.GetComponent<PlayerController>();
    if (player != null && player.EstaGanchoActivo())
    {
        // Mantener la posición actual de la cámara, no procesar input
        return;
    }

    // Input del mouse - independiente del movimiento del jugador
    float mouseX = Input.GetAxis("Mouse X") * sensibilidad;
    float mouseY = Input.GetAxis("Mouse Y") * sensibilidad;
        
        rotacionX += mouseX;
        rotacionY -= mouseY;
        rotacionY = Mathf.Clamp(rotacionY, -35f, 75f);

        // Calcular posici�n objetivo de la c�mara
        Quaternion rotacion = Quaternion.Euler(rotacionY, rotacionX, 0);
        Vector3 puntoObjetivo = objetivo.position + Vector3.up * altura;
        
        // Offset desde el jugador
        Vector3 offset = rotacion * new Vector3(0, 0, -distancia);
        posicionDeseada = puntoObjetivo + offset;

        // Detecci�n de colisiones
        RaycastHit hit;
        Vector3 direccionRayo = posicionDeseada - puntoObjetivo;
        float distanciaRayo = direccionRayo.magnitude;
        
        if (distanciaRayo > 0.1f && Physics.SphereCast(puntoObjetivo, radioColision, 
            direccionRayo.normalized, out hit, distanciaRayo, capasColision))
        {
            distanciaActual = Mathf.Lerp(distanciaActual, Mathf.Max(hit.distance - radioColision * 1.5f, distanciaMinima), Time.deltaTime * 10f);
        }
        else
        {
            distanciaActual = Mathf.Lerp(distanciaActual, distancia, Time.deltaTime * 8f);
        }

        // Recalcular con distancia ajustada
        offset = rotacion * new Vector3(0, 0, -distanciaActual);
        posicionDeseada = puntoObjetivo + offset;

        // Aplicar posici�n con suavizado anti-jitter
        transform.position = Vector3.Lerp(transform.position, posicionDeseada, Time.deltaTime * suavizadoPosicion);

        // Rotaci�n suave mirando al objetivo
        Vector3 direccionMirada = puntoObjetivo - transform.position;
        
        if (direccionMirada.sqrMagnitude > 0.001f)
        {
            rotacionDeseada = Quaternion.LookRotation(direccionMirada);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, 
                Time.deltaTime * suavizadoRotacion);
        }
    }

    #region Efectos de C�mara

    // Fade Out (de negro a transparente - ver la escena)
    public void FadeOut(float duracion = -1f)
    {
        if (fadeCanvasGroup == null) return;
        
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCoroutine(0f, duracion > 0 ? duracion : duracionFade));
    }

    // Fade In (de transparente a negro - ocultar la escena)
    public void FadeIn(float duracion = -1f)
    {
        if (fadeCanvasGroup == null) return;
        
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCoroutine(1f, duracion > 0 ? duracion : duracionFade));
    }

    private IEnumerator FadeCoroutine(float targetAlpha, float duracion)
    {
        float startAlpha = fadeCanvasGroup.alpha;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, tiempo / duracion);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }

    // Cambio suave de posici�n (para momentos narrativos)
    public void CambiarAPosicion(Vector3 nuevaPosicion, Vector3 nuevaRotacion, float duracion = 2f)
    {
        StartCoroutine(TransicionPosicionCoroutine(nuevaPosicion, nuevaRotacion, duracion));
    }

    private IEnumerator TransicionPosicionCoroutine(Vector3 targetPos, Vector3 targetRot, float duracion)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(targetRot);
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracion;
            t = t * t * (3f - 2f * t); // Smoothstep

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = endRot;
    }

    // Zoom suave
    public void CambiarDistancia(float nuevaDistancia, float duracion = 1f)
    {
        StartCoroutine(ZoomCoroutine(nuevaDistancia, duracion));
    }

    private IEnumerator ZoomCoroutine(float targetDistancia, float duracion)
    {
        float startDistancia = distancia;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            distancia = Mathf.Lerp(startDistancia, targetDistancia, tiempo / duracion);
            yield return null;
        }

        distancia = targetDistancia;
    }

    #endregion

    // Método para sincronizar con la cámara de primera persona
    public void SincronizarConPrimeraPersona(FirstPersonCameraController firstPerson)
    {
        if (firstPerson == null) return;
        
        // Obtener la rotación de primera persona y aplicarla a tercera persona
        Vector3 firstPersonRotation = firstPerson.transform.eulerAngles;
        rotacionX = firstPersonRotation.y;
        rotacionY = firstPersonRotation.x;
        
        Debug.Log($"[CAMERA] Sincronizada tercera persona con primera: X={rotacionY}, Y={rotacionX}");
    }

    // Debug visual
    void OnDrawGizmos()
    {
        if (objetivo == null) return;

        Gizmos.color = Color.yellow;
        Vector3 origen = objetivo.position + Vector3.up * altura;
        Gizmos.DrawWireSphere(origen, radioColision);
        Gizmos.DrawLine(origen, transform.position);
    }
}