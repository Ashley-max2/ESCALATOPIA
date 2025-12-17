using UnityEngine;

/// <summary>
/// Jump/Air state for the player.
/// Refactored to use component-based architecture.
/// </summary>
public class PlayerJumpState : StateBase<PlayerControllerRefactored>
{
    private bool hasJumped = false;

    public override void Enter(PlayerControllerRefactored player)
    {
        base.Enter(player);

        // Apply jump force if grounded and jump was pressed
        if (player.Movement.IsGrounded && player.Input.Jump)
        {
            // Get jump force from player settings (we'll need to add this)
            float jumpForce = 12f; // Default value
            player.Movement.ApplyJumpForce(jumpForce);
            hasJumped = true;
        }
    }

    public override void Update(PlayerControllerRefactored player)
    {
        // Check if landed
        if (player.Movement.IsGrounded && player.Movement.Velocity.y <= 0.1f)
        {
            // Transition based on input
            if (player.Input.HasMovementInput)
            {
                player.StateMachine.ChangeState(new PlayerMovementState());
            }
            else
            {
                player.StateMachine.ChangeState(new PlayerIdleState());
            }
            return;
        }

        // Allow air control (limited movement while in air)
        if (player.Input.HasMovementInput)
        {
            ApplyAirControl(player);
        }
    }

    private void ApplyAirControl(PlayerControllerRefactored player)
    {
        // Limited air control
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector2 input = player.Input.MovementInput;
        Vector3 direction = (cameraRight * input.x + cameraForward * input.y).normalized;

        // Apply reduced air control force
        player.Movement.Rigidbody.AddForce(direction * 5f, ForceMode.Acceleration);
    }

    public override void Exit(PlayerControllerRefactored player)
    {
        base.Exit(player);
        hasJumped = false;
    }
}
