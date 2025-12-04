using UnityEngine;

public class HookImpulseState : IState
{
    private ThirdPersonCameraController thirdPersonCam;
    private FirstPersonCameraController firstPersonCam;
    private CameraManager cameraManager;
    
    public void Enter(PlayerController player)
    {
        Debug.Log("Entrando en HookImpulseState - Bloqueando todos los movimientos");
        
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
        
        // Congelar rotación del Rigidbody durante el impulso para evitar que el personaje se voltee
        if (player.rb != null)
        {
            player.rb.freezeRotation = true;
            player.rb.angularVelocity = Vector3.zero;
        }
    }

    public void Update(PlayerController player)
    {
        // Verificar si el gancho ya no está impulsando
        if (!player.EstaGanchoActivo())
        {
            Debug.Log("Gancho terminó de impulsar - Saliendo de HookImpulseState");
            
            // IMPORTANTE: Asegurar que las cámaras se reactiven antes de cambiar de estado
            ReactivarCamaras();
            
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
        // La rotación está congelada en el Rigidbody y los inputs bloqueados en PlayerController.LeerInputs()
    }

    public void Exit(PlayerController player)
    {
        Debug.Log("Saliendo de HookImpulseState");
        
        // Asegurar que las cámaras estén reactivadas
        ReactivarCamaras();
        
        // Restaurar rotación normal y limpiar velocidad angular
        if (player.rb != null)
        {
            player.rb.freezeRotation = false;
            player.rb.angularVelocity = Vector3.zero;
        }
    }
    
    private void ReactivarCamaras()
    {
        if (cameraManager != null)
        {
            if (thirdPersonCam != null && !thirdPersonCam.enabled)
            {
                thirdPersonCam.enabled = true;
                Debug.Log("Cámara tercera persona reactivada");
            }
            if (firstPersonCam != null && !firstPersonCam.enabled)
            {
                firstPersonCam.enabled = true;
                Debug.Log("Cámara primera persona reactivada");
            }
        }
    }
}