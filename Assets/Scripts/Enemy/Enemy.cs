using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    protected virtual int maxHealth { get; set; } = 100;
    private int currentHealth;
    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        
    }

    public virtual void TakeDamage(int _damage) // public class instead of protected so that the Hitbox script can access it
    {
        if (currentHealth > 0)
        {
            currentHealth -= _damage;
        } else {
            Die();
        }
    }

    protected virtual void Die()
    {
        gameObject.SetActive(false);
    }
}
