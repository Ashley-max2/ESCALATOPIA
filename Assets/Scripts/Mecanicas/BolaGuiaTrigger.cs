using UnityEngine;

/// <summary>
/// Úsalo como zona en el suelo o asociado a un objeto que quieras que actualice el marcador.
/// Al entrar al trigger, se llamará a la bola guía para avanzar al siguiente objetivo.
/// </summary>
public class BolaGuiaTrigger : MonoBehaviour
{
    [Tooltip("Rastrea aquí al objeto BolaGuiaNPC de la escena")]
    public BolaGuiaNPC bolaGuia;

    [Tooltip("Si marcas esto, el trigger no se desactivará e intentará actualizar cada vez que entres. Mantenlo desmarcado para que suceda solo 1 vez.")]
    public bool noDesactivarAlUsar = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Comprueba que sea el jugador quien entra
        {
            if (bolaGuia != null)
            {
                bolaGuia.CompletarObjetivoActual();
                
                if (!noDesactivarAlUsar)
                {
                    // Desactivar el GameObject que tiene el Trigger para que no mande señales repetidas
                    gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning("BolaGuiaTrigger intentó completarse en " + gameObject.name + " pero NO tiene asignada la Bola Guia.");
            }
        }
    }

    // OPCIONAL: Funciones extra si en el futuro quieres llamarlo mediante botones, recoger un item o UnityEvents
    public void CompletarManualmente()
    {
        if (bolaGuia != null)
        {
            bolaGuia.CompletarObjetivoActual();
        }
    }
}
