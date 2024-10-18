using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundOffset = 0.1f;
    private AirState airState;

    [Header("Movement")]
    [SerializeField] float groundForce = 90f;
    [SerializeField] float airForce = 70f;
    [SerializeField] float jumpForce = 50f;
    [SerializeField] float gravity = 100f;
    [SerializeField] float fallSpeedThreshold = 100f;
    private bool facingRight = true;

    [Header("Wall")]
    [SerializeField] Transform wallCheck;
    [SerializeField] float offWallForceX = 40f;
    [SerializeField] float offWallForceY = 40f;
    [SerializeField] float maxWallTime = 1f;
    private float wallTime = 0f;
    private WallState wallState;

    // ---------------------- //
    private Rigidbody2D rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        ApplyManualGravity();
        OnWall();
        PlayAnimation();
    }

    public void Move(float moveX)
    {
        Vector2 right = transform.right;
        right.Normalize();

        if (moveX != 0 && rb.velocity.magnitude < 10f)
        {
            Vector2 force = (IsGrounded() ? groundForce : airForce) * (moveX > 0 ? 1 : -1) * moveX * right;
            rb.AddForce(force, ForceMode2D.Force);
            RotateHero(moveX);
        }
    }

    public void Jump()
    {
        if (wallState == WallState.None)
        {
            if (IsGrounded())
            {
                rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
                airState = AirState.Jumping;
            }
            else if (airState == AirState.Jumping)
            {
                rb.velocityY = 0;
                rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
                airState = AirState.DoubleJumping;
            }
        }
        else
        {
            WallJump();
        }
    }

    private void WallJump()
    {
        Vector2 wallJumpForce;
        if (wallState == WallState.OnWall)
        {
            RotateHero(facingRight ? -1f : 1f);
            wallJumpForce = new Vector2(offWallForceX * transform.right.x, offWallForceY);
            airState = AirState.Jumping;
            wallState = WallState.None;
            rb.AddForce(wallJumpForce, ForceMode2D.Impulse);
        }
    }

    private void RotateHero(float moveX)
    {
        if (moveX > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            facingRight = true;
        }
        else if (moveX < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            facingRight = false;
        }
    }

    private void CheckGrounded()
    {
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundOffset, groundLayer);

        if (isGrounded)
        {
            airState = AirState.Grounded;
            ResetWallState();
        }
        else if (!(airState == AirState.Jumping || airState == AirState.DoubleJumping))
        {
            airState = AirState.Jumping;
        }

        if (!isGrounded)
        {
            CheckOnWall();
        }
    }

    private bool IsGrounded()
    {
        return airState == AirState.Grounded;
    }

    private void ApplyManualGravity()
    {
        if (wallState == WallState.None)
        {
            if (rb.velocity.y < 0 && rb.velocity.y > -fallSpeedThreshold && airState != AirState.Grounded)
            {
                Vector3 downForce = new Vector3(0, -gravity, 0);
                rb.AddForce(downForce, ForceMode2D.Force);
            }
        }
        else 
        {
            rb.AddForce(new Vector3(0, -20f, 0), ForceMode2D.Force);
        }
    }

    private void CheckOnWall()
    {
        if (wallTime < 0f) return;

        bool onWall = Physics2D.OverlapCircle(wallCheck.position, groundOffset, groundLayer);

        if (onWall)
        {
            wallState = WallState.OnWall;
        }
        else
        {
            ResetWallState();
        }
    }

    private void OnWall()
    {
        if (wallState != WallState.None)
        {
            rb.velocity = new Vector3(0, 0, 0);

            if (wallTime <= 0)
            {
                wallTime = maxWallTime;
            }

            if (wallTime > 0)
            {
                wallTime -= Time.fixedDeltaTime;
            }

            if (wallTime <= 0)
            {
                wallState = WallState.None;
            }
        }
    }

    private void ResetWallState()
    {
        wallState = WallState.None;
        wallTime = 0f;
    }

    private void PlayAnimation()
    {
        Animator animator = GetComponent<Animator>();
        if (IsGrounded() && rb.velocity.magnitude > 0.75)
        {
            animator.CrossFade("Run", 0.01f);
        } 
        else if (!IsGrounded() && rb.velocity.y > 0)
        {
            animator.CrossFade("Rise", 0.01f);
        }
        else if (!IsGrounded() && rb.velocity.y < -0)
        {
            animator.CrossFade("Fall", 0.01f);
        }
        else
        {
            animator.CrossFade("Idle", 0.01f);
        }
    }

    private enum AirState
    {
        Grounded,
        Jumping,
        DoubleJumping
    }

    private enum WallState
    {
        None,
        OnWall,
        OnCeiling
    }
}
