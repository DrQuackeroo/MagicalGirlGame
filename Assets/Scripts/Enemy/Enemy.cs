using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls basic Enemy behavior. Changes states stored in an AnimatorController, which acts as a state machine.
/// </summary>
public class Enemy : MonoBehaviour
{
    // Tag used only by the Player
    const string playerTag = "Player";
    // Minimum x-axis speed Enemy needs to be moving at to change sprite facing direction. Causes jittery flipping if too low.
    const float flipSpeedThreshold = 0.85f;

    // Range at which Enemy sees the player and starts moving towards them.
    [SerializeField] protected float _playerDetectionRange = 20.0f;
    [SerializeField] protected float _movementSpeed = 5.0f;
    // Waypoints for Enemy patrol path.
    [SerializeField] protected List<GameObject> _patrolWaypoints = new List<GameObject>();

    protected GameObject _player;
    protected Rigidbody _rigidbody;
    protected SpriteRenderer _spriteRenderer;
    protected NavMeshAgent _navMeshAgent;
    protected Animator _animator;
    protected int _currentWaypointIndex = 0;

    // Animation variables need to be hashed before they can be set in code.
    protected readonly int _hashPlayerWithinRange = Animator.StringToHash("PlayerWithinRange");

    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.FindWithTag(playerTag);
        _rigidbody = GetComponent<Rigidbody>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // Patrolling Enemy has detected Player
        if (Vector3.Distance(_player.transform.position, transform.position) <= _playerDetectionRange)
        {
            _animator.SetTrigger(_hashPlayerWithinRange);
        }


        // Lock rotation
        transform.rotation = Quaternion.Euler(0, 0, 0);

        // Flip sprite if speed is above a threshold
        if (Mathf.Abs(_navMeshAgent.velocity.x) > flipSpeedThreshold)
        {
            _spriteRenderer.flipX = _navMeshAgent.velocity.x < 0.0f;
        }

        // Prevents Enemies from overlapping each other due to navigation in 3D space.
        transform.Translate(0, 0, -transform.position.z);

        // NavMeshAgent needs to be disabled and Rigidbody.isKinematic needs to be set to false for physics-based forces to be applied.
        // Reverse the process to return to NavMesh movement.
        // For example, if the Enemy is launched by a Player attack, Enemy movement would switch to being controlled by physics.
        // https://docs.unity3d.com/Manual/nav-MixingComponents.html for more details.

        //GetComponent<NavMeshAgent>().enabled = false;
        //GetComponent<Rigidbody>().isKinematic = false;
        //GetComponent<Rigidbody>().AddForce(Vector3.up * 1000.0f);
    }

    /// <summary>
    /// Returns position of next waypoint in a series of waypoints that this Enemy is patrolling between. Resets to first waypoint once all have been returned.
    /// </summary>
    /// <returns>Next waypoint in list or transform.position if no waypoints set.</returns>
    public Vector3 GetNextWaypoint()
    {
        if (_patrolWaypoints.Count == 0)
        {
            return transform.position;
        }

        _currentWaypointIndex = (_currentWaypointIndex + 1) % _patrolWaypoints.Count;
        return _patrolWaypoints[_currentWaypointIndex].transform.position;
    }
}
