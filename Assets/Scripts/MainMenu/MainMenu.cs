using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void ChangeScene(string newScene)
    {
        Debug.Log("Cargando escena: " + newScene);
        SceneManager.LoadScene(newScene);
    }

    public void ExitGame()
    {
        Debug.Log("Saliste");
        
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }
}
