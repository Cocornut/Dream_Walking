using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float currentMoveSpeed;
    [SerializeField] float moveSpeed;
    [SerializeField] float runSpeed;
    [SerializeField] float crouchSpeed;
    [SerializeField] float groundDrag;

    [SerializeField] float jumpForce;
    [SerializeField] float jumpCooldown;
    [SerializeField] float airMultiplier;

    [SerializeField] float crouchCooldown;
    [SerializeField] float runCooldown;

    [SerializeField] float gravity = 9.8f;

    [Header("Keybinds")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] KeyCode runKey = KeyCode.LeftShift;
    [SerializeField] KeyCode visionKey = KeyCode.Mouse1;
    [SerializeField] KeyCode interactKey = KeyCode.Mouse0;

    [Header("Checks")]
    [SerializeField] bool grounded;
    [SerializeField] bool onLadder;
    [SerializeField] bool readyToJump;

    bool readyToCrouch;
    [SerializeField] bool isCrouching;
    bool readyToRun;
    [SerializeField] bool isRunning;

    [Header("Components")]
    public Transform orientation;
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
    }

    private void Update()
    {
        MyInput();      

        // Handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
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

            Invoke(nameof(ResetCrouch), crouchCooldown);
        }        
        
        if (Input.GetKeyDown(runKey) && readyToRun && grounded)
        {
            isRunning = !isRunning;

            Invoke(nameof(ResetRun), runCooldown);
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
}
