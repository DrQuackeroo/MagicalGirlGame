using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttack : MonoBehaviour
{
    private BasicAttackCombo _combo;
    private BasicAttack _nextAttack = null;
    private string _name;
    private int _damage;
    private float _attackRadius;
    private float _windUp;
    private float _windDown;

    public void Initialize(BasicAttackCombo combo, BasicAttack nextAttack, string name, int damage, float attackRadius, float windUp, float windDown)
    {
        _combo = combo;
        _nextAttack = nextAttack;
        _name = name;
        _damage = damage;
        _attackRadius = attackRadius;
        _windUp = windUp;
        _windDown = windDown;
    }

    public IEnumerator Attack()
    {
        yield return new WaitForSeconds(_windUp);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(_combo.transform.position, _attackRadius, _combo._enemyLayers);

        if (colliders.Length > 0)
        {
            foreach (Collider2D c in colliders)
            {
                print(c.gameObject.name);
                // healthComponent = c.GetComponent for health
                // healthComponent.TakeDamage(damage);
            }
        }

        yield return new WaitForSeconds(_windDown);
    }
}
