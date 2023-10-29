using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 10f;
    public float gravity = -19.62f;
    public float jumpHeight = 1.5f;
    public float dashSpeed;
    public float dashTime;
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float grapplingCoolDown;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public Transform cam;
    public Transform gunTip;
    public LayerMask groundMask;
    public LayerMask grappleMask;
    public KeyCode grappleKey = KeyCode.Mouse1;
    public LineRenderer lineRenderer;

    private Vector3 velocity;
    private Vector3 grapplePoint;
    private float grapplingCoolDownTimer;
    private bool isGrounded;
    private bool hasAirJumped = false;
    private bool isGrappling;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // variable init
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if(isGrounded)
        {
            hasAirJumped = false;
        }

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // WASD Movement
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        // DoubleJump
        if(Input.GetButtonDown("Jump") && (isGrounded || !hasAirJumped)) StartCoroutine(DoubleJump());

        // Dash
        if(Input.GetKeyDown(KeyCode.LeftShift)) StartCoroutine(Dash());

        // Grapple
        if(grapplingCoolDownTimer > 0) grapplingCoolDownTimer -= Time.deltaTime;
        if(Input.GetKeyDown(grappleKey)) StartGrapple();
    }

    private void LateUpdate()
    {
        if(isGrappling)
        {
            lineRenderer.SetPosition(0, gunTip.position);
        }
    }

    IEnumerator DoubleJump()
    {
        if(!isGrounded)
        {
            hasAirJumped = true;
        }
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        yield return null;
    }

    IEnumerator Dash()
    {
        float startTime = Time.time;

        while(Time.time < startTime + dashTime)
        {
            controller.Move(transform.forward * dashSpeed * Time.deltaTime);

            yield return null;
        }
    }

    private void StartGrapple()
    {
        if(grapplingCoolDownTimer > 0) return;

        isGrappling = true;

        RaycastHit hit;
        if(Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, grappleMask))
        {
            grapplePoint = hit.point;

            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;

            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(1, grapplePoint);
    }

    private void ExecuteGrapple()
    {
    }

    private void StopGrapple()
    {
        isGrappling = false;

        grapplingCoolDownTimer = grapplingCoolDown;

        lineRenderer.enabled = false;
    }
}
