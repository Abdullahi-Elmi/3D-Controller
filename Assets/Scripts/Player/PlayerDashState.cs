using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    private Vector3 _dashDirection;
    private float _dashTime;
    public PlayerDashState(PlayerController currentContext, PlayerStateFactory stateFactory) : base(currentContext, stateFactory){}

    public override void EnterState(){
        Context.DashToConsume = false;
        _dashTime = 0f;
        SetDashDirection();
    }
    public override void UpdateState(){
        Dash();
        CheckTransitions();
    }
    public override void ExitState(){
        Debug.Log("Exiting Dash State");
        Context.DashCooldownTimer = Context.DashCooldown;
    }
    public override void CheckTransitions(){
        // TODO: Decide if we want to allow jumping during the dash
        // If we've been in the dashing state for longer than dash duration, then switch back to another substate depending on the input
        if (_dashTime >= Context.DashDuration){
            SwitchState(StateFactory.AirMovement());
        }
        // otherwise, stay dashing and add to the time we've been in the state
        else{
            _dashTime += Time.deltaTime;
        }
    }
    public override void InitializeSubState(){}

    void SetDashDirection(){
        _dashDirection = Context.CameraTransform.forward;
        _dashDirection.y = 0f;
        _dashDirection.Normalize();
        Debug.Log("Dash Direction: "+ _dashDirection);
    }

    void Dash(){
        Context.FrameVelocity = _dashDirection * Context.DashSpeed;
    }
}
