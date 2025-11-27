using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookInputController : MonoBehaviour
{
    public void ProcessInput(HookSystem hookSystem)
    {
        // Lanzar gancho
        if (Input.GetMouseButtonDown(0) && hookSystem.TargetFinder.HasValidTarget && !hookSystem.IsHooking)
        {
            hookSystem.LaunchHook();
        }

        // Cancelar gancho
        if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Space)) && hookSystem.IsHooking)
        {
            hookSystem.CancelHook();
            hookSystem.HookMovement.CancelHook();
        }
    }
}