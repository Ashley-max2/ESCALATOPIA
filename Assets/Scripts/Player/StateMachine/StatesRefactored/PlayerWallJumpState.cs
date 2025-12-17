using UnityEngine;

/// <summary>
/// Wall jump state for the player.
/// Refactored to use component-based architecture.
/// </summary>
public class PlayerWallJumpState : StateBase<PlayerControllerRefactored>
{
    private float wallJumpDuration = 0.15f;
    private float elapsedTime = 0f;

    public override void Enter(PlayerControllerRefactored player)
    {
        base.Enter(player);

        // Get wall normal from climbing component
        Vector3 wallNormal = player.Climbing.ClimbingSurfaceNormal;

        // Apply wall jump force
        float upwardForce = 12f;
        float lateralForce = 8f;
        player.Movement.ApplyWallJumpForce(upwardForce, lateralForce, wallNormal);

        // Start reentry delay for climbing
        player.Climbing.StartReentryDelay();

        // Re-enable gravity
        player.Climbing.EnableGravity();

        elapsedTime = 0f;
    }

    public override void Update(PlayerControllerRefactored player)
    {
        elapsedTime += Time.deltaTime;

        // After wall jump duration, transition to jump state
        if (elapsedTime >= wallJumpDuration)
        {
            player.StateMachine.ChangeState(new PlayerJumpState());
            return;
        }

        // Limited air control during wall jump
        if (player.Input.HasMovementInput)
        {
            ApplyLimitedAirControl(player);
        }
    }

    private void ApplyLimitedAirControl(PlayerControllerRefactored player)
    {
        // Very limited air control during wall jump
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector2 input = player.Input.MovementInput;
        Vector3 direction = (cameraRight * input.x + cameraForward * input.y).normalized;

        // Apply very reduced air control force
        player.Movement.Rigidbody.AddForce(direction * 2f, ForceMode.Acceleration);
    }
}
