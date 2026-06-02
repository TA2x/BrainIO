using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public bool _enabled = true;

    PlayerInput input;

    Rigidbody rb;

    [Header("Movement")]
    float moveSpeed;
    [SerializeField] private float walkSpeed = 7f;
    [SerializeField] private float crouchSpeed = 4f;
    [SerializeField] private float moveMultiplier = 10f;
    [SerializeField] private float airMultiplier = 0.4f;

    [SerializeField] private Transform orientation;

    Vector3 moveDirection;

    [Header("Drag")]
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private float airDrag = 2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask whatIsGround;
    bool isGrounded;
    float startHeight;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle = 60f;
    [SerializeField] private float slopeDownForce = 5f;
    RaycastHit slopeHit;
    bool exitingSlope;

    [Header("Crouch")]
    [SerializeField] private Transform gfx;
    [SerializeField] private float crouchHeight;
    bool isCrouching = false;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private bool canJump = true;
    [SerializeField] private float jumpCooldown = 0.2f;
    bool readyToJump = true;

    [Header("Sway")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Transform head;
    private float swayFreq;
    [SerializeField] private float walkSwayFreq;
    [SerializeField] private float crouchSwayFreq;
    [SerializeField] private float walkSwayAmp;
    float noiseAmp;
    float noiseFreq;
    CinemachineBasicMultiChannelPerlin camNoise;

    [Header("Footsteps")]
    [SerializeField] AudioClip[] footsteps;
    [SerializeField] AudioSource footstepsSource;
    [SerializeField] float footstepsTimerVal = 0.35f;
    float footstepsTimer;

    private void Awake()
    {
        input = new PlayerInput();
        input.Movement.Enable();

        input.Movement.Crouch.performed += Crouch;
        input.Movement.Crouch.canceled += UnCrouch;
        input.Movement.Jump.performed += Jump;

        rb = GetComponent<Rigidbody>();

        playerHeight = gfx.transform.localScale.y * 2f;

        startHeight = playerHeight;

        camNoise = cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        noiseAmp = camNoise.AmplitudeGain;
        noiseFreq = camNoise.FrequencyGain;

        swayFreq = walkSwayFreq;

        footstepsTimer = footstepsTimerVal;
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, whatIsGround);

        if (_enabled)
        {
            GetInput();
            SpeedControl();
            SpeedLimit();

            if (rb.linearVelocity.magnitude > 0 && moveDirection.magnitude > 0)
            {
                footstepsTimer -= Time.deltaTime;
                if (footstepsTimer <= 0)
                {
                    PlayFootstep();
                    footstepsTimer = footstepsTimerVal;
                }
            }
        }

        rb.linearDamping = isGrounded ? groundDrag : airDrag;
        rb.useGravity = !OnSlope();

        if (moveDirection.magnitude != 0 && isGrounded)
        {
            camNoise.AmplitudeGain = 0f;
            camNoise.FrequencyGain = 0f;

            WalkSway();
        }
        else
        {
            camNoise.AmplitudeGain = noiseAmp;
            camNoise.FrequencyGain = noiseFreq;

            head.localPosition = Vector3.Lerp(head.localPosition, Vector3.zero, 0.5f * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (_enabled)
            MovePlayer();
    }

    void GetInput()
    {
        Vector2 inputDirection = input.Movement.Movement.ReadValue<Vector2>();

        moveDirection = orientation.forward * inputDirection.y + orientation.right * inputDirection.x;
    }

    void SpeedControl()
    {
        if (isCrouching)
        {
            moveSpeed = crouchSpeed;
        }
        else
        {
            moveSpeed = walkSpeed;
        }
    }

    void MovePlayer()
    {
        if (isGrounded)
        {
            if (OnSlope() && !exitingSlope)
            {
                rb.AddForce(moveSpeed * moveMultiplier * SlopeMoveDirection().normalized, ForceMode.Acceleration);

                if (rb.linearVelocity.y > 0)
                    rb.AddForce(Vector3.down * slopeDownForce, ForceMode.Force);
            }
            else
                rb.AddForce(moveSpeed * moveMultiplier * moveDirection.normalized, ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(moveSpeed * moveMultiplier * airMultiplier * moveDirection.normalized, ForceMode.Acceleration);
        }
    }

    void SpeedLimit()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 baseVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (baseVelocity.magnitude > moveSpeed)
            {
                Vector3 limitedVelocity = baseVelocity.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
            }
        }
    }

    bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f, whatIsGround))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);

            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    Vector3 SlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }

    void WalkSway()
    {
        float speed = rb.linearVelocity.magnitude / moveSpeed;

        float swayX = Mathf.Sin(Time.time * swayFreq) * walkSwayAmp * speed;
        float swayY = Mathf.Cos(Time.time * swayFreq * 2) * walkSwayAmp * 0.5f * speed;

        head.localPosition = new Vector3(swayX, swayY, 0f);
    }

    void Crouch(InputAction.CallbackContext callbackContext)
    {
        gfx.localScale = new Vector3(gfx.localScale.x, crouchHeight, gfx.localScale.z);
        rb.AddForce(Vector3.down * 8f, ForceMode.Impulse);
        playerHeight = crouchHeight * 2f;
        swayFreq = crouchSwayFreq;
        isCrouching = true;
    }
    
    void UnCrouch(InputAction.CallbackContext callbackContext)
    {
        playerHeight = startHeight;
        gfx.localScale = new Vector3(gfx.localScale.x, playerHeight * 0.5f, gfx.localScale.z);
        swayFreq = walkSwayFreq;
        isCrouching = false;
    }

    void Jump(InputAction.CallbackContext callbackContext)
    {
        if (!canJump) return;
        if (!readyToJump) return;
        if (!isGrounded) return;

        readyToJump = false;
        exitingSlope = true;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); ;

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        Invoke(nameof(ResetJump), jumpCooldown);
    }

    void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    void PlayFootstep()
    {
        footstepsSource.clip = footsteps[Random.Range(0, footsteps.Length)];
        footstepsSource.Play();
    }
}
