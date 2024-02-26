using Unity.VisualScripting;
using UnityEngine;
public abstract class PlayerBaseState
{
    private bool _isRootState = false;
    private PlayerController _context;
    private PlayerStateFactory _stateFactory;
    private PlayerBaseState _currentSuperState;
    private PlayerBaseState _currentSubState;

    protected bool IsRootState { set { _isRootState = value; } }
    protected PlayerController Context { get { return _context; } }
    protected PlayerStateFactory StateFactory { get { return _stateFactory; } }
    public PlayerBaseState Substate => _currentSubState;
    public PlayerBaseState(PlayerController currentContext, PlayerStateFactory stateFactory){
        _context = currentContext;
        _stateFactory = stateFactory;
    }
    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public abstract void CheckTransitions();
    public abstract void InitializeSubState();

    public void UpdateStates(){
        UpdateState();
        if(_currentSubState != null){
            _currentSubState.UpdateStates();
        }
    }
    protected void SwitchState(PlayerBaseState newState){
        // current state exits first
        ExitState();
        // then we can enter the new state
        newState.EnterState();
        
        // Only update the current state of the context to the new state if the state we're in (this) is a root state
        if(_isRootState){
            // and update the current state of the context to the new state
            _context.CurrentState = newState;
        }
        else if(_currentSuperState != null){
            // if we're not a root state, then we need to update the current state of the super state to the new state
            _currentSuperState.SetSubState(newState);
        }
        
    }
    protected void SetSuperState(PlayerBaseState newSuperState){
        _currentSuperState = newSuperState;
    }
    protected void SetSubState(PlayerBaseState newSubState){
        if(_currentSubState != null){
            _currentSubState.ExitState();
        }
        newSubState.EnterState();
        _currentSubState = newSubState;
        newSubState.SetSuperState(this);
    }
}