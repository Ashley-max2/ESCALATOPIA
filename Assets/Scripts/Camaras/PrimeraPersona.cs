using UnityEngine;

public class FirstPersonCameraController : MonoBehaviour
{
    [Header("Configuración")]
    public float sensibilidad = 2f;
    public float limiteVertical = 90f;

    private float rotacionX = 0f;
    private float rotacionY = 0f;
    private Transform objetivo;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        // El objetivo es el padre (el jugador)
        objetivo = transform.parent;
        if (objetivo == null)
        {
            Debug.LogError("La cámara de 1ra persona debe ser hija del jugador");
        }

        // Posicionar la cámara a la altura de los ojos
        transform.localPosition = new Vector3(0f, 1.7f, 0f);
    }

    void Update()
    {
        if (objetivo == null) return;

        // La cámara sigue la posición del jugador
        transform.position = objetivo.position + new Vector3(0f, 1.7f, 0f);

        // Input del mouse
        float mouseX = Input.GetAxis("Mouse X") * sensibilidad;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidad;

        rotacionX += mouseX;
        rotacionY -= mouseY;
        rotacionY = Mathf.Clamp(rotacionY, -limiteVertical, limiteVertical);

        // Aplicar rotación SOLO a la cámara
        transform.rotation = Quaternion.Euler(rotacionY, rotacionX, 0f);
    }
}