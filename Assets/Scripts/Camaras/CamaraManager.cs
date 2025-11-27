using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Referencias de Cámaras")]
    public Camera camaraTerceraPersona;
    public Camera camaraPrimeraPersona;

    private bool enPrimeraPersona = false;

    void Start()
    {
        // Validar referencias
        if (camaraTerceraPersona == null || camaraPrimeraPersona == null)
        {
            Debug.LogError("Faltan referencias de cámaras en el CameraManager");
            return;
        }

        // Configurar estado inicial - 3ra persona por defecto
        camaraTerceraPersona.enabled = true;
        camaraPrimeraPersona.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
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

        enPrimeraPersona = true;
        camaraTerceraPersona.enabled = false;
        camaraPrimeraPersona.enabled = true;
    }

    private void CambiarATerceraPersona()
    {
        if (!enPrimeraPersona) return;

        enPrimeraPersona = false;
        camaraPrimeraPersona.enabled = false;
        camaraTerceraPersona.enabled = true;
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