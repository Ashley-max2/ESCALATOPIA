using UnityEngine;

public class FirstPersonCameraController : MonoBehaviour
{
    [Header("Configuración")]
    public float sensibilidad = 2f;
    public float limiteVertical = 90f;

    private float rotacionX = 0f;
    private float rotacionY = 0f;
    private Transform objetivo;

    // Referencia al PlayerController para saber si está en gancho
    private PlayerController playerController;
    private HookSystem hookSystem;

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

        // Obtener referencias
        playerController = objetivo.GetComponent<PlayerController>();
        hookSystem = objetivo.GetComponentInChildren<HookSystem>();
        
        // Sincronizar rotación inicial con la cámara de tercera persona
        if (Camera.main != null)
        {
            rotacionX = Camera.main.transform.eulerAngles.y;
            rotacionY = Camera.main.transform.eulerAngles.x;
        }
    }

    void Update()
    {
        if (objetivo == null) return;

        // La cámara sigue la posición del jugador
        transform.position = objetivo.position + new Vector3(0f, 1.7f, 0f);

        // BLOQUEAR INPUT si el gancho está activo
        bool shouldBlockInput = false;
        
        if (playerController != null && playerController.EstaGanchoActivo())
        {
            shouldBlockInput = true;
        }
        
        if (hookSystem != null && hookSystem.HookMovement != null && hookSystem.HookMovement.IsImpulsing)
        {
            shouldBlockInput = true;
        }

        if (shouldBlockInput)
        {
            // Mantener la rotación actual, no procesar input
            return;
        }

        // Input del mouse (solo si no está en gancho)
        float mouseX = Input.GetAxis("Mouse X") * sensibilidad;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidad;

        rotacionX += mouseX;
        rotacionY -= mouseY;
        rotacionY = Mathf.Clamp(rotacionY, -limiteVertical, limiteVertical);

        // Aplicar rotación SOLO a la cámara
        transform.rotation = Quaternion.Euler(rotacionY, rotacionX, 0f);
    }

    // Método para sincronizar con la cámara de tercera persona
    public void SincronizarConTerceraPersona(ThirdPersonCameraController thirdPerson)
    {
        if (thirdPerson == null) return;
        
        // Obtener la rotación de tercera persona y aplicarla a primera persona
        Vector3 thirdPersonRotation = thirdPerson.transform.eulerAngles;
        rotacionX = thirdPersonRotation.y;
        rotacionY = thirdPersonRotation.x;
        
        Debug.Log($"[CAMERA] Sincronizada primera persona con tercera: X={rotacionY}, Y={rotacionX}");
    }
}