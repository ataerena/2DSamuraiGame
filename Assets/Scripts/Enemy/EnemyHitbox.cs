using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    [SerializeField] Hero heroPlayer;
    [SerializeField] Enemy attachedEnemy;
    void Awake()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Hero>().TakeDamage(attachedEnemy.damage);
        }
    }
}
