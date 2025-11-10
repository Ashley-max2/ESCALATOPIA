using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; // Referencia al Canvas del menú
    private bool isPaused = false;

    void Update()
    {
        // Detecta cuando se presiona la tecla Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Menu();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Reanuda el juego
        isPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Detiene el juego
        isPaused = true;
    }

    void Menu()
    {
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }
}
