public class PlayerStateFactory
{
    private PlayerStateMachine _context;

    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
    }

    public PlayerState Grounded()
    {
        return new PlayerGroundedState(_context, this);
    }

    public PlayerState Jump()
    {
        return new PlayerJumpState(_context, this);
    }
    
    public PlayerState Air()
    {
        return new PlayerAirState(_context, this);
    }

    public PlayerState Climb()
    {
        return new PlayerClimbState(_context, this);
    }
    
    public PlayerState Hook()
    {
        return new PlayerHookState(_context, this);
    }
}
