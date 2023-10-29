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
    public float overshootYAxis;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public Transform cam;
    public Transform gunTip;
    public LayerMask groundMask;
    public LayerMask grappleMask;
    public KeyCode grappleKey = KeyCode.Mouse1;
    public LineRenderer lineRenderer;
    public Rigidbody rigidBody;

    private Vector3 velocity;
    private Vector3 grapplePoint;
    private float grapplingCoolDownTimer;
    private bool isGrounded;
    private bool hasAirJumped = false;
    private bool isGrappling;
    private bool isFrozen;

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

        if(isGrounded && isGrappling) rigidBody.drag = 0;
        if(isGrappling) return;

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if(isFrozen)
        {
            rigidBody.velocity = Vector3.zero;
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

    public void StartGrapple()
    {
        if(grapplingCoolDownTimer > 0) return;

        isGrappling = true;

        isFrozen = true;

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
        isFrozen = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y -1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if(grapplePointRelativeYPos < 0)
        {
            highestPointOnArc = overshootYAxis;
        }

        JumpToPosition(grapplePoint, highestPointOnArc);

        Invoke(nameof(StopGrapple), 1f);
    }

    private void StopGrapple()
    {
        isFrozen = false;

        isGrappling = false;

        grapplingCoolDownTimer = grapplingCoolDown;

        lineRenderer.enabled = false;
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
        // isGrappling = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);
    }

    private Vector3 velocityToSet;
    private void SetVelocity()
    {
        rigidBody.velocity = velocityToSet;
    }
}
