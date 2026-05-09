using UnityEngine;

/// <summary>
/// Estado de salto desde pared (Wall Jump).
/// Salta en la dirección WASD relativa a la pared:
///   W = hacia fuera de la pared (normal) + arriba
///   S = hacia abajo
///   A = izquierda en la pared
///   D = derecha en la pared
/// Sin input = salta hacia atrás (alejándose de la pared).
/// El player NO se gira durante el wall jump.
/// </summary>
public class PlayerWallJumpState : PlayerBaseState
{
    private bool _jumpApplied;
    private float _wallJumpTimer;
    private const float WALL_JUMP_LOCK_DURATION = 0.2f;
    
    public PlayerWallJumpState(PlayerStateMachine context, PlayerStateFactory factory) 
        : base(context, factory) { }
    
    public override void Enter()
    {
        _jumpApplied = false;
        _wallJumpTimer = 0;
        
        // Consumir stamina por wall jump
        if (ctx.Stamina != null)
        {
            ctx.Stamina.ConsumeStamina(ctx.ClimbStaminaCost * 2f);
        }
    }
    
    public override void Execute()
    {
        _wallJumpTimer += Time.deltaTime;
        
        if (_jumpApplied && _wallJumpTimer >= WALL_JUMP_LOCK_DURATION)
        {
            SwitchState(factory.Airborne());
        }
    }
    
    public override void FixedExecute()
    {
        if (!_jumpApplied)
        {
            ApplyWallJump();
            _jumpApplied = true;
        }
    }
    
    public override void Exit()
    {
    }
    
    private void ApplyWallJump()
    {
        ctx.Rb.velocity = Vector3.zero;
        
        // Ejes de la pared (misma lógica que PlayerClimbState)
        Vector3 wallNormal = ctx.WallNormal.normalized;
        Vector3 wallRight = Vector3.Cross(wallNormal, Vector3.up).normalized;
        Vector3 wallUp = Vector3.up;
        
        // Input CRUDO del teclado (WASD)
        float horizontal = ctx.Input.MoveX; // A (-1) / D (+1)
        float vertical = ctx.Input.MoveZ;   // S (-1) / W (+1)
        
        Vector3 jumpDirection;
        
        if (ctx.Input.HasMovementInput)
        {
            // Calcular dirección basada en WASD relativo a la pared
            // horizontal (A/D) = moverse a lo largo de la pared
            // vertical (W) = saltar hacia fuera de la pared + arriba
            // vertical (S) = ir hacia abajo
            Vector3 dir = Vector3.zero;
            
            // Componente lateral (A/D mueven a lo largo de la pared)
            dir += wallRight * horizontal;
            
            // Componente vertical/normal
            if (vertical > 0.1f)
            {
                // W = saltar hacia fuera de la pared + arriba
                dir += wallNormal * 1f;
            }
            else if (vertical < -0.1f)
            {
                // S = saltar ligeramente hacia fuera + abajo
                dir += wallNormal * 0.3f;
                dir += Vector3.down * 0.3f;
            }
            
            // Si solo hay input lateral (A o D sin W/S), añadir componente normal para separarse
            if (Mathf.Abs(horizontal) > 0.1f && Mathf.Abs(vertical) < 0.1f)
            {
                dir += wallNormal * 0.5f;
            }
            
            dir = dir.normalized;
            
            // Aplicar fuerzas
            jumpDirection = dir * ctx.WallJumpForce + wallUp * ctx.WallJumpUpwardForce;
        }
        else
        {
            // Sin input = saltar hacia atrás (alejándose de la pared)
            jumpDirection = wallNormal * ctx.WallJumpForce + wallUp * ctx.WallJumpUpwardForce;
        }
        
        ctx.Rb.AddForce(jumpDirection, ForceMode.Impulse);
        
        // NO rotar al player - se queda mirando en la misma dirección
        
        ctx.FallStartHeight = ctx.transform.position.y;
        
        Debug.Log("Wall Jump!");
    }
}
