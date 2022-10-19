using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls basic Enemy behavior
/// 
/// Ideas:
/// Naive movement algorithm: move left or right, depending on the Player's position relative to the Enemy. Stop when at a certain distance to the Player.
///     Problem: What if the Player is on a platform above/below the Enemy? Enemies will group up on their platform but won't navigate towards the Player.
///         Enemy above Player: Compare naive direction with A* direction limited to x-axis. If they are different directions, choose the A* direction.
///             If Enemy is above the Player and both algorithms produce the same direction, it is possible Enemy is on a slope that leads to the Player without falling.
///             If different directions, A* will lead Enemy off their current platform and down to the Player.
///         Enemy below Player: Harder problem. Would require jumping up to reach the Player.
///     Problem: What if there is a pit between the Enemy and the Player? Enemies will fall into the pit.
///         Could use ledge detection, but then the Enemy won't be able to drop down off safe platforms.
/// </summary>
public class Enemy : MonoBehaviour
{
    // Tag used only by the Player
    const string playerTag = "Player";
    // Minimum x-axis speed Enemy needs to be moving at to change sprite facing direction. Causes jittery flipping if too low.
    const float flipSpeedThreshold = 0.125f;

    // Range at which Enemy sees the player and starts moving towards them.
    [SerializeField] protected float _playerDetectionRange = 20.0f;
    [SerializeField] protected float _movementSpeed = 5.0f;
    // Waypoints for Enemy patrol path.
    [SerializeField] protected List<GameObject> _patrolWaypoints = new List<GameObject>();

    protected GameObject _player;
    protected Rigidbody _rigidbody; // TODO: Testing 3D vs 2D rigidbody. Remove one after deciding.
    protected Rigidbody2D _rigidbody2D;
    protected SpriteRenderer _spriteRenderer;
    protected NavMeshAgent _navMeshAgent;
    protected int _currentWaypointIndex = 0;

    // The direction this Enemy will move this FixedUpdate() call
    protected Vector2 _moveDirection;

    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.FindWithTag(playerTag);
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // TODO: Remove on cleanup

        // Basic move to player
        //if (Vector3.Distance(_player.transform.position, transform.position) <= _playerDetectionRange)
        //{
        //    Debug.Log("Within range of player");
        //    //transform.Translate((_player.transform.position - transform.position).normalized * _movementSpeed * Time.deltaTime);
        //    _rigidbody2D.MovePosition(transform.position + ((_player.transform.position - transform.position).normalized * _movementSpeed * Time.deltaTime));
            
        //}

        //// Naive move to Player: check if Player is left or right, then move in that direction
        //if (_player.transform.position.x < transform.position.x)
        //{
        //    _moveDirection = Vector2.left;
        //}
        //else
        //{
        //    _moveDirection = Vector2.right;
        //}

        //_rigidbody2D.MovePosition(_rigidbody2D.position + _moveDirection * _movementSpeed * Time.fixedDeltaTime);

        //// Setting velocity directly
        //Vector3 targetVelocity = new Vector2(_movementSpeed * _moveDirection.x * 1.0f, _rigidbody2D.velocity.y);
        //_rigidbody2D.velocity = targetVelocity;//Vector3.SmoothDamp(_rigidbody2D.velocity, targetVelocity, ref _currentVelocity, 0.025f);

        // Physics.gravity.y
        // _rigidbody2D.add
    }

    void Update()
    {
        //// TODO: Use later for changing states.
        //if (Vector3.Distance(_player.transform.position, transform.position) <= _playerDetectionRange)
        //    _navMeshAgent.SetDestination(_player.transform.position);


        // lock rotation
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
    /// Returns position of next waypoint in a series of waypoints that this Enemy is patrolling between. Resets to waypoint index 0 once all have been returned.
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
