using Unity.VisualScripting;
using UnityEngine;

public class ThrownBox : MonoBehaviour
{
    private bool esLanzado = false;

    [SerializeField] private ParticleSystem particulas;

    public void MarcarComoLanzado()
    {
        esLanzado = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (esLanzado && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Si el barril tiene MÁS DE 2 hijos
            if (transform.childCount > 2)
            {
                if (particulas != null)
                {
                    // Las partículas empiezan a emitir
                    particulas.Play();
                }
            }

            // ACTIVAR FRAGMENTOS
            GetComponent<BarrilFragmentado>()?.ActivarFragmentos();

            // Desparentar hijos (incluidas partículas)
            if (transform.childCount == 0)
            {
                Debug.Log("No tiene hijos para desparente.");
            }
            else
            {
                transform.DetachChildren();
            }

            // Destruir barril
            Destroy(gameObject);
        }
    }
}