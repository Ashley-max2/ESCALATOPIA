using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Text3D : MonoBehaviour
{
    public Transform target; // Arrastra la Main Camera aquí

    void Update()
    {
        if (target == null) return;
        transform.LookAt(target);
        transform.Rotate(0, 180, 0); // Corrige la rotación
    }
}
