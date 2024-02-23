using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    public PlayerJumpState(PlayerController currentContext, PlayerStateFactory stateFactory) : base(currentContext, stateFactory){
        IsRootState = true;
    }

    public override void EnterState(){
        HandleJump();
    }
    public override void UpdateState(){
        CheckTransitions();
    }
    public override void ExitState(){}
    public override void CheckTransitions(){
        // I'm not sure this ever happens
        if(Context.IsGrounded){
            SwitchState(StateFactory.Grounded());
        }
        // Once we've executed the jump (in EnterState()), we're in the air and we can switch to the airborne state
        else{
            SwitchState(StateFactory.Airborne());
        }
    }
    public override void InitializeSubState(){}

    void HandleJump(){
        // Turn jump related flags off to prevent further jumps until the player hits the ground again
        Context.TimeJumpWasPressed = 0;
        Context.CanBufferJump = false;
        Context.JumpToConsume = false;
        Context.CanCoyoteTime = false;
        Context.EndedJumpEarly = false;
        // Set the y component of the player's velocity to the jump force (actually perform the jump)
        Vector3 frameVelocity = Context.FrameVelocity;
        frameVelocity.y = Context.JumpForce;
        Context.FrameVelocity = frameVelocity;
    }
}
