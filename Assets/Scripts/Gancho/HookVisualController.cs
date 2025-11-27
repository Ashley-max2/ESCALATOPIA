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

    public void UpdateAimLine()
    {
        if (hookSystem.TargetFinder.HasValidTarget)
        {
            hookLine.SetPosition(0, hookOrigin.position);
            hookLine.SetPosition(1, hookSystem.TargetFinder.CurrentTarget.HookPoint);
            reticle.ShowValidTarget();
        }
        else
        {
            reticle.HideReticle();
        }
    }

    public void UpdateHookLine()
    {
        if (hookSystem.CurrentHookPoint != null)
        {
            hookLine.SetPosition(0, hookOrigin.position);
            hookLine.SetPosition(1, hookSystem.CurrentHookPoint.HookPoint);
        }
    }

    public void SetLineVisible(bool visible) => hookLine.enabled = visible;
}