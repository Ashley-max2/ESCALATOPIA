using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookInputController : MonoBehaviour
{
public void ProcessInput(HookSystem hookSystem)
{
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

    // Cancelar gancho en vuelo o enganchado
    if (Input.GetKeyDown(KeyCode.Space))
    {
        hookSystem.CancelHook();
        hookSystem.HookMovement.CancelHook();
    }
}

}