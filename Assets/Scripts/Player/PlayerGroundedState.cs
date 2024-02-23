using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    public PlayerGroundedState(PlayerController currentContext, PlayerStateFactory stateFactory) : base(currentContext, stateFactory){
        IsRootState = true;
    }
    public override void EnterState(){
        // Landed on the ground this frame
        // Set jump related flags back on
        InitializeSubState();
        Context.CanBufferJump = true;
        Context.CanCoyoteTime = true;
        Context.EndedJumpEarly = false;
    }
    public override void UpdateState(){
        HandleGravity();
        CheckTransitions();
    }
    public override void ExitState(){
        // _timeLeftGround = Time.time;
        Context.TimeLeftGround = Time.time;

    }
    public override void CheckTransitions(){
        // How to check for jumping transition
        if(Context.JumpToConsume){
            Debug.Log("Regular Jump");
            SwitchState(StateFactory.Jump());
        }
        else if(Context.IsJumpBuffered){
            Debug.Log("Buffered Jump");
            SwitchState(StateFactory.Jump());
        }
        else if(!Context.IsGrounded){
            SwitchState(StateFactory.Airborne());
        }
        Context.JumpToConsume = false;
    }
    public override void InitializeSubState(){
        if(!Context.MovementPressedInput){
            SetSubState(StateFactory.Idle());
        }
        else{
            if(Context.SprintHeldInput){
                SetSubState(StateFactory.Sprint());
            }
            else{
                SetSubState(StateFactory.Walk());
            }
        }
    }

    void HandleGravity(){
        Vector3 frameVelocity = Context.FrameVelocity;
        if(frameVelocity.y < 0){
            frameVelocity.y = Context.GroundingForce;
            Context.FrameVelocity = frameVelocity;
        }
    }
}
