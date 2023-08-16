using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovementScript : MonoBehaviour
{
    [Header("Components")]
    private CharacterController controller;
    public GameObject cameraObject;
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
    [SerializeField] GameObject keyObject;

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

    [SerializeField] Image runCooldownImage;
    [SerializeField] Image visionCooldownImage;

    private float visionCurrent;
    private float runCurrent;

    [Header("Audio")]
    [SerializeField] private AudioSource leftStep;
    [SerializeField] private AudioSource rightStep;

    [SerializeField] private AudioSource leftRun;
    [SerializeField] private AudioSource rightRun;

    [SerializeField] private AudioSource jumpStart;
    [SerializeField] private AudioSource jumpEnd;

    private float footstepTimer;
    private bool left;
    private bool right;

    private bool playRunSound;
    private bool playStepSound;

    private bool wasGrounded = true;
    private float notGroundedTimer = 0f;
    private const float fallThreshold = 0.2f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        GameObject camera = Instantiate(cameraObject, new Vector3(transform.position.x, transform.position.y + 0.579f, transform.position.z + 0.054f), Quaternion.identity, transform);
        playerCamera = camera.transform;

        doorManager = GameObject.FindGameObjectWithTag("DoorManager");
        renderFeatureManager = GameObject.Find("RenderFeatureToggler").GetComponent<RenderFeatureManager>();

        pickupObject = GameObject.FindGameObjectWithTag("Pickup");
        pickupScript = pickupObject.GetComponent<PickupScript>();

        keyObject = GameObject.FindGameObjectWithTag("Key");

        visionCurrent = visionCooldown;
        runCurrent = runCooldown;

        runCooldownImage = GameObject.Find("SprintCooldown").GetComponent<Image>();
        visionCooldownImage = GameObject.Find("VisionCooldown").GetComponent<Image>();

        ResetJump();
        ResetCrouch();
        ResetInteraction();
    }

    void Update()
    {
        CheckBooleans();
        MyInput();
        Highlight();
        UpdateCooldownUI();
    }

    private void FixedUpdate()
    {
        MovePlayer();
        PlayFootsteps();
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

    private void UpdateCooldownUI()
    {
        runCooldownImage.fillAmount = runCurrent / runCooldown;
        visionCooldownImage.fillAmount = visionCurrent / visionCooldown;
    }

    private void PlayFootsteps()
    {
        if (playStepSound)
        {
            footstepTimer += Time.deltaTime;

            if (footstepTimer >= 1f)
            {
                if (!left)
                {
                    leftStep.Play();
                    left = true;
                    footstepTimer = 0.5f;
                }
                else if (!right)
                {
                    rightStep.Play();
                    right = true;
                    footstepTimer = 0f;
                }
            }
            if (footstepTimer >= 0.5f)
            {
                left = false;
            }
            if (footstepTimer >= 1.0f)
            {
                right = false;
            }
        }
        else if (playRunSound)
        {
            footstepTimer += Time.deltaTime;

            if (footstepTimer >= 0.7f)
            {
                if (!left)
                {
                    leftRun.Play();
                    left = true;
                    footstepTimer = 0.35f;
                }
                else if (!right)
                {
                    rightRun.Play();
                    right = true;
                    footstepTimer = 0f;
                }
            }
            if (footstepTimer >= 0.5f)
            {
                left = false;
            }
            if (footstepTimer >= 1.0f)
            {
                right = false;
            }
        }
        else
        {
            left = false;
            right = false;            
        }
    }

    private void CheckBooleans()
    {
        isGrounded = controller.isGrounded;

        if (!isGrounded)
        {
            wasGrounded = false;
            notGroundedTimer += Time.deltaTime;
        }
        else
        {
            if (!wasGrounded && notGroundedTimer >= fallThreshold)
            {
                // Play fall sound
                jumpEnd.Play();
            }

            wasGrounded = true;
            notGroundedTimer = 0f;
        }

        if (xInput != 0 || zInput != 0)
        {
            if (isGrounded)
            {
                if (isCrouching)
                {
                    playRunSound = false;
                    playStepSound = true;
                }
                else 
                {
                    if (isRunning)
                    {
                        playStepSound = false;
                        playRunSound = true;
                    }
                    else
                    {
                        playRunSound = false;
                        playStepSound = true;
                    }
                }
            }
            else
            {
                playStepSound = false;
                playRunSound = false;
            }
        }
        else
        {
            playRunSound = false;
            playStepSound = false;
        }

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
        jumpStart.Play();

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

    private void Highlight()
    {

        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, interactDistance, useLayers))
        {
            if (hit.collider.TryGetComponent<KeyScript>(out KeyScript key))
            {
                key.Highlight();
            }
            else if (hit.collider.TryGetComponent<PickupScript>(out PickupScript pickup))
            {
                pickup.Highlight();
            }
        }
        else
        {
            keyObject.GetComponent<KeyScript>().StopHighlight();
            pickupObject.GetComponent<PickupScript>().StopHighlight();
        }
    }

    private void Interact()
    {
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, interactDistance, useLayers))
        {
            // Interacting with corridor doors
            if (hit.collider.TryGetComponent<CorridorDoorScript>(out CorridorDoorScript door))
            {
                if (keyObject.GetComponent<KeyScript>().isPickedUp)
                {
                    door.Unlock();
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

            // Interacting with keys
            if (hit.collider.TryGetComponent<KeyScript>(out KeyScript key))
            {
                key.Pickup();
            }

            // Interacting with the pickup object
            if (hit.collider.TryGetComponent<PickupScript>(out PickupScript pickup))
            {
                pickup.Pickup();
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
