using UnityEngine;

/// <summary>
/// Climbing state for the player.
/// Refactored to use PlayerClimbingComponent.
/// </summary>
public class PlayerClimbingState : StateBase<PlayerControllerRefactored>
{
    private float staminaConsumptionRate = 5f; // Stamina per second

    public override void Enter(PlayerControllerRefactored player)
    {
        base.Enter(player);
        
        // Disable gravity for climbing
        if (player.Movement.Rigidbody != null)
        {
            player.Movement.Rigidbody.useGravity = false;
        }
    }

    public override void Update(PlayerControllerRefactored player)
    {
        // Check if still in climbable zone
        if (!player.Climbing.IsInClimbableZone)
        {
            ExitClimbing(player);
            return;
        }

        // Check if climb button is still pressed
        if (!player.Input.Climb)
        {
            ExitClimbing(player);
            return;
        }

        // Check stamina
        if (player.Stamina.IsDepleted)
        {
            Debug.Log("[Climbing] Out of stamina!");
            ExitClimbing(player);
            return;
        }

        // Consume stamina
        float staminaToConsume = staminaConsumptionRate * Time.deltaTime;
        player.Stamina.ConsumeStamina(staminaToConsume);

        // Perform climbing movement
        Vector2 input = player.Input.MovementInput;
        Vector3 climbDirection = new Vector3(input.x, input.y, 0);
        
        // Get climb speed (could be configurable)
        float climbSpeed = 3f;
        player.Climbing.Climb(climbDirection, climbSpeed);

        // Check for wall jump
        if (player.Input.Jump)
        {
            player.StateMachine.ChangeState(new PlayerWallJumpState());
            return;
        }
    }

    public override void Exit(PlayerControllerRefactored player)
    {
        base.Exit(player);
        
        // Re-enable gravity
        player.Climbing.EnableGravity();
    }

    private void ExitClimbing(PlayerControllerRefactored player)
    {
        if (player.Movement.IsGrounded)
        {
            player.StateMachine.ChangeState(new PlayerIdleState());
        }
        else
        {
            player.StateMachine.ChangeState(new PlayerJumpState());
        }
    }
}
