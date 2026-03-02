using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeTrigger : MonoBehaviour
{
    [Tooltip("El nombre exacto de la escena a cargar al chocar con el collider")]
    public string sceneName;

    [Header("Puerta Final (Boss)")]
    [Tooltip("Si esta activado, la puerta esta bloqueada hasta que se desbloquee (ganar al boss)")]
    [SerializeField] private bool locked = false;

    /// <summary>
    /// Desbloquea la puerta (llamado por BossRaceManager al ganar)
    /// </summary>
    public void Unlock()
    {
        locked = false;
    }

    /// <summary>
    /// Bloquea la puerta
    /// </summary>
    public void Lock()
    {
        locked = true;
    }

    public bool IsLocked => locked;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (locked)
            {
                Debug.Log("Puerta bloqueada: debes ganar al boss primero.");
                return;
            }

            if (!string.IsNullOrEmpty(sceneName))
            {
                Debug.Log("Cargando Escena");
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning("SceneChangeTrigger: No hay nombre de escena asignado.");
            }
        }
    }
}
