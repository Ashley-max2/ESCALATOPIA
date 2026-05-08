using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathScreenManager : MonoBehaviour
{
    public Text causeText;

    /// <summary>
    /// Tiempo mínimo antes de permitir pulsar tecla (evita skips accidentales)
    /// </summary>
    [Tooltip("Segundos de espera antes de aceptar input para volver")]
    public float delayBeforeInput = 1f;

    private float _timer;
    private bool _loading;

    private void Start()
    {
        _timer = 0f;
        _loading = false;

        // Asegurar que el cursor esté visible en la pantalla de muerte
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (causeText != null)
        {
            switch (DeathManager.LastDeathCause)
            {
                case DeathCause.Stamina:
                    causeText.text = "Yo también necesito descansar";
                    break;
                case DeathCause.Fall:
                    causeText.text = "¿Tu saltarías de tan alto?";
                    break;
                case DeathCause.Water:
                    causeText.text = "Glu glu glu gl...";
                    break;
                default:
                    causeText.text = "Has muerto.";
                    break;
            }
        }
    }

    private void Update()
    {
        _timer += Time.unscaledDeltaTime;

        // Esperar un poco antes de aceptar input
        if (_timer < delayBeforeInput) return;

        // Al pulsar cualquier tecla, volver al último checkpoint
        if (Input.anyKeyDown && !_loading)
        {
            _loading = true;
            ReturnToLastCheckpoint();
        }
    }

    /// <summary>
    /// Carga la escena del último checkpoint guardado.
    /// Si no hay save, vuelve a Level_0 por defecto.
    /// El CheckpointManager se encarga de teleportar al jugador al spawn guardado.
    /// </summary>
    private void ReturnToLastCheckpoint()
    {
        string targetScene = "Level_0"; // Escena por defecto si no hay save

        if (GameProgressDatabase.HasSave())
        {
            GameProgressData save = GameProgressDatabase.Load();
            if (save != null && !string.IsNullOrWhiteSpace(save.Scene))
            {
                targetScene = save.Scene;
            }
        }

        Debug.Log($"[DeathScreen] Volviendo al checkpoint en escena: {targetScene}");
        SceneManager.LoadScene(targetScene);
    }
}