using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Enemy : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] RectTransform healthBarFill;
    protected virtual int MaxHealth { get; set; } = 100;
    private int currentHealth;
    protected EnemyState enemyState;
    protected CapsuleCollider2D cldr2D;
    protected Rigidbody2D rb;
    protected virtual float moveSpeed { get; set; } = 1f;
    private bool movingRight;
    [SerializeField] LayerMask playerLayer;
    protected float ChaseRange { get; set; } = 5f;
    private bool canAttack = false;
    [SerializeField] EnemyHitbox EnemyHitbox;
    public virtual int damage { get; set; } = 25;
    void Awake()
    {
        enemyState = EnemyState.Idle;
        currentHealth = MaxHealth;
        cldr2D = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        UpdateHealthBar();
    }

    protected virtual void UpdateHealthBar()
    {
        float fillAmount = 300 * currentHealth / MaxHealth;
        healthBarFill.sizeDelta = new Vector2(fillAmount, healthBarFill.sizeDelta.y);
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

        float verticalRange = ChaseRange + ChaseRange * 0.5f;
        Vector2 size = new Vector2(verticalRange, ChaseRange);

        Collider2D detectedHero = Physics2D.OverlapBox(center, size, 0f, playerLayer);
        
        Transform hero;
        if (detectedHero != null)
        {
            if (detectedHero.gameObject.GetComponent<Hero>().isDead) return;
            
            hero = detectedHero.gameObject.GetComponent<Transform>();
            if (hero.position.x < center.x) movingRight = true;
            else if (hero.position.x > center.x) movingRight = false;
            Flip();
        }
        else
        {
            ResetToIdle();
            return; 
        }

        UpdateCanAttack(hero);

        Vector2 movePoint = new Vector2(facingRight ? hero.position.x - 1f : hero.position.x + 1f, hero.position.y);
        if (Vector2.Distance(movePoint, transform.position) < 0.5f)
        {
            return;
        }

        enemyState = EnemyState.Running;
        transform.position = Vector2.MoveTowards(transform.position, movePoint, moveSpeed * Time.fixedDeltaTime);
    }

    protected virtual void UpdateCanAttack(Transform hero)
    {
        Vector2 movePoint = new Vector2(CheckIsFacingRight() ? hero.position.x - 1.5f : hero.position.x + 1.5f, hero.position.y);
        canAttack = Vector2.Distance(movePoint, transform.position) < 0.5f;

        if (!canAttack) return;
        else
        {
            Attack();
        }
    }

    protected virtual void Attack()
    {
        enemyState = EnemyState.Attacking;
    }

    protected virtual void ConnectAttack()
    {
        EnemyHitbox.gameObject.SetActive(true);
        EnemyHitbox.gameObject.GetComponent<BoxCollider2D>().enabled = true;
    }

    protected virtual void ResetAttack()
    {
        enemyState = EnemyState.Idle;
        EnemyHitbox.gameObject.SetActive(false);
        EnemyHitbox.gameObject.GetComponent<BoxCollider2D>().enabled = false;
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
        currentHealth -= _damage;
        if (currentHealth > 0)
        {
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
        else if (enemyState == EnemyState.Attacking)
        {
            PlayAnimation("Attack");
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
        return enemyState != EnemyState.Dead && enemyState != EnemyState.TakingDamage && enemyState != EnemyState.Attacking;
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
