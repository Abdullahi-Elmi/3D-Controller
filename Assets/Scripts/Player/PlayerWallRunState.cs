using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// There's an issue on corners of the walls where the player gets stuck on the corner instead of exiting the wall run state.
// Might need to to either adjust the wall collision check or force the player to face along the wall (this could cause other issues though)

public class PlayerWallRunState : PlayerBaseState
{
    public PlayerWallRunState(PlayerController currentContext, PlayerStateFactory stateFactory) : base(currentContext, stateFactory){
        IsRootState = true;
    }

    public override void EnterState(){
        Debug.Log("Entered Wall Run");
        Context.CanBufferJump = true;
        Context.CanCoyoteTime = true;
        Context.EndedJumpEarly = false;
        // TODO: Tilt the camera away from the wall we're running on, and increase the FOV
    }
    public override void UpdateState(){
        WallRun();
        CheckTransitions();
    }
    public override void ExitState(){
        // TODO: Set camera back to normal
    }
    public override void CheckTransitions(){
        // Can also include buffer and coyote transitions here, but I'm not sure if that's necessary right now
        if(Context.JumpToConsume){
            SwitchState(StateFactory.WallJump());
        }
        else if(!Context.IsTouchingWall || Context.transform.position.y < Context.MinimumWallRunHeight || Context.MovementInput.y <= 0f || !Context.SprintHeldInput){
            SwitchState(StateFactory.Airborne());
        }
    }
    public override void InitializeSubState(){}

    void WallRun(){
        Vector3 frameVelocity = Context.FrameVelocity;
        Vector3 wallRunDirection = Vector3.Cross(Context.WallNormal, Vector3.up); // Calculate the direction along the wall

        // Adjust the direction based on the side of the wall
        if (Context.WallOnRightside)
        {
                wallRunDirection = -wallRunDirection;
        }

        frameVelocity = wallRunDirection * Context.SprintSpeed; // Modify the player's velocity to move along the wall
        frameVelocity.y = Context.WallRunGravity;
        Context.FrameVelocity = frameVelocity;
    }
}
