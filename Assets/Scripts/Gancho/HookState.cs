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
        hookSystem.HookVisual.SetLineVisible(true);
    }

    public override void Update()
    {
        hookSystem.HookVisual.UpdateAimLine();
        hookSystem.TargetFinder.FindTarget();
    }

    public override void Exit() { }
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
    public HookAttachedState(HookSystem system) : base(system) { }

    public override void Enter()
    {
        hookSystem.HookMovement.OnHookAttached();
        hookSystem.CurrentHookPoint?.OnHookAttach();
    }

    public override void Update()
    {
        hookSystem.HookVisual.UpdateHookLine();

        // 1) Actualizar el impulso (tirón rápido)
        hookSystem.HookMovement.UpdateImpulsePull();

        // 2) Si ya no hay impulso, arrastrar hacia el hook (si está activado en el SO)
        if (!hookSystem.HookMovement.IsImpulsing)
        {
            hookSystem.HookMovement.PullPlayerToHook();
        }
    }

    public override void Exit()
    {
        hookSystem.CurrentHookPoint?.OnHookDetach();
        hookSystem.PlayerRigidbody.useGravity = true;
    }
}