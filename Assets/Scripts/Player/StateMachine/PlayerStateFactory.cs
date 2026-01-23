/// <summary>
/// Fábrica de estados del jugador.
/// Centraliza la creación de estados para facilitar la gestión y evitar duplicación.
/// Sigue el patrón Factory.
/// </summary>
public class PlayerStateFactory
{
    private PlayerStateMachine _context;
    
    // Cached states (pueden reutilizarse para evitar garbage collection)
    private PlayerGroundedState _groundedState;
    private PlayerAirborneState _airborneState;
    private PlayerJumpState _jumpState;
    private PlayerClimbState _climbState;
    private PlayerWallJumpState _wallJumpState;
    private PlayerHookState _hookState;
    private PlayerDeadState _deadState;
    
    public PlayerStateFactory(PlayerStateMachine context)
    {
        _context = context;
    }
    
    public PlayerGroundedState Grounded()
    {
        if (_groundedState == null)
            _groundedState = new PlayerGroundedState(_context, this);
        return _groundedState;
    }
    
    public PlayerAirborneState Airborne()
    {
        if (_airborneState == null)
            _airborneState = new PlayerAirborneState(_context, this);
        return _airborneState;
    }
    
    public PlayerJumpState Jump()
    {
        if (_jumpState == null)
            _jumpState = new PlayerJumpState(_context, this);
        return _jumpState;
    }
    
    public PlayerClimbState Climb()
    {
        if (_climbState == null)
            _climbState = new PlayerClimbState(_context, this);
        return _climbState;
    }
    
    public PlayerWallJumpState WallJump()
    {
        if (_wallJumpState == null)
            _wallJumpState = new PlayerWallJumpState(_context, this);
        return _wallJumpState;
    }
    
    public PlayerHookState Hook()
    {
        if (_hookState == null)
            _hookState = new PlayerHookState(_context, this);
        return _hookState;
    }
    
    public PlayerDeadState Dead()
    {
        if (_deadState == null)
            _deadState = new PlayerDeadState(_context, this);
        return _deadState;
    }
}
