using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the collisions of the "Lance" prefab. Checks for collisions with enemies and collisions
/// with the ground/walls so that the lance turns around early when hitting a wall.
/// TODO: Probably going to have to make it so you can't push and pull the boss once that is in
/// </summary>
public class LanceBehavior : MonoBehaviour
{
    private List<int> collidedEnemies = new List<int>();
    [HideInInspector] public int damage;

    void OnTriggerEnter(Collider other)
    {
        // OnTriggerEnter triggers more than once because it moves so track the enemy InstanceID's and only
        // trigger if the object is an enemy and has not been collided with yet. (layer 6 is the enemy layer)
        if (other.gameObject.layer == 6 && !collidedEnemies.Contains( other.gameObject.GetInstanceID() ))
        {
            // Make the enemy that collides with the lance a child of it so it gets dragged along
            other.transform.parent = gameObject.transform;
            other.GetComponent<Health>().TakeDamage(new DamageParameters(damage, other.gameObject));
            other.GetComponent<Enemy>().WasHit();
            collidedEnemies.Add(other.gameObject.GetInstanceID());
            Debug.Log("Lance hit an enemy");
        }

        // Since OnTriggerEnter is triggered every frame, we can use it as an update function basically
        // and stun the enemy on every frame until the lance has returned to the player
        else if (other.gameObject.layer == 6)
        {
            other.GetComponent<Enemy>().WasHit();
        }
    }
}
