using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float currentMoveSpeed;                // Current movement speed
    [SerializeField] float moveSpeed;                       // Normal movement speed
    [SerializeField] float runSpeed;                        // Movement speed when running
    [SerializeField] float crouchSpeed;                     // Movement speed while crouching
    [SerializeField] float groundDrag;                      

    [Header("Jumping")]
    [SerializeField] float jumpForce;                       // Determines velocity for jump
    [SerializeField] float airMultiplier;                   // Movement in air
    [SerializeField] float gravity = 9.8f;                  // Modifiable falling speed

    /// <summary>
    /// Vision toggling is done
    /// TODO:
    /// Vision shading
    /// </summary>
    [Header("Vision")]
    [SerializeField] Color visionColor;
    [SerializeField] float visionFadeSpeed;
    private float visionStartTime;
    private float currentVisionIntensity;
    [SerializeField] Material visionMaterial;

    [Header("Durations")]
    [SerializeField] float runDuration;
    private float runStartTime;
    [SerializeField] float visionDuration;

    [Header("Cooldowns")]                                   // Player mechanics cooldowns
    [SerializeField] float jumpCooldown;
    [SerializeField] float crouchCooldown;
    [SerializeField] float runCooldown;
    [SerializeField] float visionCooldown;
    [SerializeField] float interactCooldown;    

    [Header("Keybinds")]                                    // Player Controls
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] KeyCode runKey = KeyCode.LeftShift;
    [SerializeField] KeyCode visionKey = KeyCode.Mouse1;
    [SerializeField] KeyCode interactKey = KeyCode.Mouse0;

    [Header("Checks")]
    bool readyToJump;
    [SerializeField] bool grounded;
    [SerializeField] bool onLadder;
    bool readyToCrouch;
    [SerializeField] bool isCrouching;
    bool readyToRun;
    [SerializeField] bool isRunning;
    bool readyToVision;
    [SerializeField] bool isVision;
    bool readyToInteract;
    [SerializeField] bool isInteracting;

    [Header("Components")]
    public Transform orientation;
    public Transform cameraPosition;
    Vector3 moveDirection;
    Rigidbody rb;

    [Header("Input")]
    float horizontalInput;
    float verticalInput;

    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        ResetJump();
        ResetCrouch();
        ResetRun();
        ResetVision();
        ResetInteraction();
    }

    private void Update()
    {
        MyInput();      

        // Handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;

        //if (isVision)
        //{
        //    Vision();
        //}
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        MovePlayer();
        SpeedControl();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKey(jumpKey) && readyToJump && (grounded || onLadder))
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey) && readyToCrouch)
        {
            isCrouching = !isCrouching;

            Crouch();

            Invoke(nameof(ResetCrouch), crouchCooldown);
        }        
        
        if (Input.GetKeyDown(runKey) && readyToRun && grounded)
        {
            if (!isRunning)
            {
                isRunning = true;
                runStartTime = Time.time;
            }
            else
            {
                isRunning = false;
                runStartTime = 0f;
                Invoke(nameof(ResetRun), runCooldown);
            }
            isRunning = !isRunning;

            Invoke(nameof(ResetRun), runCooldown);
        }

        if (Input.GetKeyDown(visionKey) && readyToVision)
        {
            isVision = !isVision;
            visionStartTime = Time.time;
            currentVisionIntensity = 0f;

            Invoke(nameof(ResetVision), visionCooldown);
        }
    }

    private void MovePlayer()
    {
        if (onLadder)
        {
            // Calculate vertical movment on ladder
            float verticalMovement = verticalInput * moveSpeed;
            rb.velocity = new Vector3(rb.velocity.x, verticalMovement, rb.velocity.z);
        }
        else
        {
            // Determine movement speed
            currentMoveSpeed = moveSpeed;
            if (isCrouching)
                currentMoveSpeed = crouchSpeed;
            else if (isRunning)
                currentMoveSpeed = runSpeed;
            else if (isCrouching && isRunning)
                currentMoveSpeed = moveSpeed;

            // Calculate movement direction
            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

            // On ground
            if (grounded)
                rb.AddForce(moveDirection.normalized * currentMoveSpeed * 10f, ForceMode.Force);

            // In air
            else if (!grounded)
                rb.AddForce(moveDirection.normalized * currentMoveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity
        if (flatVel.magnitude > currentMoveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentMoveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // Reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (onLadder)
        {
            // Calculate jump direction away from the ladder
            Vector3 jumpDirection = (transform.forward + transform.up).normalized;
            rb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);

            // Disable ladder movement and enable gravity
            onLadder = false;
            rb.useGravity = true;
        }
        else
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void Crouch()
    {
        if (isCrouching)
        {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y - 0.5f, transform.localScale.z);
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y + 0.5f, transform.localScale.z);
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

        }
    }

    private void ApplyGravity()
    {
        if (!grounded && !onLadder)
        {
            Vector3 gravityForce = -transform.up * gravity;
            rb.AddForce(gravityForce, ForceMode.Acceleration);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void ResetCrouch()
    {
        readyToCrouch = true;
    }    
    
    private void ResetRun()
    {
        readyToRun = true;
    }

    private void ResetVision()
    {
        readyToVision = true; 
    }

    private void ResetInteraction()
    {
        readyToInteract = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            grounded = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ladder"))
        {
            onLadder = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            grounded = false;
        }
        if (collision.gameObject.CompareTag("Ladder"))
        {
            onLadder = false;
            rb.useGravity = true;
        }
    }

    ///TODO///
    ///Do Vision Shading
    //private void ApplyVisionEffect(float intensity)
    //{
    //    // Adjust the overall intensity of the vision effect
    //    RenderSettings.ambientLight = Color.Lerp(Color.white, visionColor, intensity);
    //}

    //private void Vision()
    //{
    //    float visionTime = Time.time - visionStartTime;
    //    float visionProgress = Mathf.Clamp01(visionTime / visionDuration);

    //    // Calculate current vision intensity (darkness) based on the vision progress
    //    currentVisionIntensity = Mathf.Lerp(0f, 1f, visionProgress);

    //    // Apply vision effect to the material
    //    visionMaterial.SetFloat("_VisionIntensity", currentVisionIntensity);
    //    visionMaterial.SetColor("_VisionColor", visionColor);

    //    if (visionProgress >= 1f)
    //    {
    //        // Vision effect completed, reset variables
    //        isVision = false;
    //        currentVisionIntensity = 0f;
    //    }
    //}
}
