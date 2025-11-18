using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaltoEscalado : MonoBehaviour
{
      public float fuerzaSalto = 12f;        // Fuerza vertical
    public float fuerzaLateral = 8f;       // Fuerza hacia el lado
    public float bloqueoMovimientoTiempo = 0.15f; // Para que el movimiento no cancele el salto

    private Escalada escalada;
    private Rigidbody rb;
    private PlayerMovement movimiento;

    private ResistenceController resistenceController;

    private bool saltandoDesdePared = false;
    private float temporizadorBloqueo = 0f;

    void Start()
    {
        escalada = GetComponent<Escalada>();
        rb = GetComponent<Rigidbody>();
        movimiento = GetComponent<PlayerMovement>();
        resistenceController = GetComponent<ResistenceController>();
    }

    void Update()
    {
        if (escalada == null || rb == null) return;

        // Evitar que Movement cancele el salto
        if (saltandoDesdePared)
        {
            temporizadorBloqueo -= Time.deltaTime;
            if (temporizadorBloqueo <= 0)
            {
                movimiento.enabled = true;
                saltandoDesdePared = false;
            }
        }

        // Si estás escalando y pulsas salto
        if (escalada.EstaEscalando() && Input.GetKeyDown(KeyCode.Space))
        {
            HacerWallJump();
        }
    }

    void HacerWallJump()
    {
    if (resistenceController != null && !resistenceController.TieneResistencia(10f))
    {
        Debug.Log("No hay resistencia para salto de pared");
        return;
    }

    // 2) Gastar resistencia
    if (resistenceController != null)
    {
        resistenceController.ConsumirResistencia(10f);
    }

    // Detener escalada
    escalada.ForzarFinEscalada();



    // Bloquear PlayerMovement un momento
    movimiento.enabled = false;
    saltandoDesdePared = true;
    temporizadorBloqueo = bloqueoMovimientoTiempo;

    // Reset velocidad para que el salto sea limpio
    rb.velocity = Vector3.zero;

    // ⚡ SALTO DIAGONAL RÁPIDO
    // Hacia arriba + hacia atrás (o hacia el lado opuesto de la pared)
    Vector3 direccionDiagonal = (-transform.forward * fuerzaLateral + Vector3.up * fuerzaSalto);

    // AÑADIMOS VELOCIDAD EXTRA PARA QUE SEA MÁS RÁPIDO
    Vector3 velocidadExtra = direccionDiagonal * 1.5f; 

    // Aplicar fuerza instantánea y rápida
    rb.AddForce(velocidadExtra, ForceMode.VelocityChange);

    Debug.Log("WALL JUMP DIAGONAL RÁPIDO!");
    
    }
}   