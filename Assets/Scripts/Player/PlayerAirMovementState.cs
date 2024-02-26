using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirMovementState : PlayerBaseState
{
    private bool _wasSprintingOnGround = false;
    public PlayerAirMovementState(PlayerController currentContext, PlayerStateFactory stateFactory) : base(currentContext, stateFactory){}

    public override void EnterState(){
        // check if the player was sprinting before they entered the air by checking their horizontal velocity (xz plane) and comparing it to the sprint speed
        Vector3 horizontalVelocity = new Vector3(Context.FrameVelocity.x, 0, Context.FrameVelocity.z);
        _wasSprintingOnGround = horizontalVelocity.magnitude > Context.WalkSpeed;
    }
    public override void UpdateState(){
        AirMove();
        CheckTransitions();
    }
    public override void ExitState(){}
    public override void CheckTransitions(){}
    public override void InitializeSubState(){}
    void AirMove(){
        Vector3 frameVelocity = Context.FrameVelocity;
        Vector3 movementDirection = Context.GetMovementDirection();
        float movementSpeed = Context.WalkSpeed;
        float airMultiplier = Context.AirMultiplier;
        float horizontalAcceleration = Context.HorizontalAcceleration;

        if(_wasSprintingOnGround && Context.SprintHeldInput){
            // if the player was sprinting on the ground and still holding the sprint button, then apply sprint speed in the air
            movementSpeed = Context.SprintSpeed;
        }
        else if(Context.SprintHeldInput && !_wasSprintingOnGround){
            // if the player was not sprinting on the ground and is holding the sprint button we don't want to let them start sprinting in the air
            // so we set the sprinting on the ground flag to false
            _wasSprintingOnGround = false;
        }

        frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, movementDirection.x * movementSpeed * airMultiplier, horizontalAcceleration * Time.fixedDeltaTime);
        frameVelocity.z = Mathf.MoveTowards(frameVelocity.z, movementDirection.z * movementSpeed * airMultiplier, horizontalAcceleration * Time.fixedDeltaTime);
        Context.FrameVelocity = frameVelocity;
    }
}
