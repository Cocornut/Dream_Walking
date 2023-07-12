using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    [Header("Components")]
    private CharacterController controller;
    private Transform playerCamera;

    [Header("Movement")]
    [SerializeField] float currentMoveSpeed;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float runSpeed = 10f;
    [SerializeField] float crouchSpeed = 3f;
    [SerializeField] float ladderSpeed = 3f;

    [Header("Jumping")]
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float jumpHeight = 2f;
    Vector3 velocity;

    [Header("Input")]
    private float xInput;
    public float zInput;

    [Header("Interaction")]
    public float interactDistance = 3f;
    public LayerMask useLayers;
    [SerializeField] GameObject doorManager;

    [Header("Vision")]
    [SerializeField] RenderFeatureManager renderFeatureManager;
    [SerializeField] PickupScript pickupScript;
    [SerializeField] GameObject pickupObject;

    [Header("Checks")]
    public bool isGrounded;
    public bool onLadder;
    [SerializeField] bool isRunning;
    [SerializeField] bool isCrouching;
    [SerializeField] bool isVision;

    bool readyToJump;
    bool readyToCrouch;
    bool readyToInteract;

    [Header("Cooldowns")]
    [SerializeField] float jumpCooldown;
    [SerializeField] float runCooldown;
    [SerializeField] float crouchCooldown;
    [SerializeField] float interactCooldown;
    [SerializeField] float visionCooldown;

    private float visionCurrent;
    private float runCurrent;


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = gameObject.transform.Find("Camera").transform;

        doorManager = GameObject.Find("DoorManager");
        renderFeatureManager = GameObject.Find("RenderFeatureToggler").GetComponent<RenderFeatureManager>();

        pickupObject = GameObject.FindGameObjectWithTag("Pickup");
        pickupScript = pickupObject.GetComponent<PickupScript>();

        visionCurrent = visionCooldown;
        runCurrent = runCooldown;
        ResetJump();
        ResetCrouch();
        ResetInteraction();
    }

    void Update()
    {
        CheckBooleans();
        MyInput();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        if (onLadder)
        {
            Vector3 ladderMove = transform.up * zInput * ladderSpeed;
            controller.Move(ladderMove * Time.deltaTime);
        }
        else
        {
            // Determine movement speed
            currentMoveSpeed = moveSpeed;
            if (isCrouching)
                currentMoveSpeed = crouchSpeed;
            else if (isRunning)
                currentMoveSpeed = runSpeed;
            else if (isRunning && isCrouching)
                currentMoveSpeed = moveSpeed;

            // Reset velocity
            if (isGrounded && velocity.y < 0)
            {
                velocity.x = 0f;
                velocity.y = -2f;
                velocity.z = 0f;
            }

            // Set movement direction
            Vector3 move = transform.right * xInput + transform.forward * zInput;

            // Move on ground
            controller.Move(move * currentMoveSpeed * Time.deltaTime);

            // Apply gravity when not touching the ground or a ladder
            if (!isGrounded && !onLadder)
            {
                velocity.y += gravity * Time.deltaTime;
            }

            // Move up and down
            controller.Move(velocity * Time.deltaTime);
        }
    }

    private void MyInput()
    {
        xInput = Input.GetAxis("Horizontal");
        zInput = Input.GetAxis("Vertical");

        // Jump if ready and on ground or ladder
        if (Input.GetButtonDown("Jump") && readyToJump && (isGrounded || onLadder))
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Crouch if ready
        if (Input.GetKeyDown(KeyCode.C) && readyToCrouch)
        {
            isCrouching = !isCrouching;

            Crouch();

            Invoke(nameof(ResetCrouch), crouchCooldown);
        }

        // Run if ready
        if (Input.GetKeyDown(KeyCode.LeftShift) && isGrounded)
        {
            if (!isRunning && runCurrent >= runCooldown)
            {
                isRunning = true;
            }
            else if (isRunning)
            {
                isRunning = false;
            }
        }

        // Interact with doors
        if (Input.GetKeyDown(KeyCode.E) && readyToInteract)
        {
            if (readyToInteract)
            {
                readyToInteract = false;

                Interact();

                Invoke(nameof(ResetInteraction), interactCooldown);
            }
        }

        // Use dream vision
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (!isVision && visionCurrent >= visionCooldown)
            {
                isVision = true;
            }
            else if (isVision)
            {
                isVision = false;
            }
        }
    }

    private void CheckBooleans()
    {
        isGrounded = controller.isGrounded;

        if (runCurrent > 0 && isRunning)
        {
            runCurrent -= Time.deltaTime;
            if (runCurrent <= 0)
            {
                isRunning = false;
            }
        }
        else if (!isRunning && runCurrent < runCooldown)
        {
            runCurrent += Time.deltaTime;
            if (runCurrent >= runCooldown)
            {
                runCurrent = runCooldown;
            }
        }

        if (visionCurrent > 0 && isVision)
        {
            ToggleRenderFeatures(true);
            visionCurrent -= Time.deltaTime;
            if (visionCurrent <= 0)
            {
                isVision = false;
            }
        }
        else if (!isVision && visionCurrent < visionCooldown)
        {
            ToggleRenderFeatures(false);
            visionCurrent += Time.deltaTime;
            if (visionCurrent >= visionCooldown)
            {
                visionCurrent = visionCooldown;
            }
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void Jump()
    {
        if (onLadder)
        {
            // Calculate jump direction away from the ladder
            Vector3 jumpDirection = (transform.forward + transform.up).normalized;
            velocity += jumpDirection * jumpHeight;

            // Disable ladder movement
            onLadder = false;
        }
        else
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void ResetCrouch()
    {
        readyToCrouch = true;
    }

    private void Crouch()
    {
        if (isCrouching)
        {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y / 2f, transform.localScale.z);
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * 2f, transform.localScale.z);
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        }
    }

    private void ResetInteraction()
    {
        readyToInteract = true;
    }

    private void Interact()
    {
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, interactDistance, useLayers))
        {
            // Interacting with corridor doors
            if (hit.collider.TryGetComponent<CorridorDoorScript>(out CorridorDoorScript door))
            {
                doorManager.GetComponent<DoorManagerScript>().CheckKeyCollected(door.doorID);
                if (door.hasKey)
                {
                    if (door.isOpen)
                    {
                        door.Close();
                    }
                    else
                    {
                        door.Open(transform.position);
                    }
                }
                else
                {
                    door.Budge();
                }
            }
        }
    }

    public void ToggleRenderFeatures(bool isEnabled)
    {
        for (int i = 0; i < renderFeatureManager.renderFeatures.Count; i++)
        {
            RenderFeatureToggle toggleObj = renderFeatureManager.renderFeatures[i];
            toggleObj.isEnabled = isEnabled;
            toggleObj.feature.SetActive(isEnabled);
            renderFeatureManager.renderFeatures[i] = toggleObj;
        }
        pickupObject.GetComponent<MeshRenderer>().enabled = isEnabled;
        pickupScript.isRendered = isEnabled;
    }
}
