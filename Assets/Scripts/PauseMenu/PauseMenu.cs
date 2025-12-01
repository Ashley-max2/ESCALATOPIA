using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject menuDePausa;   // PADRE de todos los menús
    public GameObject pauseMenuUI;   // Menú base
    public GameObject songMenuUI;    // Menú sonido
    public GameObject screenMenuUI;  // Menú pantalla

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
        menuDePausa.SetActive(false);  // Cerrar menú padre

        Time.timeScale = 1f;
        isPaused = false;
    }

    void Pause()
    {
        // Activar menú padre
        menuDePausa.SetActive(true);

        // Abrir menú base
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
