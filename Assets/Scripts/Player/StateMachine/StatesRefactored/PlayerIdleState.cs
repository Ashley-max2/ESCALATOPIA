using UnityEngine;

/// <summary>
/// Idle state for the player.
/// Refactored to use component-based architecture.
/// </summary>
public class PlayerIdleState : StateBase<PlayerControllerRefactored>
{
    public override void Update(PlayerControllerRefactored player)
    {
        // Check for climbing (highest priority)
        if (player.Climbing.CanStartClimbing() && player.Input.Climb)
        {
            player.StateMachine.ChangeState(new PlayerClimbingState());
            return;
        }

        // Check if not grounded (falling)
        if (!player.Movement.IsGrounded)
        {
            player.StateMachine.ChangeState(new PlayerJumpState());
            return;
        }

        // Check for movement input
        if (player.Input.HasMovementInput)
        {
            player.StateMachine.ChangeState(new PlayerMovementState());
            return;
        }

        // Check for jump
        if (player.Input.Jump)
        {
            player.StateMachine.ChangeState(new PlayerJumpState());
            return;
        }

        // Stay idle - apply minimal friction
        ApplyIdleFriction(player);
    }

    private void ApplyIdleFriction(PlayerControllerRefactored player)
    {
        // Gradually reduce velocity when idle
        Vector3 velocity = player.Movement.Velocity;
        velocity.x *= 0.9f;
        velocity.z *= 0.9f;
        player.Movement.Rigidbody.velocity = velocity;
    }
}
