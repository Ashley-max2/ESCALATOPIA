using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiezaMecanismo : MonoBehaviour
{
    public bool tieneElJugador = false;

    [Header("Interacción")]
    public float distanciaInteraccion = 3f;
    public GameObject objetoAActivar; // Lo asignas desde el inspector

    private Camera camara;

    private void Start()
    {
        camara = Camera.main;
    }

    private void Update()
    {
        // Solo puede interactuar si ya tiene la pieza
        if (tieneElJugador && Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(camara.transform.position, camara.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, distanciaInteraccion))
            {
                // Detectar por tag "mecanismo"
                if (hit.collider.CompareTag("mecanismo"))
                {
                    if (objetoAActivar != null)
                    {
                        objetoAActivar.SetActive(true);
                        Destroy(gameObject);
                    }

                    // Aquí termina la lógica de este script
                    enabled = false;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !tieneElJugador)
        {
            tieneElJugador = true;

            transform.SetParent(other.transform);
            transform.localPosition = Vector3.zero;

            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
                rend.enabled = false;

            Collider col = GetComponent<Collider>();
            if (col != null)
                col.enabled = false;
        }
    }
}