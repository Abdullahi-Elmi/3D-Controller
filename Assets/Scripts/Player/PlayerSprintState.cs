using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSprintState : PlayerBaseState
{
    public PlayerSprintState(PlayerController currentContext, PlayerStateFactory stateFactory) : base(currentContext, stateFactory){}

    public override void EnterState(){}
    public override void UpdateState(){
        Sprint();
        CheckTransitions();
    }
    public override void ExitState(){}
    public override void CheckTransitions(){
        if(Context.DashToConsume){
            SwitchState(StateFactory.Dash());
        }
        else if(!Context.MovementPressedInput){
            SwitchState(StateFactory.Idle());
        }
        else if(!Context.SprintHeldInput){
            SwitchState(StateFactory.Walk());
        }
    }
    public override void InitializeSubState(){}

    void Sprint(){
        Vector3 frameVelocity = Context.FrameVelocity;
        Vector3 movementDirection = Context.GetMovementDirection();
        frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, movementDirection.x * Context.SprintSpeed, Context.HorizontalAcceleration * Time.fixedDeltaTime);
        frameVelocity.z = Mathf.MoveTowards(frameVelocity.z, movementDirection.z * Context.SprintSpeed, Context.HorizontalAcceleration * Time.fixedDeltaTime);
        Context.FrameVelocity = frameVelocity;
    }
}