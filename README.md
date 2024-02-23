# Unity 3D Character Controller
 
This is a Third Person 3D Rigidbody based character controller in Unity. This serves as a solid base and future test bed for all 3D character controller actions I want to create in the future.

A hierarchical state machine pattern is utilized to encapsulate the player actions. T

Currently the player character can:

- Walk
- Sprint
- Jump
- Move in the air

## Project Notes

The project uses both Unity's Cinemachine and Input System libraries. I'm using the input system in this project to learn how to handle things like rebinding inputs, using several input devices, and navigating UI with the input system.

## Coding Notes

### State Machine Pattern

Originally the code for the character controller was all in one script called PlayerMovement (now changed to OG) that is still in the project files. This quickly became far too bloated for me and harder to manage, once I started adding wall movement functionality so I refactored everything into a state machine pattern. PlayerController is the context of the state machine, the rest of the scripts are directly named after their function in the state machine. Currently, grounded/jumping/airborne are the root states with idle/walk/sprint being substates of grounded and airmovement being a substate of airborne.

### Physics Implementation

As mentioned, the character controller is rigidbody based. That being said, the movement code does not often use functions like AddForce(), but instead modifies the velocity of the rigidbody. To prevent all these separate movement functions from modifying the rigidbody's velocity directly several times in the same frame and possibly interfering with one another, a proxy _frameVelocity variable is utilized that actually stores the change each movement state makes and applies it to the actual rb.velocity at the very end of FixedUpdate.

## Movement Actions

### Grounded Movement

This is pretty simple and straightforward. There is still a lower gravity force being applied to the player when grounded to help when moving on slopes in the future.

### Jumping

In order to emulate the jumping of platformer games (often made in 2D), a lot of platformer related jumping logic has been implemented into this controller.

- Variable Jump Height - The height of the player's jump is affected by how long the player holds the jump input. If they simply tap the button, they'll only do a short hop, and if they hold it the entire time, they'll have a much larger jump. This is to give players more control over their jump height.
- Buffered Jump Input - There is a grace period in which the player can press jump even if they aren't grounded, and the controller will buffer (remember) this input and perform the jump when it hits the ground. This is to have the controller feel more responsive, so that players do not have to be frame perfect on waiting to be grounded before jumping again immediately.
- Coyote Time - If the player steps off of a surface, there is a grace period allowing the player to still jump even though they aren't grounded. This is for similar reasons as buffered jumps, to make the controller feel more responsive and not require players to be frame perfect
