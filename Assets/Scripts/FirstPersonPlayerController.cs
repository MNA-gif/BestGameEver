using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public sealed class FirstPersonPlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = "Player";
    [SerializeField] private string moveActionName = "Move";
    [SerializeField] private string lookActionName = "Look";
    [SerializeField] private string jumpActionName = "Jump";
    [SerializeField] private string sprintActionName = "Sprint";
    [SerializeField] private string crouchActionName = "Crouch";

    [Header("View")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float lookSensitivity = 0.08f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private bool lockCursorOnStart = true;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4.5f;
    [SerializeField] private float sprintSpeed = 7.5f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -20f;

    [Header("Crouch")]
    [SerializeField] private float crouchHeight = 1.2f;
    [SerializeField] private float crouchTransitionSpeed = 12f;

    [Header("Stairs")]
    [SerializeField] private float stepOffset = 0.35f;
    [SerializeField] private float slopeLimit = 55f;

    private CharacterController characterController;

    private InputActionMap playerMap;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
    private bool usesAssetActions;

    private float pitch;
    private float verticalVelocity;
    private bool jumpRequested;
    private bool sprintHeld;
    private bool crouchHeld;

    private float standingHeight;
    private Vector3 standingCenter;
    private Vector3 standingCameraLocalPos;
    private Vector3 crouchingCenter;
    private Vector3 crouchingCameraLocalPos;

    public bool IsCrouching => crouchHeld;
    public bool IsSprinting => sprintHeld && !crouchHeld;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        standingHeight = characterController.height;
        standingCenter = characterController.center;

        crouchingCenter = standingCenter;
        crouchingCenter.y -= (standingHeight - crouchHeight) * 0.5f;

        if (cameraTransform != null)
        {
            pitch = cameraTransform.localEulerAngles.x;
            if (pitch > 180f)
                pitch -= 360f;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);

            standingCameraLocalPos = cameraTransform.localPosition;
            crouchingCameraLocalPos = standingCameraLocalPos + Vector3.down * (standingHeight - crouchHeight) * 0.5f;
        }

        ApplyControllerSettings();
    }

    private void OnEnable()
    {
        SetupInputActions();

        jumpAction.performed += OnJumpPerformed;
        sprintAction.performed += OnSprintPerformed;
        sprintAction.canceled += OnSprintCanceled;
        crouchAction.performed += OnCrouchPerformed;
        crouchAction.canceled += OnCrouchCanceled;

        if (usesAssetActions)
        {
            playerMap.Enable();
        }
        else
        {
            moveAction.Enable();
            lookAction.Enable();
            jumpAction.Enable();
            sprintAction.Enable();
            crouchAction.Enable();
        }
    }

    private void Start()
    {
        if (lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnDisable()
    {
        if (jumpAction == null)
            return;

        jumpAction.performed -= OnJumpPerformed;
        sprintAction.performed -= OnSprintPerformed;
        sprintAction.canceled -= OnSprintCanceled;
        crouchAction.performed -= OnCrouchPerformed;
        crouchAction.canceled -= OnCrouchCanceled;

        if (usesAssetActions)
        {
            playerMap.Disable();
        }
        else
        {
            moveAction.Disable();
            lookAction.Disable();
            jumpAction.Disable();
            sprintAction.Disable();
            crouchAction.Disable();

            moveAction.Dispose();
            lookAction.Dispose();
            jumpAction.Dispose();
            sprintAction.Dispose();
            crouchAction.Dispose();

            moveAction = null;
            lookAction = null;
            jumpAction = null;
            sprintAction = null;
            crouchAction = null;
        }
    }

    private void Update()
    {
        UpdateView();
        UpdateCharacterBody();
        UpdateMovement();
    }

    private void UpdateView()
    {
        if (lookAction == null)
            return;

        Vector2 lookDelta = lookAction.ReadValue<Vector2>();
        float yawDelta = lookDelta.x * lookSensitivity;
        float pitchDelta = lookDelta.y * lookSensitivity;

        transform.Rotate(0f, yawDelta, 0f, Space.Self);

        if (cameraTransform != null)
        {
            pitch = Mathf.Clamp(pitch - pitchDelta, minPitch, maxPitch);
            cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
        }
    }

    private void UpdateCharacterBody()
    {
        float targetHeight = crouchHeld ? crouchHeight : standingHeight;
        Vector3 targetCenter = crouchHeld ? crouchingCenter : standingCenter;

        characterController.height = Mathf.Lerp(characterController.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        characterController.center = Vector3.Lerp(characterController.center, targetCenter, crouchTransitionSpeed * Time.deltaTime);
        characterController.stepOffset = GetClampedStepOffset(characterController.height);

        if (cameraTransform != null)
        {
            Vector3 targetCamera = crouchHeld ? crouchingCameraLocalPos : standingCameraLocalPos;
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, targetCamera, crouchTransitionSpeed * Time.deltaTime);
        }
    }

    private void UpdateMovement()
    {
        if (moveAction == null)
            return;

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 move = (transform.right * moveInput.x + transform.forward * moveInput.y);
        if (move.sqrMagnitude > 1f)
            move.Normalize();

        float speed = walkSpeed;
        if (crouchHeld)
            speed = crouchSpeed;
        else if (sprintHeld && moveInput.sqrMagnitude > 0.01f)
            speed = sprintSpeed;

        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (jumpRequested && !crouchHeld)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        jumpRequested = false;
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * speed + Vector3.up * verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        jumpRequested = true;
    }

    private void OnSprintPerformed(InputAction.CallbackContext ctx)
    {
        sprintHeld = true;
    }

    private void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        sprintHeld = false;
    }

    private void OnCrouchPerformed(InputAction.CallbackContext ctx)
    {
        crouchHeld = true;
    }

    private void OnCrouchCanceled(InputAction.CallbackContext ctx)
    {
        crouchHeld = false;
    }

    private void SetupInputActions()
    {
        if (inputActions != null)
        {
            usesAssetActions = true;

            playerMap = inputActions.FindActionMap(actionMapName, true);
            moveAction = playerMap.FindAction(moveActionName, true);
            lookAction = playerMap.FindAction(lookActionName, true);
            jumpAction = playerMap.FindAction(jumpActionName, true);
            sprintAction = playerMap.FindAction(sprintActionName, true);
            crouchAction = playerMap.FindAction(crouchActionName, true);

            return;
        }

        usesAssetActions = false;
        playerMap = null;

        moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        var composite = moveAction.AddCompositeBinding("2DVector");
        composite.With("Up", "<Keyboard>/w");
        composite.With("Up", "<Keyboard>/upArrow");
        composite.With("Down", "<Keyboard>/s");
        composite.With("Down", "<Keyboard>/downArrow");
        composite.With("Left", "<Keyboard>/a");
        composite.With("Left", "<Keyboard>/leftArrow");
        composite.With("Right", "<Keyboard>/d");
        composite.With("Right", "<Keyboard>/rightArrow");

        lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta", expectedControlType: "Vector2");

        jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        sprintAction = new InputAction("Sprint", InputActionType.Button);
        sprintAction.AddBinding("<Keyboard>/leftShift");
        sprintAction.AddBinding("<Keyboard>/rightShift");

        crouchAction = new InputAction("Crouch", InputActionType.Button);
        crouchAction.AddBinding("<Keyboard>/leftCtrl");
        crouchAction.AddBinding("<Keyboard>/rightCtrl");
        crouchAction.AddBinding("<Keyboard>/c");
    }

    private void ApplyControllerSettings()
    {
        if (characterController == null)
            return;

        characterController.slopeLimit = slopeLimit;
        characterController.stepOffset = GetClampedStepOffset(characterController.height);
    }

    private float GetClampedStepOffset(float controllerHeight)
    {
        float maxAllowed = Mathf.Max(0f, controllerHeight * 0.5f - 0.01f);
        return Mathf.Clamp(stepOffset, 0f, maxAllowed);
    }
}
