using UnityEngine;

/// <summary>
/// Movement state for the player.
/// Refactored to use PlayerMovementComponent.
/// </summary>
public class PlayerMovementState : StateBase<PlayerControllerRefactored>
{
    public override void Update(PlayerControllerRefactored player)
    {
        // Check for climbing (highest priority)
        if (player.Climbing.CanStartClimbing() && player.Input.Climb)
        {
            player.StateMachine.ChangeState(new PlayerClimbingState());
            return;
        }

        // Check for jump
        if (player.Input.Jump && player.Movement.IsGrounded)
        {
            player.StateMachine.ChangeState(new PlayerJumpState());
            return;
        }

        // Check if stopped moving
        if (!player.Input.HasMovementInput)
        {
            player.StateMachine.ChangeState(new PlayerIdleState());
            return;
        }

        // Perform movement using component
        player.Movement.MoveRelativeToCamera(player.Input.MovementInput, player.Input.Run);
    }
}
