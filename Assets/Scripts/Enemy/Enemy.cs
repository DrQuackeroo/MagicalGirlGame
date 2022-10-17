using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Controls basic Enemy behavior
/// </summary>
public class Enemy : MonoBehaviour
{
    // Tag used only by the Player
    const string playerTag = "Player";

    // Range at which Enemy sees the player and starts moving towards them.
    [SerializeField] private float playerDetectionRange = 20.0f;
    [SerializeField] private float movementSpeed = 5.0f;

    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag(playerTag);
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(player.transform.position, transform.position) <= playerDetectionRange)
        {
            Debug.Log("Within range of player");
            //transform.Translate((player.transform.position - transform.position).normalized * movementSpeed * Time.deltaTime);
            GetComponent<Rigidbody2D>().MovePosition(transform.position + ((player.transform.position - transform.position).normalized * movementSpeed * Time.deltaTime));
        }
    }
}
