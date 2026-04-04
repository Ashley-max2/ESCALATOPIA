using Unity.VisualScripting;
using UnityEngine;

public class ThrownBox : MonoBehaviour
{
    private bool esLanzado = false;  // Controla si el objeto ha sido lanzado

    // Este método se llama cuando el objeto ha sido lanzado desde el script del jugador
    public void MarcarComoLanzado()
    {
        esLanzado = true;
    }

    // Detecta la colisión con el suelo
    private void OnCollisionEnter(Collision collision)
    {
        // Si el objeto ha sido lanzado y colisiona con la capa "Ground", lo destruye
        if (esLanzado && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Comprobar si tiene hijos
            if (transform.childCount == 0)
            {
                Debug.Log("No tiene hijos para desparente.");
            }
            else
            {
                Debug.Log("Tiene hijos, desparentando.");
                transform.DetachChildren();  // Desparentea los hijos
            }

            Destroy(gameObject);  // Destruye el objeto
        }
    }
}