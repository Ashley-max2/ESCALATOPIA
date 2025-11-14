using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallDetector : MonoBehaviour
{
    public bool playerLive = true;
    public float startY;
    public float maxFall = 10f;

    void Start()
    {
        startY = transform.position.y;
    }

    void OnCollisionEnter(Collision collision)
    {
        float fallDistance = startY - transform.position.y;

        if (fallDistance > maxFall)
        {
            //Si cae más de 10 metros
            playerLive = false;
            Object.Destroy(gameObject);
            Debug.Log("El jugador ha caído más de 10 metros, muerte.");
        }
        else
        {
            //Si cae menos o igual de 10 metros
            startY = transform.position.y;
            Debug.Log("Caída segura, jugador sigue vivo.");
        }
    }

}
