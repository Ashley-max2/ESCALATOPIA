using UnityEngine;

/// <summary>
/// Hook impulse state for the player.
/// Refactored to use component-based architecture.
/// Active while the hook is pulling/impulsing the player.
/// </summary>
public class PlayerHookImpulseState : StateBase<PlayerControllerRefactored>
{
    public override void Enter(PlayerControllerRefactored player)
    {
        base.Enter(player);
        
        // Disable gravity during hook impulse
        if (player.Movement.Rigidbody != null)
        {
            player.Movement.Rigidbody.useGravity = false;
        }

        Debug.Log("[HookImpulse] Entered hook impulse state");
    }

    public override void Update(PlayerControllerRefactored player)
    {
        // Check if hook is still active
        if (!player.IsBeingImpulsed && player.Hook != null && !player.Hook.IsHooking)
        {
            ExitHookImpulse(player);
            return;
        }

        // The hook system handles the physics during impulse
        // This state just maintains the player state while hooked
        
        // Optional: Add visual feedback or effects here
    }

    public override void Exit(PlayerControllerRefactored player)
    {
        base.Exit(player);

        // Re-enable gravity
        if (player.Movement.Rigidbody != null)
        {
            player.Movement.Rigidbody.useGravity = true;
        }

        Debug.Log("[HookImpulse] Exited hook impulse state");
    }

    private void ExitHookImpulse(PlayerControllerRefactored player)
    {
        // Transition based on current state
        if (player.Movement.IsGrounded)
        {
            if (player.Input.HasMovementInput)
            {
                player.StateMachine.ChangeState(new PlayerMovementState());
            }
            else
            {
                player.StateMachine.ChangeState(new PlayerIdleState());
            }
        }
        else
        {
            player.StateMachine.ChangeState(new PlayerJumpState());
        }
    }
}
