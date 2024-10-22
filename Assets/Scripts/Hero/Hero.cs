using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Hero : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundOffset = 0.1f;
    public AirState airState;

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

    [Header("Combat")]
    public Weapon weapon;
    [SerializeField] GameObject hitBox;
    public bool isAttacking = false;
    public bool comboActive = false;
    public bool comboActivated = false;
    [Header("Stats")]
    private int maxHealth = 100;
    private int health;
    private bool isTakingHit = false;
    public bool isDead = false;

    // ---------------------- //
    private Rigidbody2D rb;
    private void Start()
    {
        health = maxHealth;
        weapon = new Sword();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
    }

    private void FixedUpdate()
    {
        if (!isDead)
        {
            CheckGrounded();
            ApplyManualGravity();
            OnWall();
        }

        PlayAnimation();
    }

    #region Movement

    public bool IsMoving()
    {
        return rb.velocity.magnitude > 0.1f;
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

    #endregion

    #region Combat 

    public void LightAttack()
    {
        
        hitBox.SetActive(true);
        hitBox.GetComponent<BoxCollider2D>().enabled = true;
    }

    private void ResetAttack()
    {
        comboActive = true;
        hitBox.SetActive(false);
        hitBox.GetComponent<BoxCollider2D>().enabled = false;
    }

    private void EndAttack()
    {
        comboActivated = false;
        comboActive = false;
        isAttacking = false;
    }

    private void EndCombo()
    {
        hitBox.SetActive(false);
        hitBox.GetComponent<BoxCollider2D>().enabled = false;
        comboActive = false;
        comboActivated = false;
        isAttacking = false;
    }

    private void FailCombo()
    {
        comboActive = false;
    }

    #endregion

    public void TakeDamage(int _damage)
    {
        health -= _damage;
        if (health > 0)
        {
            isTakingHit = true;
        }
        else
        {
            Die();
        }
    }

    private void ResetTakingDamage()
    {
        isTakingHit = false;
    }

    private void Die()
    {
        isDead = true;
    }

    private void ReloadScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    #region Animation

    private void PlayAnimation()
    {
        Animator animator = GetComponent<Animator>();

        if (isDead)
        {
            animator.CrossFade("Die", 0.01f);
            return;
        }

        if (isTakingHit)
        {
            animator.CrossFade("TakeHit", 0.01f);
            return;
        }

        if (isAttacking)
        {
            if (comboActive == false)
            {
                animator.CrossFade("Attack1", 0.01f);
            }
            else if (comboActive == true && comboActivated == true)
            {
                animator.CrossFade("Attack2", 0.01f);
            }
            return;
        }

        if (IsGrounded() && rb.velocity.magnitude > 0.75)
        {
            animator.CrossFade("Run", 0.01f);
        }
        else if (!IsGrounded() && wallState == WallState.OnWall) 
        {
            animator.CrossFade("OnWall", 0.01f);
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

    #endregion

    public enum AirState
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
