using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OG : MonoBehaviour
{
    private Rigidbody _rb;
    private CapsuleCollider _capsuleCollider;
    private Vector3 _frameVelocity; // Buffer variable to calculate the player's velocity all at once before then setting the _rb.velocity to this
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Transform _topOfBuildingSpawn;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        StatedHandler();
    }

    void FixedUpdate()
    {
        HandleTimers();
        CheckCollisions();
        RotatePlayer();
        
        if(IsWallRunning){
            Debug.Log("We're wall Running!");
            HandleWallRunning();
            HandleWallJump();
        }
        else{
            Move();
            HandleJump();
            HandleDash();
            HandleGravity();
        }
        _rb.velocity = _frameVelocity;
    }

    private void HandleTimers()
    {
        if (_dashCooldownTimer >= 0)
        {
            _dashCooldownTimer -= Time.fixedDeltaTime;
        }
    }

    #region Inputs
    private float _horizontalInput;
    private float _verticalInput;
    private bool _jumpHeldInput;
    private bool _sprintHeldInput;
    private bool _dashToConsume;
    private void HandleInput()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        bool jumpPressed = Input.GetButtonDown("Jump");
        _jumpHeldInput = Input.GetButton("Jump");
        _sprintHeldInput = Input.GetKey(KeyCode.LeftShift);

        if (jumpPressed)
        {
            _jumpToConsume = true; // This is set to true until the player actually jumps in the Jump() method. Prevents input from getting lost between physics and update frames
            _timeJumpWasPressed = Time.time;
        }

        bool dashPressed = Input.GetKeyDown(KeyCode.E);
        if (dashPressed && _dashCooldownTimer <= 0) _dashToConsume = true;
    }
    #endregion

    #region Collisions Check
    [Header("Collisions Check")]
    [SerializeField] private LayerMask _playerLayer; // Set this to the layer(s) which your ground is on
    [SerializeField] private float _groundCheckDistance = 0.1f; // How far to check for the ground
    [SerializeField] private float _capsuleOriginOffset = 0.01f; // Offset capsule origin - prevent sphere from intersecting the ground when the capsule is on it
    private bool _isGrounded;
    private void CheckCollisions()
    {
        // Calculate the positions of the top and bottom of the capsule
        Vector3 capsuleBottom = transform.position + Vector3.down * (_capsuleCollider.height / 2 - _capsuleCollider.radius);
        Vector3 capsuleTop = transform.position + Vector3.up * (_capsuleCollider.height / 2 - _capsuleCollider.radius);

        // Add a small upward offset to the bottom of the capsule
        capsuleBottom += Vector3.up * _capsuleOriginOffset;
        // Add a small downward offset to the top of the capsule
        capsuleTop += Vector3.down * _capsuleOriginOffset;

        // CapsuleCast to check if the player hit a ceiling (collider above it)
        bool ceilingHit = Physics.CapsuleCast(capsuleBottom, capsuleTop, _capsuleCollider.radius, Vector3.up, out RaycastHit ceilingHitInfo, _groundCheckDistance, ~_playerLayer);
        if(ceilingHit){
            _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);
        }

        // CapsuleCast to check if the player is grounded
        bool groundHit = Physics.CapsuleCast(capsuleBottom, capsuleTop, _capsuleCollider.radius, Vector3.down, out RaycastHit groundHitInfo, _groundCheckDistance, ~_playerLayer);
        
        // If the player hit the ground this frame (meaning they were in the air last frame) then update the _isGrounded variable and get the time the player hit the ground
        // Landed on the ground this frame
        if(!_isGrounded && groundHit){
            _isGrounded = true;
            // Set jump related flags back on
            _canBufferJump = true;
            _canCoyoteTime = true;
            _endedJumpEarly = false;
        }
        // If the player did not hit the ground this frame but were grounded last frame then update the _isGrounded variable
        // Left the ground this frame
        else if(_isGrounded && !groundHit){
            _isGrounded = false;
            _timeLeftGround = Time.time;
        }
        CheckWallCollisions();
    }
    #endregion

    #region Movement
    [Header("Movement")]
    
    private float _movementSpeed = 10f; // How fast the player should move horizontally (xz plane)
    [SerializeField] private float _walkSpeed = 10f; // Player's _movementSpeed when walking (base speed)
    [SerializeField] private float _sprintSpeed = 15f; // Player's _movementSpeed when sprinting
    [SerializeField] private float _rotationSpeed = 7.5f; // How fast the player should rotate to face the direction they want to move in
    [SerializeField] private float _airMultiplier = 0.8f; // How much slower the player should move in the air
    [SerializeField] private float _acceleration = 120f; // How fast the player should accelerate horizontally
    [SerializeField] private float _groundDeceleration = 60f; // How fast the player should stop when on the ground
    [SerializeField] private float _airDeceleration = 30f; // How fast the player should stop when in the air
    private void Move()
    {
        // If the player is not inputting any movement, set the player's velocity to 0 to stop them from sliding
        if(_horizontalInput == 0 && _verticalInput == 0){
            float deceleration = _isGrounded ? _groundDeceleration : _airDeceleration;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            _frameVelocity.z = Mathf.MoveTowards(_frameVelocity.z, 0, deceleration * Time.fixedDeltaTime);
            return;
        }

        // Get the camera's forward (z-axis) and right (x-axis) vectors
        Vector3 cameraForward = _mainCamera.transform.forward;
        Vector3 cameraRight = _mainCamera.transform.right;
        
        // Caclulate the direction the player should move in by adding the camera's forward and right vectors so that the player's inputs are relative to the camera's orientation
        Vector3 movementDirection = cameraForward * _verticalInput + cameraRight * _horizontalInput;
        
        // Set the y component to 0 to prevent the player from moving up or down
        movementDirection.y = 0;

        // Normalize the vector to prevent the player from moving faster when moving diagonally
        movementDirection.Normalize();

        if(_isGrounded){
            // Move the player in the direction of the movement vector
            // _frameVelocity.x = movementDirection.x * _movementSpeed;
            // _frameVelocity.z = movementDirection.z * _movementSpeed;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, movementDirection.x * _movementSpeed, _acceleration * Time.fixedDeltaTime);
            _frameVelocity.z = Mathf.MoveTowards(_frameVelocity.z, movementDirection.z * _movementSpeed, _acceleration * Time.fixedDeltaTime);
        }
        else{
            // If the player is not grounded multiply the movement speed by an air multiplier to make the player move slower in the air
            // _frameVelocity.x = movementDirection.x * _movementSpeed * _airMultiplier;
            // _frameVelocity.z = movementDirection.z * _movementSpeed * _airMultiplier;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, movementDirection.x * _movementSpeed * _airMultiplier, _acceleration * Time.fixedDeltaTime);
            _frameVelocity.z = Mathf.MoveTowards(_frameVelocity.z, movementDirection.z * _movementSpeed * _airMultiplier, _acceleration * Time.fixedDeltaTime);
        }
    }
    
    private void RotatePlayer()
    {
        // These 5 lines are the same as in the Move() method
        Vector3 cameraForward = _mainCamera.transform.forward;
        Vector3 cameraRight = _mainCamera.transform.right;
        Vector3 inputDirection = cameraForward * _verticalInput + cameraRight * _horizontalInput;
        inputDirection.y = 0;
        inputDirection.Normalize();

        // If the player is moving, rotate the player to face the direction the player wnats to move in
        if (inputDirection != Vector3.zero)
            transform.forward = Vector3.Slerp(transform.forward, inputDirection, _rotationSpeed * Time.fixedDeltaTime);
    }
    #endregion

    #region Jump
    [Header("Jumping")]
    [SerializeField] private float _jumpForce = 36f; // How much force to apply when jumping
    [SerializeField] private float _jumpBufferTime = 0.2f; // How long before the player hits the ground where their jump input is accepted and buffered.
    [SerializeField] private float _coyoteTime = 0.2f; // How long after the player leaves the ground where they can still jump
    private bool _jumpToConsume; // Becomes true when jump button is pressed, and remains so until the player actually jumps in the Jump() method. Prevents input from getting lost between physics and update frames.
    private bool _endedJumpEarly; // Flag to check if the player let go of the jump button early to cut their jump short

    private bool _canBufferJump; // Flag controlling whether a buffered jump can be executed or not. True when player lands, false when they execute a jump. (Should only be able to buffer a jump once in the air).
    private float _timeJumpWasPressed = float.MinValue; // Keeps track of the time the player pressed the jump button to compare with the current time to see if the jump input was buffered.
    private bool IsJumpBuffered => _canBufferJump && Time.time < _timeJumpWasPressed + _jumpBufferTime;

    private bool _canCoyoteTime; // Flag controlling whether a coyote time jump can be executed or not. True when player lands, turns false when after execute a jump.
    private float _timeLeftGround = float.MinValue; // Keeps track of the time the player left the ground to compare with the current time to see if we're within the coyote time window.
    private bool InCoyoteTime => _canCoyoteTime && Time.time < _timeLeftGround + _coyoteTime;

    // Handles the jumping logic (flags, timers, etc.), then calls ExecuteJump() if we can actually jump to perform the jump
    private void HandleJump()
    {
        // if the player stopped holding jump while the player was in the air and still moving upwards, then set the _endedJumpEarly flag to true
        if(!_endedJumpEarly && !_isGrounded && !_jumpHeldInput && _frameVelocity.y > 0){
            _endedJumpEarly = true;
        }

        if(_isGrounded && _jumpToConsume){
            ExecuteJump();
        }
        else if(_isGrounded && IsJumpBuffered){
            ExecuteJump();
        }
        else if(!_isGrounded && InCoyoteTime && _jumpToConsume){
            ExecuteJump();
        }
        _jumpToConsume = false; // Reset the jumpToConsume flag. Otherwise if player presses jump in the air even outside of buffer time, the jump will be performed when next they hit the ground
    }

    private void ExecuteJump()
    {
        // Turn jump related flags off to prevent further jumps until the player hits the ground again
        _timeJumpWasPressed = 0;
        _canBufferJump = false;
        _jumpToConsume = false;
        _canCoyoteTime = false;
        _endedJumpEarly = false;

        // Set the y component of the player's velocity to the jump force (actually perform the jump)
        _frameVelocity.y = _jumpForce;
    }
    #endregion

    #region Wall Running
    [Header("Wall Running")]
    [SerializeField] private float _wallCheckDistance = 0.1f; // How far from the outside of the player's collider to check for walls.
    [SerializeField] private float _minWallRunHeight = 1f; // The minimum height the player needs to be from the ground to wall run
    private Vector3 _wallNormal; // Add this field to store the wall's normal vector
    private bool _hitWall = false; // Flag to keep track of if the player hit a wall
    private bool _wallOnRightSide;
    private bool _wallOnLeftSide;
    private bool IsWallRunning => !_isGrounded && _hitWall && transform.position.y > _minWallRunHeight && (Mathf.Abs(_horizontalInput) > 0 || Mathf.Abs(_verticalInput) > 0);
    private void CheckWallCollisions(){
        _hitWall = false;
        _wallOnRightSide = false;
        _wallOnLeftSide = false;
        Vector3[] directions = new Vector3[]{
            transform.right, 
            transform.right + transform.forward,
            transform.forward,
            -transform.right + transform.forward,
            -transform.right
        };
        foreach (Vector3 direction in directions)
        {
            Vector3 raycastOrigin = transform.position + direction.normalized * _capsuleCollider.radius;
            Vector3 raycastDirection = direction.normalized;

            RaycastHit hit;
            if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, _wallCheckDistance))
            {
                // Check if the hit surface is roughly perpendicular to the ground
                if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < 0.1f)
                {
                    _hitWall = true;
                    _wallNormal = hit.normal;
                    
                    // Determine the side of the wall
                    Vector3 crossProduct = Vector3.Cross(_wallNormal, transform.forward);
                    if (crossProduct.y > 0)
                    {
                        _wallOnRightSide = true;
                    }
                    else
                    {
                        _wallOnLeftSide = true;
                    }

                    break; // Exit the loop as soon as a wall is hit
                }
            }
        }
    }
    private void HandleWallRunning()
    {
        if (IsWallRunning)
        {
            Vector3 wallRunDirection = Vector3.Cross(_wallNormal, Vector3.up); // Calculate the direction along the wall

            // Adjust the direction based on the side of the wall
            if (_wallOnRightSide)
            {
                wallRunDirection = -wallRunDirection;
            }

            _frameVelocity = wallRunDirection * _movementSpeed; // Modify the player's velocity to move along the wall
            _dashToConsume = false; // Prevent the player from dashing while wall running
        }
    }
    [SerializeField] private float _wallJumpUpForce = 36f; // The upwards force to apply when jumping off a wall
    [SerializeField] private float _wallJumpOutForce = 10f; // The outwards (sideways/away from the wall) force to apply when jumping off a wall
    private void HandleWallJump(){
        if(IsWallRunning && _jumpToConsume){
            _frameVelocity = Vector3.up * _wallJumpUpForce + _wallNormal * _wallJumpOutForce;
            Debug.Log("Wall Jump");
        }

        // Turn jump related flags off to prevent further jumps until the player hits the ground again
        _timeJumpWasPressed = 0;
        // _canBufferJump = false;
        _jumpToConsume = false;
        _canCoyoteTime = false;
        _endedJumpEarly = false;
    }
    #endregion

    #region Dashing
    [Header("Dashing")]
    [SerializeField] private float _dashForce = 10f; // The force to apply when dashing
    [SerializeField] private float _dashUpwardForce = 10f; // The force to apply on the y axis when dashing
    [SerializeField] private float _dashDuration = 0.1f; // The duration of the dash
    [SerializeField] private float _dashCooldown = 1f; // The time to wait before the player can dash again
    private float _dashCooldownTimer = -1f; // The timer to keep track of the dash cooldown
    private void HandleDash()
    {
        if (_dashToConsume && _dashCooldownTimer < 0)
        {
            // Get the direction the camera is facing
            Vector3 dashDirection = _mainCamera.transform.forward;

            // Normalize the dash direction and apply the dash force
            _frameVelocity = dashDirection.normalized * _dashForce;

            // Apply an additional upward force to counteract gravity
            _frameVelocity.y = _dashUpwardForce;

            // Start the dash cooldown
            _dashCooldownTimer = _dashCooldown;

            _dashToConsume = false; // Reset the dashToConsume flag
            Debug.Log("Dash");
        }
    }
    #endregion

    #region Gravity
    [Header("Gravity")]
    [SerializeField] private float _groundingForce = -1.5f; // Constant downard force to apply when the player is grounded, meant to help on slopes
    [SerializeField] private float _maxFallSpeed = 40f; // The maximum speed the player can fall at
    [SerializeField] private float _fallAcceleration = 110f; // The player's capacity to gain fall speed. a.k.a. In Air Gravity
    [SerializeField] private float _endedJumpEarlyGravityModifier = 3f; // Multiplier to apply to the player's fall speed when they let go of the jump button early
    private void HandleGravity()
    {
        // If we're grounded and aren't performing a jump (i.e: _frameVelocity.y has been set to greater than 0 this frame) 
        if(_isGrounded && _frameVelocity.y <= 0f){
            // then apply a small downards force to the player to help them stick to the ground on slopes
            _frameVelocity.y = _groundingForce;
        }
        else{
            // Base gravity value to apply when the player is in the air
            float inAirGravity = _fallAcceleration;
            // Use a multiplier on the gravity value to make the player fall faster when they let go of the jump button early
            if(_endedJumpEarly){
                inAirGravity *= _endedJumpEarlyGravityModifier;
            }
            // Apply the gravity to the player's y velocity
            _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_maxFallSpeed, inAirGravity * Time.fixedDeltaTime); // Similar to calculating the gravity normally then clamping, this is more efficient and in 1 line.
        }
    }
    #endregion

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        // Calculate the positions of the top and bottom of the capsule
        Vector3 capsuleBottom = transform.position + Vector3.down * (_capsuleCollider.height / 2 - _capsuleCollider.radius);
        Vector3 capsuleTop = transform.position + Vector3.up * (_capsuleCollider.height / 2 - _capsuleCollider.radius);

        // Add the offsets to the positions of the spheres
        capsuleBottom += Vector3.up * _capsuleOriginOffset;
        capsuleTop += Vector3.down * _capsuleOriginOffset;

        bool ceilingHit = Physics.CapsuleCast(capsuleBottom, capsuleTop, _capsuleCollider.radius, Vector3.up, out RaycastHit ceilingHitInfo, _groundCheckDistance, ~_playerLayer);

        Color capsuleColor;
        if (_isGrounded)
        {
            capsuleColor = Color.green;
        }
        else if (ceilingHit)
        {
            capsuleColor = Color.blue;
        }
        else
        {
            capsuleColor = Color.red;
        }

        Gizmos.color = capsuleColor;

        // Draw the top half-sphere of the capsule
        Gizmos.DrawWireSphere(capsuleTop, _capsuleCollider.radius);

        // Draw the bottom half-sphere of the capsule
        Gizmos.DrawWireSphere(capsuleBottom, _capsuleCollider.radius);

        // Draw a line for the ground check distance
        Vector3 lineStart = capsuleBottom - Vector3.up * _capsuleCollider.radius;
        Gizmos.DrawLine(lineStart, lineStart + Vector3.down * _groundCheckDistance);
        lineStart = capsuleTop + Vector3.up * _capsuleCollider.radius;
        Gizmos.DrawLine(lineStart, lineStart + Vector3.up * _groundCheckDistance);

        // Draw the wall check rays

        Vector3[] directions = new Vector3[]{
            transform.right, 
            transform.right + transform.forward,
            transform.forward,
            -transform.right + transform.forward,
            -transform.right
        };

        foreach (Vector3 direction in directions)
        {
            Vector3 raycastOrigin = transform.position + direction.normalized * _capsuleCollider.radius;
            Vector3 raycastEnd = raycastOrigin + direction.normalized * _wallCheckDistance;
            
            if (Physics.Raycast(raycastOrigin, direction.normalized, _wallCheckDistance))
            {
                Gizmos.color = Color.red;
            }
            else Gizmos.color = Color.green;

            Gizmos.DrawLine(raycastOrigin, raycastEnd);
        }
    }

    #region State
    public MovementState movementState;
    public enum MovementState
    {
        Walking,
        Sprinting,
        Sliding,
        Airborne
    }
    private void StatedHandler()
    {
        // Sprinting
        if(_isGrounded && _sprintHeldInput){
            movementState = MovementState.Sprinting;
            _movementSpeed = _sprintSpeed;
        }
        else if(_isGrounded){
            movementState = MovementState.Walking;
            _movementSpeed = _walkSpeed;
        }
        else{
            movementState = MovementState.Airborne;
        }
    }
    #endregion
}
