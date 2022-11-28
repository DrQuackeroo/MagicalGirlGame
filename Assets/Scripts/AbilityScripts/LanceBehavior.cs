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
    private bool collidedWithWall = false;
    private List<int> collidedEnemies = new List<int>();
    [SerializeField] private Sprite _umbrellaOpen;
    [HideInInspector] public int damage;

    void OnTriggerEnter(Collider other)
    {
        /*
        // The lance should not push or pull the boss, it should only damage it
        if (other.gameObject.tag == "Boss" && !collidedEnemies.Contains( other.gameObject.GetInstanceID() ))
        {
            other.GetComponent<Health>().TakeDamage(new DamageParameters(damage, other.gameObject));
            other.GetComponent<Enemy>().WasHit();
            collidedEnemies.Add(other.gameObject.GetInstanceID());
            return;
        }
        */

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

        // If the trigger collides with the ground, the lance should turn around immediately with the same velocity
        // (layer 3 is the ground layer)
        if (other.gameObject.layer == 3)
        {
            collidedWithWall = true;
            Debug.Log("Lance hit a wall");
        }
    }

    // Swaps the umbrella sprite to have the umbrella open
    public void OpenUmbrellaSprite()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = _umbrellaOpen;
    }

    // Returns true if the lance has collided with the wall
    public bool CollidedWithWall()
    {
        return collidedWithWall;
    }
}
