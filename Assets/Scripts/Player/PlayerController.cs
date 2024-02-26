using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// To-Do:
// - Add a wall running state
// - Add a wall jump state
// - Add Dashing
// - Add Swinging 

public class PlayerController : MonoBehaviour
{
    private PlayerBaseState _currentState;
    private PlayerStateFactory _stateFactory;
    public PlayerBaseState CurrentState { get{ return _currentState; } set{ _currentState = value; } }
    private Vector3 _frameVelocity; // Buffer variable to calculate the player's velocity all at once before then setting the _rb.velocity to this

    #region Parameters
    [Header("Component References")]
    private Rigidbody _rb;
    private CapsuleCollider _capsuleCollider;
    [SerializeField] private Camera _mainCamera;
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _dashAction;
    private InputAction _sprintAction;
    
    [Header("Collisions Check")]
    [SerializeField] private LayerMask _playerLayer; // Set this to the layer(s) which your ground is on
    [SerializeField] private float _groundCheckDistance = 0.1f; // How far to check for the ground
    [SerializeField] private float _capsuleOriginOffset = 0.01f; // Offset capsule origin - prevent sphere from intersecting the ground when the capsule is on it
    private bool _isGrounded;
    
    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 10f; // Player's movement speed when walking (base speed)
    [SerializeField] private float _sprintSpeed = 15f; // Player's movement speed when sprinting
    [SerializeField] private float _rotationSpeed = 7.5f; // How fast the player should rotate to face the direction they want to move in
    [SerializeField] private float _airMultiplier = 0.8f; // How much slower the player should move in the air
    [SerializeField] private float _acceleration = 120f; // How fast the player should accelerate horizontally
    [SerializeField] private float _groundDeceleration = 60f; // How fast the player should stop when on the ground
    [SerializeField] private float _airDeceleration = 30f; // How fast the player should stop when in the air

    [Header("Gravity")]
    [SerializeField] private float _groundingForce = -1.5f; // Constant downard force to apply when the player is grounded, meant to help on slopes
    [SerializeField] private float _maxFallSpeed = 40f; // The maximum speed the player can fall at
    [SerializeField] private float _fallAcceleration = 110f; // The player's capacity to gain fall speed. a.k.a. In Air Gravity
    [SerializeField] private float _endedJumpEarlyGravityModifier = 3f; // Multiplier to apply to the player's fall speed when they let go of the jump button early

    [Header("Jumping")]
    [SerializeField] private float _jumpForce = 36f; // How much force to apply when jumping
    [SerializeField] private float _jumpBufferTime = 0.2f; // How long before the player hits the ground where their jump input is accepted and buffered.
    [SerializeField] private float _coyoteTime = 0.2f; // How long after the player leaves the ground where they can still jump
    private bool _jumpToConsume; // Becomes true when jump button is pressed, and remains so until the player actually jumps in the Jump() method. Prevents input from getting lost between physics and update frames.
    private bool _endedJumpEarly; // Flag to check if the player let go of the jump button early to cut their jump short
    private bool _canBufferJump; // Flag controlling whether a buffered jump can be executed or not. True when player lands, false when they execute a jump. (Should only be able to buffer a jump once in the air).
    private float _timeJumpWasPressed = float.MinValue; // Keeps track of the time the player pressed the jump button to compare with the current time to see if the jump input was buffered.
    private bool _canCoyoteTime; // Flag controlling whether a coyote time jump can be executed or not. True when player lands, turns false when after execute a jump.
    private float _timeLeftGround = float.MinValue; // Keeps track of the time the player left the ground to compare with the current time to see if we're within the coyote time window.

    [Header("Dashing")]
    [SerializeField] private float _dashForce = 10f; // The force to apply when dashing
    [SerializeField] private float _dashUpwardForce = 10f; // The force to apply on the y axis when dashing
    [SerializeField] private float _dashDuration = 0.1f; // The duration of the dash
    [SerializeField] private float _dashCooldown = 1f; // The time to wait before the player can dash again
    private bool _dashToConsume; // Becomes true when dash button is pressed, and remains so until the player actually dashes in the Dash() method. Prevents input from getting lost between physics and update frames.
    private float _dashCooldownTimer = -1f; // The timer to keep track of the dash cooldown

    [Header("Wall Running")]
    [SerializeField] private float _wallCheckDistance = 0.1f; // How far from the outside of the player's collider to check for walls.
    [SerializeField] private float _minWallRunHeight = 1f; // The minimum height the player needs to be from the ground to wall run
    [SerializeField] private float _wallRunGravity = -1.0f; // The gravity to apply when wall running
    [SerializeField] private float _wallJumpUpForce = 36f; // The upwards force to apply when jumping off a wall
    [SerializeField] private float _wallJumpOutForce = 10f; // The outwards (sideways/away from the wall) force to apply when jumping off a wall
    private Vector3 _wallNormal; // Add this field to store the wall's normal vector
    private bool _hitWall = false; // Flag to keep track of if the player hit a wall
    private bool _wallOnRightSide;
    private bool IsWallRunning => !_isGrounded && _hitWall && transform.position.y > _minWallRunHeight && (_movementInput != Vector2.zero);
    #endregion

    #region Getters & Setters
    public Vector3 FrameVelocity { get => _frameVelocity; set => _frameVelocity = value; }
    public Vector2 MovementInput => _movementInput;
    public bool MovementPressedInput => _movementInput != Vector2.zero;
    public bool JumpHeldInput => _jumpHeldInput;
    public bool SprintHeldInput => _sprintHeldInput;
    public float WalkSpeed => _walkSpeed;
    public float SprintSpeed => _sprintSpeed;
    public float HorizontalAcceleration => _acceleration;
    public float GroundDeceleration => _groundDeceleration;
    public float AirDeceleration => _airDeceleration;
    public float JumpForce => _jumpForce;
    public float WallJumpUpForce => _wallJumpUpForce;
    public float WallJumpOutForce => _wallJumpOutForce;
    public float AirMultiplier => _airMultiplier;
    public float GroundingForce => _groundingForce;
    public float FallAcceleration => _fallAcceleration;
    public float MaxFallSpeed => _maxFallSpeed;
    public float WallRunGravity => _wallRunGravity;
    public float EndedJumpEarlyGravityModifier => _endedJumpEarlyGravityModifier;
    public bool IsJumpBuffered => _canBufferJump && Time.time < _timeJumpWasPressed + _jumpBufferTime;
    public bool InCoyoteTime => _canCoyoteTime && Time.time < _timeLeftGround + _coyoteTime;
    public bool IsGrounded { get => _isGrounded; set => _isGrounded = value; }
    public bool JumpToConsume { get => _jumpToConsume; set => _jumpToConsume = value; }
    public float TimeJumpWasPressed { get => _timeJumpWasPressed; set => _timeJumpWasPressed = value; }
    public bool CanBufferJump { get => _canBufferJump; set => _canBufferJump = value; }
    public bool CanCoyoteTime { get => _canCoyoteTime; set => _canCoyoteTime = value; }
    public bool EndedJumpEarly { get => _endedJumpEarly; set => _endedJumpEarly = value; }
    public float TimeLeftGround { get => _timeLeftGround; set => _timeLeftGround = value; }
    public bool IsTouchingWall => _hitWall;
    public bool WallOnRightside => _wallOnRightSide;
    public float MinimumWallRunHeight => _minWallRunHeight;
    public Vector3 WallNormal => _wallNormal;
    #endregion

    private void Awake()
    {
        // Initialize the state factory and initial state
        _stateFactory = new PlayerStateFactory(this);
        _currentState = _stateFactory.Grounded();
        _currentState.EnterState();

        // Get the PlayerInput component and the actions
        _playerInput = GetComponent<PlayerInput>();
        _moveAction = _playerInput.actions["Move"];
        _jumpAction = _playerInput.actions["Jump"];
        _sprintAction = _playerInput.actions["Sprint"];
        _dashAction = _playerInput.actions["Dash"];
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleInput();
        if(Input.GetKeyDown(KeyCode.Q)){
            transform.position = new Vector3(-30.6f, 61f, -0.15f);
        }
    }

    private void FixedUpdate()
    {
        CheckCollisions();
        RotatePlayer();
        _currentState.UpdateStates();
        _rb.velocity = _frameVelocity;
    }
    private void RotatePlayer()
    {
        Vector3 inputDirection = GetMovementDirection();
        // If the player is moving, rotate the player to face the direction the player wants to move in
        if (inputDirection != Vector3.zero)
            transform.forward = Vector3.Slerp(transform.forward, inputDirection, _rotationSpeed * Time.fixedDeltaTime);
    }
    // Helper method to calculate the direction the player should move in based on the camera's orientation
    // Used both when rotating the player above, and when moving the player in horizontal movement states
    public Vector3 GetMovementDirection(){
        // Get the camera's forward (z-axis) and right (x-axis) vectors
        Vector3 cameraForward = _mainCamera.transform.forward;
        Vector3 cameraRight = _mainCamera.transform.right;
        // Caclulate the direction the player should move in by adding the camera's forward and right vectors so that the player's inputs are relative to the camera's orientation
        Vector3 inputDirection = cameraForward * _movementInput.y + cameraRight * _movementInput.x;
        inputDirection.y = 0;   // Set the y component to 0 to prevent the player from moving up or down
        inputDirection.Normalize(); // Normalize the vector to prevent the player from moving faster when moving diagonally
        return inputDirection;
    }

    #region Input

    private Vector2 _movementInput;
    private bool _jumpHeldInput;
    private bool _sprintHeldInput;
    private void HandleInput()
    {
        _movementInput = _moveAction.ReadValue<Vector2>();

        _jumpHeldInput = _jumpAction.IsPressed();
        _sprintHeldInput = _sprintAction.IsPressed();

        if (_jumpAction.WasPressedThisFrame())
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = Time.time;
        }

        if (_dashAction.WasPressedThisFrame() && _dashCooldownTimer <= 0)
        {
            _dashToConsume = true;
        }
    }
    #endregion
    #region Collision Checks
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
        _isGrounded = groundHit;
        CheckWallCollisions();
    }

    private void CheckWallCollisions(){
        _hitWall = false;
        _wallOnRightSide = false;
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
                    // If the cross-product of the wall's normal, and direction we're facing is positive, then the wall's on our right, otherwise it's on our left
                    _wallOnRightSide = crossProduct.y > 0; 
                    break; // Exit the loop as soon as a wall is hit
                }
            }
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

    void OnGUI()
    {
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontSize = 24; // Change the size as needed
        guiStyle.normal.textColor = Color.white; // Change the color as needed

        // Display the current state's name at the top left of the screen
        GUI.Label(new Rect(10, 10, 300, 50), "Current State: " + _currentState.GetType().Name, guiStyle);
        if(_currentState.Substate != null)
            GUI.Label(new Rect(10, 50, 300, 50), "Sub State: " + _currentState.Substate.GetType().Name, guiStyle);
    }
}