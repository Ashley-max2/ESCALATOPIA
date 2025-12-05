using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookVisualController : MonoBehaviour
{
    [Header("Visual References")]
    public Transform hookOrigin;
    public LineRenderer hookLine;
    public HookReticle reticle;

    private HookSystem hookSystem;

    void Start()
    {
        hookSystem = GetComponent<HookSystem>();
        hookLine.positionCount = 2;
        SetLineVisible(false);
    }

    // Solo maneja la retícula durante el apuntado
    public void UpdateAimLine()
    {
        if (hookSystem.TargetFinder.HasValidTarget)
        {
            reticle.ShowValidTarget();
        }
        else
        {
            reticle.HideReticle();
        }
    }
       /* public void UpdateHookLine()
    {
        if (hookSystem.CurrentHookPoint != null)
        {
            hookLine.SetPosition(0, hookOrigin.position);
            hookLine.SetPosition(1, hookSystem.CurrentHookPoint.HookPoint);
        }
    }*/

    // Dibuja la línea del gancho cuando está lanzado o enganchado
    public void UpdateHookLine()
    {
        if (!hookLine.enabled) return;

        hookLine.SetPosition(0, hookOrigin.position);

        if (hookSystem.CurrentHookPoint != null)
        {
            hookLine.SetPosition(1, hookSystem.CurrentHookPoint.HookPoint);
        }
        else
        {
            // Mientras viaja y aún no hay CurrentHookPoint, usa el target del movimiento
            hookLine.SetPosition(1, hookSystem.HookMovement.GetCurrentTargetPosition());
        }
    }

    public void SetLineVisible(bool visible)
    {
        hookLine.enabled = visible;
    }
}