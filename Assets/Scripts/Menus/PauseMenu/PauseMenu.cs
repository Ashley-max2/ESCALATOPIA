using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject menuDePausa;   // PADRE de todos los men�s
    public GameObject pauseMenuUI;   // Men� base
    public GameObject songMenuUI;    // Men� sonido
    public GameObject screenMenuUI;  // Men� pantalla

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Menu();
        }
    }

    public void Resume()
    {
        // Cerrar todo
        pauseMenuUI.SetActive(false);
        songMenuUI.SetActive(false);
        screenMenuUI.SetActive(false);
        menuDePausa.SetActive(false);  // Cerrar men� padre

        Time.timeScale = 1f;
        isPaused = false;
    }

    void Pause()
    {
        // Activar men� padre
        menuDePausa.SetActive(true);

        // Abrir men� base
        pauseMenuUI.SetActive(true);
        songMenuUI.SetActive(false);
        screenMenuUI.SetActive(false);

        Time.timeScale = 0f;
        isPaused = true;
    }

    void Menu()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    // Método público para el gamepad
    public void TogglePauseMenu()
    {
        Menu();
    }

    public void OpenSongMenu()
    {
        pauseMenuUI.SetActive(false);
        songMenuUI.SetActive(true);
        screenMenuUI.SetActive(false);
    }

    public void OpenScreenMenu()
    {
        pauseMenuUI.SetActive(false);
        songMenuUI.SetActive(false);
        screenMenuUI.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        pauseMenuUI.SetActive(true);
        songMenuUI.SetActive(false);
        screenMenuUI.SetActive(false);
    }
}
