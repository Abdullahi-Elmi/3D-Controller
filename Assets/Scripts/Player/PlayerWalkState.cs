using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    public PlayerWalkState(PlayerController currentContext, PlayerStateFactory stateFactory) : base(currentContext, stateFactory){}

    public override void EnterState(){}
    public override void UpdateState(){
        Walk();
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
        else if(Context.SprintHeldInput){
            SwitchState(StateFactory.Sprint());
        }
    }
    public override void InitializeSubState(){}

    void Walk(){
        Vector3 frameVelocity = Context.FrameVelocity;
        Vector3 movementDirection = Context.GetMovementDirection();
        frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, movementDirection.x * Context.WalkSpeed, Context.HorizontalAcceleration * Time.fixedDeltaTime);
        frameVelocity.z = Mathf.MoveTowards(frameVelocity.z, movementDirection.z * Context.WalkSpeed, Context.HorizontalAcceleration * Time.fixedDeltaTime);
        Context.FrameVelocity = frameVelocity;
    }
}