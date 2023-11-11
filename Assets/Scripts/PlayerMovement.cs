using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float walkSpeed;
    public float wallrunSpeed;
    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public float dashCoefficient;
    bool readyToJump;
    bool hasAirJumped;
    public bool isFrozen;
    bool activeGrapple;
    bool enableMovementOnNextTouch;


    [Header("Dash")]
    public float dashTime;

    [Header("Key Bindings")]
    public KeyCode jumpKey = KeyCode.Space;


    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask groundLayerMask;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    bool grounded;
    public bool wallrunning;

    public Transform orientation;
    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    private Vector3 velocityToSet;


    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        wallrunning,
        air
    }

    private void StateHandler()
    {
        if (wallrunning)
        {
            state = MovementState.wallrunning;
            moveSpeed = wallrunSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        readyToJump = true;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        // ground check via raycast
        // grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f * 0.2f, groundLayerMask);
        grounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayerMask);

        PlayerInput();
        SpeedControl();
        StateHandler();

        // apply drag & reset air jump
        if (grounded && !activeGrapple)
        {
            rb.drag = groundDrag;
            hasAirJumped = false;
        }
        else
            rb.drag = 0;

        if (isFrozen)
        {
            rb.velocity = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // check for jumping
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
        // air jump
        else if (Input.GetKey(jumpKey) && readyToJump && !hasAirJumped)
        {
            readyToJump = false;
            hasAirJumped = true;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // dash
        if (Input.GetKeyDown(KeyCode.Q)) StartCoroutine(Dash());
    }

    private void MovePlayer()
    {
        if (activeGrapple) return;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if (!grounded) // in air
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

    }

    private void SpeedControl()
    {
        if (activeGrapple) return;

        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit flat velocity
        if (flatVelocity.magnitude > moveSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }

    private void Jump()
    {
        // set fixed y velocity to ensure same jump height
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    IEnumerator Dash()
    {
        float startTime = Time.time;

        while (Time.time < startTime + dashTime)
        {
            rb.AddForce(moveDirection.normalized * dashCoefficient, ForceMode.Impulse);

            yield return null;
        }
    }

    private Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestictions), 3f);
    }
    
    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.velocity = velocityToSet;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestictions();

            GetComponent<Grappling>().StopGrapple();
        }
    }

    public void ResetRestictions()
    {
        activeGrapple = false;
    }
}
