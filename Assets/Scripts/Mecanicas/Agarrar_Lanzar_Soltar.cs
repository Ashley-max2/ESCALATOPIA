using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agarrar_Lanzar_Soltar : MonoBehaviour
{
    public void FollowPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject followPoint = GameObject.FindGameObjectWithTag("ObjectFollow");

        if (player != null && followPoint != null)
        {
            // Primero lo hacemos hijo del Player
            transform.SetParent(player.transform);

            // Luego copiamos posición y rotación del ObjectFollow
            transform.position = followPoint.transform.position;
        }
        else
        {
            Debug.LogWarning("No se encontró Player o ObjectFollow.");
        }
    }
}