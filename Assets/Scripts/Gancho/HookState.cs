using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HookState
{
    protected HookSystem hookSystem;

    public HookState(HookSystem system) => hookSystem = system;
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}

public class HookIdleState : HookState
{
    public HookIdleState(HookSystem system) : base(system) { }

    public override void Enter()
{
    hookSystem.HookVisual.SetLineVisible(false);
    hookSystem.PlayerRigidbody.useGravity = true;
}
    public override void Update() => hookSystem.TargetFinder.FindTarget();
    public override void Exit() { }
}

public class HookAimingState : HookState
{
    public HookAimingState(HookSystem system) : base(system) { }

    public override void Enter()
    {
        // YA NO encendemos la línea aquí
        hookSystem.HookVisual.SetLineVisible(false);
    }

    public override void Update()
    {
        // Solo usamos la retícula (HookReticle) para mostrar si hay target
        hookSystem.HookVisual.UpdateAimLine();
        hookSystem.TargetFinder.FindTarget();
    }

    public override void Exit()
    {
        // Al salir del aiming, ocultamos la retícula
        hookSystem.HookVisual.reticle.HideReticle();
    }
}

public class HookThrownState : HookState
{
    public HookThrownState(HookSystem system) : base(system) { }

    public override void Enter()
    {
        hookSystem.HookMovement.LaunchHook();
        hookSystem.HookVisual.SetLineVisible(true);
    }

    public override void Update()
    {
        hookSystem.HookMovement.UpdateHookTravel();
        hookSystem.HookVisual.UpdateHookLine();

        if (hookSystem.HookMovement.HasReachedTarget())
            hookSystem.ChangeState(new HookAttachedState(hookSystem));
    }

    public override void Exit() { }
}

public class HookAttachedState : HookState
{
    private float maxAttachmentTime = 3f; // Máximo 3 segundos enganchado
    private float attachmentTimer;
    
    public HookAttachedState(HookSystem system) : base(system) { }

    public override void Enter()
    {
        attachmentTimer = maxAttachmentTime;
        hookSystem.HookMovement.OnHookAttached();
        hookSystem.CurrentHookPoint?.OnHookAttach();
    }

    public override void Update()
    {
        hookSystem.HookVisual.UpdateHookLine();

        // 1) Actualizar el impulso (tirón rápido)
        hookSystem.HookMovement.UpdateImpulsePull();
        
        // 2) Timer de seguridad para prevenir enganche permanente
        attachmentTimer -= Time.deltaTime;
        if (attachmentTimer <= 0f)
        {
            Debug.Log("[HOOK] Timer de seguridad agotado, cancelando gancho");
            hookSystem.CancelHook();
            return;
        }

        // 3) Si ya no hay impulso, cancelar el gancho
        if (!hookSystem.HookMovement.IsImpulsing)
        {
            Debug.Log("[HOOK] IsImpulsing = false, cancelando gancho desde HookAttachedState");
            hookSystem.CancelHook();
        }
    }

    public override void Exit()
    {
        hookSystem.CurrentHookPoint?.OnHookDetach();
        hookSystem.PlayerRigidbody.useGravity = true;
        hookSystem.PlayerRigidbody.freezeRotation = true;
    }
}