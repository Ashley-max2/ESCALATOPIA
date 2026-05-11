using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Al tocar la gema (trigger o colisión), cambia de escena.
/// Asegúrate de que la escena destino esté en File > Build Settings.
/// </summary>
public class SceneChangerGema : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Nombre exacto de la escena a la que quieres ir (debe estar en Build Settings)")]
    [SerializeField] private string sceneToLoad;

    [Header("Efectos (Opcional)")]
    [SerializeField] private GameObject collectEffect;

    private bool _loading = false;

    // ── Detecta por Trigger (Is Trigger = true en el collider) ──
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CambiarDeEscena();
        }
    }

    // ── Detecta por Colisión normal (Is Trigger = false) ──
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CambiarDeEscena();
        }
    }

    private void CambiarDeEscena()
    {
        if (_loading) return; // Evitar doble carga
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("[SceneChangerGema] ¡No has puesto el nombre de la escena en el Inspector!");
            return;
        }

        _loading = true;

        // Restaurar timeScale por si la pantalla de muerte lo dejó en 0
        Time.timeScale = 1f;

        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        Debug.Log("[SceneChangerGema] Cambiando a la escena: " + sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
    }
}
