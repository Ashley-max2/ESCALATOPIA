using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookInputController : MonoBehaviour
{
    public void ProcessInput(HookSystem hookSystem)
    {
        // Durante el impulso del gancho, ignorar cualquier entrada (incluido Space)
        if (hookSystem.HookMovement != null && hookSystem.HookMovement.IsImpulsing)
        {
            return;
        }
        
        // Mientras mantengo click derecho → Aiming
        if (Input.GetMouseButton(1) && !hookSystem.IsHooking)
        {
            hookSystem.ChangeState(new HookAimingState(hookSystem));
        }

        // Soltar click derecho → cancelar aiming
        if (Input.GetMouseButtonUp(1))
        {
            hookSystem.ChangeState(new HookIdleState(hookSystem));
        }

        // Lanzar gancho con click izquierdo
        if (Input.GetMouseButtonDown(0) && hookSystem.TargetFinder.HasValidTarget)
        {
            hookSystem.LaunchHook();
        }

        // Cancelar gancho en vuelo o enganchado (SOLO si no está impulsando)
        if (Input.GetKeyDown(KeyCode.Space) && !hookSystem.HookMovement.IsImpulsing)
        {
            hookSystem.CancelHook();
            hookSystem.HookMovement.CancelHook();
        }
    }

}