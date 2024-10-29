using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Hero : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] List<AudioClip> audioClips;
    private Dictionary<string, AudioClip> audioClipDictionary;
    private AudioSource audioSource;
    private float walkSoundMaxTime = .3f;
    private float walkSoundTime = 0;
    [Header("Ground Check")]
    [SerializeField] LayerMask killzoneLayer;
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
    [SerializeField] private float maxSlopeAngle = 45f;
    private bool onSlope = false;
    private Vector2 slopeNormal;
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
    [SerializeField] LayerMask enemyLayer;
    public bool isAttacking = false;
    public bool comboActive = false;
    public bool comboActivated = false;

    [Header("Stats")]
    public int maxHealth = 100;
    public int health;
    private bool isTakingHit = false;
    public bool isDead = false;

    // ---------------------- //
    private Rigidbody2D rb;
    private void Start()
    {
        SetAudioClips();
        health = maxHealth;
        weapon = new Sword();
        rb = GetComponent<Rigidbody2D>();
        GameObject.FindGameObjectWithTag("UI Animation").GetComponent<Animator>().Play("OpenScene");
    }

    private void Update()
    {
    }

    private void FixedUpdate()
    {
        if (!isDead)
        {
            WalkSoundUpdate();
            CheckGrounded();
            ApplyManualGravity();
            OnWall();
            HandleSlopeMovement();
        }

        PlayAnimation();
    }

    #region Audio

    private void SetAudioClips()
    {
        audioSource = GetComponent<AudioSource>();
        audioClipDictionary = new Dictionary<string, AudioClip>();
        foreach (var clip in audioClips)
        {
            audioClipDictionary[clip.name] = clip;
        }
    }

    private void PlayAudio(string name)
    {
        if (audioClipDictionary.TryGetValue(name, out AudioClip clip))
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogError($"No audio clip with name \"{name}\" was found!");
        }
    }

    private void PlayWalkSound()
    {
        if (walkSoundTime <= 0)
        {
            walkSoundTime = walkSoundMaxTime;
            PlayAudio("Walk");
        }
    }

    private void WalkSoundUpdate()
    {
        if(walkSoundTime > 0)
        {
            walkSoundTime -= Time.fixedDeltaTime;
        }
    }

    #endregion

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
            if (IsGrounded())
                PlayWalkSound();

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
                PlayAudio("Jump");
                rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
                airState = AirState.Jumping;
            }
            else if (airState == AirState.Jumping)
            {
                PlayAudio("Jump");
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
            PlayAudio("Jump");
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
        if (Physics2D.OverlapCircle(groundCheck.position, 0.01f, killzoneLayer))
        {
            Die();
            return;
        }

        Collider2D jumpOffEnemy = Physics2D.OverlapCircle(groundCheck.position, 0.001f, enemyLayer);

        if (jumpOffEnemy != null)
        {
            rb.velocityY = 0f;
            rb.AddForce(transform.up * jumpForce / 2, ForceMode2D.Impulse);
            airState = AirState.Jumping;
            // this is too overpowered, and this game isn't going to be on a large enough scale to solve this problem. For now, take no damage if from jumping off of enemy heads
            jumpOffEnemy.GetComponentInParent<Enemy>().TakeDamage(0);
            PlayAudio("Land");
            return;
        }

        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundOffset, groundLayer);

        if (isGrounded)
        {
            airState = AirState.Grounded;
            DetectSlope();
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

    private void DetectSlope()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundOffset + 0.5f, groundLayer);

        if (hit)
        {
            slopeNormal = hit.normal;
            float slopeAngle = Vector2.Angle(slopeNormal, Vector2.up);

            if (slopeAngle > 10f && slopeAngle <= maxSlopeAngle)
            {
                onSlope = true;
            }
            else
            {
                onSlope = false;
            }
        }
        else
        {
            onSlope = false;
        }
    }

    private void HandleSlopeMovement()
    {
        if (IsGrounded() && onSlope)
        {
            Vector2 slopeDirection = Vector2.Perpendicular(slopeNormal).normalized;
            float moveDirection = facingRight ? -1 : 1;
            float force = groundForce / 2;
            
            Vector2 moveForce = force * moveDirection * slopeDirection;
            rb.AddForce(moveForce, ForceMode2D.Force);
            if (rb.velocity.y < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }
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
            if (wallState != WallState.OnWall) PlayAudio("OnWall");
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
        PlayAudio("Attack");
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
        PlayAudio("PlayerGetHit");
        EndCombo();
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

    #region Scene

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Finish Game") && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            StartCoroutine(SceneController.LoadNextScene());
        }
    }
    private void ReloadScene()
    {
        StartCoroutine(SceneController.ReloadScene());
    }

    #endregion

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
