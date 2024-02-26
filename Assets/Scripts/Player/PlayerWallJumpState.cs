using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJumpState : PlayerBaseState
{
    public PlayerWallJumpState(PlayerController currentContext, PlayerStateFactory stateFactory) : base(currentContext, stateFactory){
        IsRootState = true;
    }

    public override void EnterState(){
        HandleWallJump();
    }
    public override void UpdateState(){
        CheckTransitions();
    }
    public override void ExitState(){}
    public override void CheckTransitions(){
        if(Context.IsTouchingWall && Context.transform.position.y > Context.MinimumWallRunHeight && Context.MovementInput.y > 0f && Context.SprintHeldInput)
            SwitchState(StateFactory.WallRun());
        else
            SwitchState(StateFactory.Airborne());
    }
    public override void InitializeSubState(){}

    void HandleWallJump(){
        // Turn jump related flags off to prevent further jumps until the player hits the ground (or wall) again
        Debug.Log("Wall Jump");
        Context.TimeJumpWasPressed = 0;
        Context.CanBufferJump = false;
        Context.JumpToConsume = false;
        Context.CanCoyoteTime = false;
        Context.EndedJumpEarly = false;
        // Set the y component of the player's velocity to the jump force (actually perform the jump)
        Vector3 frameVelocity = Context.FrameVelocity;
        frameVelocity = Vector3.up * Context.WallJumpUpForce + Context.WallNormal * Context.WallJumpOutForce;
        Context.FrameVelocity = frameVelocity;
    }
}
