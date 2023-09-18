using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldBehaviour : MeleeWeaponBehaviour
{
    List<GameObject> markedEnemies;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        markedEnemies = new List<GameObject>();
    }

    protected override void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Enemy") && !markedEnemies.Contains(col.gameObject))
        {
            EnemyStats enemy = col.GetComponent<EnemyStats>();
            enemy.TakeDamage(currentDamage);
            
            markedEnemies.Add(col.gameObject); //mark the enemy so that it doesn't take another instance of damage from shield
        }
    }
}