using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeadMenuManager : MonoBehaviour
{
    public void ChangeScene(string newScene)
    {
        Debug.Log("Cargando escena: " + newScene);
        SceneManager.LoadScene(newScene);
    }

    public void a()
    {
        Debug.LogError("a");
    }
}
