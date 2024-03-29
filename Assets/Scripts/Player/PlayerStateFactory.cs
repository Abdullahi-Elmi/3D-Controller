using System.Collections.Generic;

enum PlayerStates
{
    Grounded,
    Idle,
    Walk,
    Sprint,
    Airborne,
    AirMovement,
    Jump,
    Dash,
    WallRun,
    WallJump
    // Add more states here
}

public class PlayerStateFactory
{
    private PlayerController _context;
    Dictionary<PlayerStates, PlayerBaseState> _states = new Dictionary<PlayerStates, PlayerBaseState>();

    public PlayerStateFactory(PlayerController currentContext)
    {
        _context = currentContext;
        _states.Add(PlayerStates.Grounded, new PlayerGroundedState(_context, this));
        _states.Add(PlayerStates.Idle, new PlayerIdleState(_context, this));
        _states.Add(PlayerStates.Walk, new PlayerWalkState(_context, this));
        _states.Add(PlayerStates.Sprint, new PlayerSprintState(_context, this));
        _states.Add(PlayerStates.Airborne, new PlayerAirborneState(_context, this));
        _states.Add(PlayerStates.AirMovement, new PlayerAirMovementState(_context, this));
        _states.Add(PlayerStates.Jump, new PlayerJumpState(_context, this));
        _states.Add(PlayerStates.Dash, new PlayerDashState(_context, this));
        _states.Add(PlayerStates.WallRun, new PlayerWallRunState(_context, this));
        _states.Add(PlayerStates.WallJump, new PlayerWallJumpState(_context, this));
    }

    public PlayerBaseState Grounded()
    {
        return _states[PlayerStates.Grounded];
    }
    public PlayerBaseState Idle()
    {
        return _states[PlayerStates.Idle];
    }
    public PlayerBaseState Walk()
    {
        return _states[PlayerStates.Walk];
    }
    public PlayerBaseState Sprint()
    {
        return _states[PlayerStates.Sprint];
    }

    public PlayerBaseState Airborne()
    {
        return _states[PlayerStates.Airborne];
    }
    public PlayerBaseState AirMovement()
    {
        return _states[PlayerStates.AirMovement];
    }
    public PlayerBaseState Jump()
    {
        return _states[PlayerStates.Jump];
    }
    public PlayerBaseState Dash()
    {
        return _states[PlayerStates.Dash];
    }
    public PlayerBaseState WallRun()
    {
        return _states[PlayerStates.WallRun];
    }
    public PlayerBaseState WallJump()
    {
        return _states[PlayerStates.WallJump];
    }
}
