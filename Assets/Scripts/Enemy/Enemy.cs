using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    protected virtual int MaxHealth { get; set; } = 100;
    private int currentHealth;
    protected EnemyState enemyState;
    protected EnemyState lastenemyState;
    protected CapsuleCollider2D collider2D;
    protected Rigidbody2D rb;
    void Start()
    {
        enemyState = EnemyState.Idle;
        currentHealth = MaxHealth;
        collider2D = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (enemyState == EnemyState.Idle) {
            PlayAnimation("Idle");
        }
    }

    public virtual void TakeDamage(int _damage) // public class instead of protected so that the Hitbox script can access it
    {
        enemyState = EnemyState.TakingDamage;
        if (currentHealth > 0)
        {
            currentHealth -= _damage;
            PlayAnimation("TakeHit");
        } else {
            Die();
        }
    }

    protected virtual void Die()
    {
        PlayAnimation("Die");
        rb.isKinematic = true;
        collider2D.enabled = false;
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

    protected enum EnemyState
    {
        Idle,
        TakingDamage,
        Attacking,
        Dead
    }
}
