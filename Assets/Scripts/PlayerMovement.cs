using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;
    bool hasAirJumped;

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

    public Transform orientation;
    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

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

        // apply drag & reset air jump
        if (grounded)
        {
            rb.drag = groundDrag;
            hasAirJumped = false;
        }
        else
            rb.drag = 0;
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
        else if (Input.GetKey(jumpKey) && readyToJump && !hasAirJumped)
        {
            readyToJump = false;
            hasAirJumped = true;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // dash
        if (Input.GetKeyDown(KeyCode.LeftShift)) StartCoroutine(Dash());
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if (!grounded) // in air
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

    }

    private void SpeedControl()
    {
        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 vertVelocity = new Vector3(0f, rb.velocity.y, 0f);

        // limit flat velocity
        if (flatVelocity.magnitude > moveSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }

        // limit vertical velocity
        if (vertVelocity.magnitude > moveSpeed)
        {
            Vector3 limitedVelocity = vertVelocity.normalized * moveSpeed;
            rb.velocity = new Vector3(rb.velocity.x, limitedVelocity.y, rb.velocity.z);
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
            rb.AddForce(moveDirection.normalized * moveSpeed, ForceMode.Impulse);

            yield return null;
        }
    }
}