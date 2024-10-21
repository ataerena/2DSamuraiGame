using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    protected virtual int MaxHealth { get; set; } = 100;
    private int currentHealth;
    protected EnemyState enemyState;
    protected EnemyState lastenemyState;
    protected CapsuleCollider2D cldr2D;
    protected Rigidbody2D rb;
    protected virtual float moveSpeed { get; set; } = 1f;
    private bool movingRight;
    [SerializeField] Transform hero;
    [SerializeField] LayerMask playerLayer;
    private float chaseRange { get; set; } = 5f;
    void Start()
    {
        enemyState = EnemyState.Idle;
        currentHealth = MaxHealth;
        cldr2D = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        DetectHero();
        UpdateAnimation();
    }

    protected virtual void DetectHero()
    {
        if (!CanDetectHero()) return;

        bool facingRight = CheckIsFacingRight();
        Vector2 movePoint = facingRight ? new Vector2(hero.position.x - 1f, hero.position.y) : new Vector2(hero.position.x + 1f, hero.position.y);

        bool heroDetected = Physics2D.Raycast(transform.position, facingRight ? Vector3.right : Vector3.left, 7.5f, playerLayer);
        if (heroDetected && transform.position.x != movePoint.x)
        {
            Debug.Log("LESS THAN!");
            enemyState = EnemyState.Running;
            transform.position = Vector2.MoveTowards(transform.position, movePoint, moveSpeed * Time.deltaTime);
        }
        else
        {
            /* Debug.Log("Hero is NOT in range!"); */
            ResetToIdle();
        }
    }

    protected virtual bool CheckIsFacingRight()
    {
        if (transform.rotation == Quaternion.Euler(0, 0, 0)) 
        {
            return true;
        }
        else
        {
            movingRight = false;
            return false;
        }
    }

    protected virtual void Flip()
    {
        movingRight = !movingRight;
        transform.rotation = Quaternion.Euler(0, movingRight ? 0 : 180, 0);
    }

    public virtual void TakeDamage(int _damage) // public class instead of protected so that the Hitbox script can access it
    {
        enemyState = EnemyState.TakingDamage;
        if (currentHealth > 0)
        {
            currentHealth -= _damage;
            PlayAnimation("TakeHit");
        } 
        else 
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        PlayAnimation("Die");
        rb.isKinematic = true;
        cldr2D.enabled = false;
    }

    protected virtual void UpdateAnimation()
    {
        if (enemyState == EnemyState.Idle) 
        {
            PlayAnimation("Idle");
        }
        else if (enemyState == EnemyState.Running)
        {
            PlayAnimation("Run");
        }
    }

    protected virtual void PlayAnimation(string _name)
    {
        GetComponent<Animator>().CrossFade(_name, 0.01f);
    }

    protected virtual void DeleteGO()
    {
        Destroy(gameObject);
    }

    protected virtual void ResetToIdle()
    {
        enemyState = EnemyState.Idle;
    }

    protected virtual bool CanDetectHero()
    {
        return enemyState == EnemyState.Idle || enemyState == EnemyState.Running;
    }

    protected enum EnemyState
    {
        Idle,
        TakingDamage,
        Attacking,
        Running,
        Dead
    }
}
