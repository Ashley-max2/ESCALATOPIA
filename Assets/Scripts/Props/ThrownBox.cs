using Unity.VisualScripting;
using UnityEngine;

public class ThrownBox : MonoBehaviour
{
    private bool esLanzado = false;

    public void MarcarComoLanzado()
    {
        esLanzado = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (esLanzado && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // ACTIVAR FRAGMENTOS
            GetComponent<BarrilFragmentado>()?.ActivarFragmentos();

            if (transform.childCount == 0)
            {
                Debug.Log("No tiene hijos para desparente.");
            }
            else
            {
                transform.DetachChildren();
            }

            Destroy(gameObject);
        }
    }
}