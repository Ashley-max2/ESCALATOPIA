using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiezaMecanismo : MonoBehaviour
{
    // Indica si el jugador tiene esta pieza
    public bool tieneElJugador = false;

    [Header("Interacci�n")]
    public float distanciaInteraccion = 3f; // Distancia m�xima del raycast
    public GameObject objetoAActivar; // Objeto que se activar� (estaba oculto con SetActive false)

    private Camera camara;

    void Start()
    {
        // Obtener la c�mara principal
        camara = Camera.main;
    }

    void Update()
    {
        // Solo permite interactuar si el jugador tiene la pieza
        if (tieneElJugador && InteractInput.PressedThisFrame())
        {
            // Crear un rayo desde la c�mara hacia delante
            Ray ray = new Ray(camara.transform.position, camara.transform.forward);
            RaycastHit hit;

            // Lanzar el raycast
            if (Physics.Raycast(ray, out hit, distanciaInteraccion))
            {
                // Comprobar si el objeto tiene el tag "mecanismo"
                if (hit.collider.CompareTag("mecanismo"))
                {
                    // Activar el objeto oculto
                    if (objetoAActivar != null)
                    {
                        objetoAActivar.SetActive(true);
                        Destroy(gameObject);
                    }

                    // Desactivar este script para que no vuelva a ejecutarse
                    enabled = false;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detectar si el jugador entra en el trigger
        if (other.CompareTag("Player") && !tieneElJugador)
        {
            // Marcar que el jugador tiene la pieza
            tieneElJugador = true;

            // Hacer este objeto hijo del jugador
            transform.SetParent(other.transform);
            transform.localPosition = Vector3.zero;

            // Ocultar el objeto visualmente (pero sigue existiendo)
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
                rend.enabled = false;

            // Desactivar el collider para evitar m�ltiples activaciones
            Collider col = GetComponent<Collider>();
            if (col != null)
                col.enabled = false;
        }
    }
}
