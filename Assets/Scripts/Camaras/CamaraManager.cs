using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Referencias de C�maras")]
    public Camera camaraTerceraPersona;
    public Camera camaraPrimeraPersona;

    [Header("Efectos de Transici�n")]
    public CanvasGroup fadeCanvasGroup;
    public float duracionFade = 0.3f;

    private bool enPrimeraPersona = false;
    private ThirdPersonCameraController terceraPersonaController;
    private Coroutine fadeCoroutine;

    void Start()
    {
        // Validar referencias
        if (camaraTerceraPersona == null || camaraPrimeraPersona == null)
        {
            Debug.LogError("Faltan referencias de c�maras en el CameraManager");
            return;
        }

        // Obtener el controlador de tercera persona
        terceraPersonaController = camaraTerceraPersona.GetComponent<ThirdPersonCameraController>();

        // Configurar estado inicial - 3ra persona por defecto
        camaraTerceraPersona.enabled = true;
        camaraPrimeraPersona.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        
        // Fade inicial desde negro
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            StartCoroutine(FadeCoroutine(0f, duracionFade));
        }
    }

    void Update()
    {
        ProcesarInput();
    }

    private void ProcesarInput()
    {
        // Cambio con click derecho
        if (Input.GetMouseButtonDown(1))
        {
            CambiarAPrimeraPersona();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            CambiarATerceraPersona();
        }

        // Toggle cursor con Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursor();
        }
    }

    private void CambiarAPrimeraPersona()
    {
        if (enPrimeraPersona) return;

        StartCoroutine(CambiarCamaraConFade(true));
    }

    private void CambiarATerceraPersona()
    {
        if (!enPrimeraPersona) return;

        StartCoroutine(CambiarCamaraConFade(false));
    }

    private IEnumerator CambiarCamaraConFade(bool aPrimeraPersona)
    {
        // Fade in (a negro)
        if (fadeCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCoroutine(1f, duracionFade));
        }

        // Cambiar c�mara
        enPrimeraPersona = aPrimeraPersona;
        camaraPrimeraPersona.enabled = aPrimeraPersona;
        camaraTerceraPersona.enabled = !aPrimeraPersona;

        // Peque�a pausa
        yield return new WaitForSeconds(0.1f);

        // Fade out (transparente)
        if (fadeCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCoroutine(0f, duracionFade));
        }
    }

    private IEnumerator FadeCoroutine(float targetAlpha, float duracion)
    {
        if (fadeCanvasGroup == null) yield break;

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

    private void ToggleCursor()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public bool EstaEnPrimeraPersona()
    {
        return enPrimeraPersona;
    }
}