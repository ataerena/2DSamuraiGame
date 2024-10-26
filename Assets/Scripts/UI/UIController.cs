using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] Hero hero;

    [Header("Fields")]
    [SerializeField] TextMeshProUGUI health;
    [SerializeField] TextMeshProUGUI enemyCount;

    private void Update()
    {
        if (hero != null)
        {
            health.text = hero.health.ToString();
        }

        enemyCount.text = GameObject.FindGameObjectsWithTag("Enemy").Length.ToString();
    }
}
