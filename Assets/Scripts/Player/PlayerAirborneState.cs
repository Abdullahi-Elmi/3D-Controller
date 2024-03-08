using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirborneState : PlayerBaseState
{
    public PlayerAirborneState(PlayerController currentContext, PlayerStateFactory stateFactory) : base(currentContext, stateFactory){
        IsRootState = true;
    }

    public override void EnterState(){
        InitializeSubState();
    }
    public override void UpdateState(){
        // if the player stopped holding jump while the player was in the air and still moving upwards, then set the _endedJumpEarly flag to true
        if(!Context.EndedJumpEarly && !Context.IsGrounded && !Context.JumpHeldInput && Context.FrameVelocity.y > 0){
            Context.EndedJumpEarly = true;
        }
        HandleGravity();
        CheckTransitions();
    }
    public override void ExitState(){}
    public override void CheckTransitions(){
        if(Context.IsGrounded){
            SwitchState(StateFactory.Grounded());
        }
        else if(Context.InCoyoteTime && Context.JumpToConsume){
            Debug.Log("Coyote time jump");
            SwitchState(StateFactory.Jump());
        }
        else if(Context.IsTouchingWall && Context.transform.position.y > Context.MinimumWallRunHeight && Context.MovementInput.y > 0f && Context.SprintHeldInput){
            SwitchState(StateFactory.WallRun());
        }
        Context.JumpToConsume = false;
    }
    public override void InitializeSubState(){
        if(Context.DashToConsume){
            SetSubState(StateFactory.Dash());
        }
        else{
            SetSubState(StateFactory.AirMovement());
        }
    }

    void HandleGravity(){
        Vector3 frameVelocity = Context.FrameVelocity;
        // Base gravity value to apply when the player is in the air
        float inAirGravity = Context.FallAcceleration;
        // Use a multiplier on the gravity value to make the player fall faster when they let go of the jump button early
        if(Context.EndedJumpEarly){
            inAirGravity *= Context.EndedJumpEarlyGravityModifier;
        }
        // Apply the gravity to the player's y velocity
        // Similar to calculating the gravity normally then clamping, this is more efficient and in 1 line.
        frameVelocity.y = Mathf.MoveTowards(frameVelocity.y, -Context.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime); 
        Context.FrameVelocity = frameVelocity;
    }
}