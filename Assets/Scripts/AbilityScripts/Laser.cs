// Sourced from https://docs.unity3d.com/ScriptReference/Physics.OverlapBox.html, modified


//Attach this script to your GameObject. This GameObject doesn’t need to have a Collider component
//Set the Layer Mask field in the Inspector to the layer you would like to see collisions in (set to Everything if you are unsure).
//Create a second Gameobject for testing collisions. Make sure your GameObject has a Collider component (if it doesn’t, click on the Add Component button in the GameObject’s Inspector, and go to Physics>Box Collider).
//Place it so it is overlapping your other GameObject.
//Press Play to see the console output the name of your second GameObject

//This script uses the OverlapBox that creates an invisible Box Collider that detects multiple collisions with other colliders. The OverlapBox in this case is the same size and position as the GameObject you attach it to (acting as a replacement for the BoxCollider component).

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public LayerMask m_LayerMask;
    private SpriteRenderer spriteRenderer;

    private GameObject _owner;
    [SerializeField] private int _damage;


    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(GameObject owner)
    {
        _owner = owner;
    }


    void FixedUpdate()
    {

        if (spriteRenderer.enabled)
            MyCollisions();
    }

    void MyCollisions()
    {

        List<Collider> uniqueEnemyColliders = new List<Collider>();
        Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position, transform.localScale / 2, Quaternion.identity, m_LayerMask);


        for (int i = 0; i < hitColliders.Length; i++)
        {
            Debug.Log("Hit : " + hitColliders[i].name + i);
            if (!uniqueEnemyColliders.Contains(hitColliders[i]))
            {
                uniqueEnemyColliders.Add(hitColliders[i]);
                Health enemyHealth = hitColliders[i].GetComponent<Health>();

                if (enemyHealth != null)
                    enemyHealth.TakeDamage(_damage, _owner);
            }

        }




    }

    //Draw the Box Overlap as a gizmo to show where it currently is testing. Click the Gizmos button to see this
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
        //if (m_Started)
        //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
