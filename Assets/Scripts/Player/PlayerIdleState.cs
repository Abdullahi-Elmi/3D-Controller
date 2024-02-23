using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerController currentContext, PlayerStateFactory stateFactory) : base(currentContext, stateFactory){}

    public override void EnterState(){}
    public override void UpdateState(){
        Decelerate();
        CheckTransitions();
    }
    public override void ExitState(){}
    public override void CheckTransitions(){
        if(Context.MovementPressedInput){
            if(Context.SprintHeldInput){
                SwitchState(StateFactory.Sprint());
            } 
            else{
                SwitchState(StateFactory.Walk());
            }
        }
    }
    public override void InitializeSubState(){}
    void Decelerate(){
        Vector3 frameVelocity = Context.FrameVelocity;
        float deceleration = Context.GroundDeceleration;
        frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        frameVelocity.z = Mathf.MoveTowards(frameVelocity.z, 0, deceleration * Time.fixedDeltaTime);
        Context.FrameVelocity = frameVelocity;
    }
}