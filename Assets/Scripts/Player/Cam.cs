using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour
{
    public Transform objetivo;
    public float distancia = 4f;
    public float altura = 1.5f;
    public float sensibilidad = 2f;

    private float rotacionX = 0f;
    private float rotacionY = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        if (objetivo == null) return;

        // Input del mouse
        rotacionX += Input.GetAxis("Mouse X") * sensibilidad;
        rotacionY -= Input.GetAxis("Mouse Y") * sensibilidad;
        rotacionY = Mathf.Clamp(rotacionY, -30f, 70f);

        // Calcular posición y rotación
        Vector3 direccion = new Vector3(0, 0, -distancia);
        Quaternion rotacion = Quaternion.Euler(rotacionY, rotacionX, 0);

        transform.position = objetivo.position + Vector3.up * altura + rotacion * direccion;
        transform.LookAt(objetivo.position + Vector3.up * altura * 0.5f);
    }
}