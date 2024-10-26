using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] Hero hero;

    [Header("Fields")]
    [SerializeField] RectTransform health;
    [SerializeField] TextMeshProUGUI enemyCount;

    private void Update()
    {
        if (hero != null)
        {
            health.sizeDelta = new Vector2 (100 * hero.health / hero.maxHealth, health.sizeDelta.y);
        }

        enemyCount.text = GameObject.FindGameObjectsWithTag("Enemy").Length.ToString();
    }
}
