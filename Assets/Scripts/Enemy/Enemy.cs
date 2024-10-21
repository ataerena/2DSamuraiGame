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
    [SerializeField] LayerMask playerLayer;
    private float chaseRange { get; set; } = 5f;
    void Start()
    {
        enemyState = EnemyState.Idle;
        currentHealth = MaxHealth;
        cldr2D = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        DetectHero();
        UpdateAnimation();
    }

    protected virtual void DetectHero()
    {
        if (!CanDetectHero()) return;

        bool facingRight = CheckIsFacingRight();
        
        Vector2 center = new Vector2(transform.position.x, transform.position.y);
        Vector2 size = new Vector2(8f, 5f);

        Collider2D detectedHero = Physics2D.OverlapBox(center, size, 0f, playerLayer);
        
        Transform hero;
        if (detectedHero != null)
        {
            hero = detectedHero.gameObject.GetComponent<Transform>();
        }
        else
        {
            ResetToIdle();
            return; 
        }   

        Vector2 movePoint = new Vector2(facingRight ? hero.position.x - 1.5f : hero.position.x + 1.5f, hero.position.y);
        if (Vector2.Distance(movePoint, transform.position) < 0.5f)
        {
            ResetToIdle();
            return;
        }

        enemyState = EnemyState.Running;
        transform.position = Vector2.MoveTowards(transform.position, movePoint, moveSpeed * Time.fixedDeltaTime);
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
        Destroy(rb);
        Destroy(cldr2D);
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
        Dead,
    }
}
