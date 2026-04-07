using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuroGancho : MonoBehaviour
{
    // Indica si el jugador ha recogido el gancho
    public bool tieneGancho = false;

    [Header("Objeto externo a destruir")]
    public GameObject objetoADestruir; // Objeto ajeno que se destruirá

    private void OnTriggerEnter(Collider other)
    {
        // Detecta al jugador
        if (other.CompareTag("Player") && !tieneGancho)
        {
            // Cambia el estado
            tieneGancho = true;

            // Teletransporta el gancho lejos
            transform.position = new Vector3(100000f, 100000f, 100000f);

            // Desactiva el collider del gancho para no repetir
            Collider col = GetComponent<Collider>();
            if (col != null)
                col.enabled = false;

            // Destruye el objeto externo
            if (objetoADestruir != null)
                Destroy(objetoADestruir);
        }
    }
}