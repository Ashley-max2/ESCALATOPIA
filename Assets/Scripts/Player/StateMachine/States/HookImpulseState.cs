using UnityEngine;

public class HookImpulseState : IState
{
    private ThirdPersonCameraController thirdPersonCam;
    private FirstPersonCameraController firstPersonCam;
    private CameraManager cameraManager;
    private Quaternion rotacionInicial;
    
    public void Enter(PlayerController player)
    {
        Debug.Log("Entrando en HookImpulseState - Bloqueando todos los movimientos");
        
        // Guardar rotación inicial para mantenerla durante el impulso
        rotacionInicial = player.transform.rotation;
        
        // Buscar los controladores de cámara
        cameraManager = Object.FindObjectOfType<CameraManager>();
        if (cameraManager != null)
        {
            // Desactivar temporalmente el control de cámara de tercera persona
            thirdPersonCam = cameraManager.camaraTerceraPersona?.GetComponent<ThirdPersonCameraController>();
            if (thirdPersonCam != null)
            {
                thirdPersonCam.enabled = false;
            }
            
            // Desactivar temporalmente el control de cámara de primera persona
            firstPersonCam = cameraManager.camaraPrimeraPersona?.GetComponent<FirstPersonCameraController>();
            if (firstPersonCam != null)
            {
                firstPersonCam.enabled = false;
            }
        }
        
        // NO congelar rotación para evitar que se caiga
        // En su lugar, mantendremos la rotación manualmente
        if (player.rb != null)
        {
            player.rb.angularVelocity = Vector3.zero;
        }
    }

    public void Update(PlayerController player)
    {
        // Mantener la rotación del jugador durante el impulso
        if (player != null && player.EstaGanchoActivo())
        {
            player.transform.rotation = rotacionInicial;
        }
        
        // Verificar si el gancho ya no está impulsando
        if (!player.EstaGanchoActivo())
        {
            Debug.Log("Gancho terminó de impulsar - Saliendo de HookImpulseState");
            
            // Transicionar según la situación
            if (player.PuedeIniciarEscalada())
            {
                // Si está cerca de una pared escalable, ir a ClimbingState
                player.CambiarEstado(new ClimbingState());
            }
            else if (player.EstaEnSuelo())
            {
                // Si cayó al suelo, ir a IdleState
                player.CambiarEstado(new IdleState());
            }
            else
            {
                // Si está en el aire, ir a JumpState (cayendo)
                player.CambiarEstado(new JumpState());
            }
        }
        
        // Durante el impulso, NO hacer nada más
        // Los inputs ya están bloqueados en PlayerController.LeerInputs()
    }

    public void Exit(PlayerController player)
    {
        Debug.Log("Saliendo de HookImpulseState");
        
        // Reactivar los controladores de cámara
        if (cameraManager != null)
        {
            if (thirdPersonCam != null)
            {
                thirdPersonCam.enabled = true;
            }
            if (firstPersonCam != null)
            {
                firstPersonCam.enabled = true;
            }
        }
        
        // Restaurar velocidad angular normal
        if (player.rb != null)
        {
            player.rb.angularVelocity = Vector3.zero;
        }
    }
}