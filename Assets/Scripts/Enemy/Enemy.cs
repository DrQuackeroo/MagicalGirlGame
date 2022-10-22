using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls basic Enemy behavior. Changes states stored in an AnimatorController, which acts as a state machine.
/// 
/// Enemy attacking code flow: StateMachineBehavior calls Enemy.Attack(), Enemy.Attack() calls BasicAttackCombo.Attack() and returns how long the attack takes.
///     StateMachineBehavior uses returned attack duration to exit the "Attacking" state once the attack is over.
/// </summary>
public class Enemy : MonoBehaviour
{
    // Tag used only by the Player
    const string playerTag = "Player";
    // Minimum x-axis speed Enemy needs to be moving at to change sprite facing direction. Causes jittery flipping if too low.
    const float flipSpeedThreshold = 0.85f;

    [Tooltip("Range at which Enemy sees the player and starts moving towards them.")]
    [SerializeField] protected float _playerDetectionRange = 20.0f;
    [Tooltip("At most how far away the Enemy should be from the Player before attacking.")]
    [SerializeField] protected float _attackRange = 5.0f;
    [Tooltip("Waypoints for Enemy patrol path.")]
    [SerializeField] protected List<GameObject> _patrolWaypoints = new List<GameObject>();

    protected GameObject _player;
    protected Rigidbody _rigidbody;
    protected SpriteRenderer _spriteRenderer;
    protected NavMeshAgent _navMeshAgent;
    protected Animator _animator;
    protected BasicAttackCombo _basicAttackCombo;
    protected int _currentWaypointIndex = 0;

    // Animation variables need to be hashed before they can be set in code.
    protected readonly int _hashPlayerHasBeenSighted = Animator.StringToHash("PlayerHasBeenSighted");
    protected readonly int _hashPlayerWithinAttackRange = Animator.StringToHash("PlayerWithinAttackRange");

    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.FindWithTag(playerTag);
        _rigidbody = GetComponent<Rigidbody>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _basicAttackCombo = GetComponent<BasicAttackCombo>();
    }

    // Update is called once per frame
    void Update()
    {
        // Patrolling Enemy has sighted Player
        if (!_animator.GetBool(_hashPlayerHasBeenSighted) && Vector3.Distance(_player.transform.position, transform.position) <= _playerDetectionRange)
        {
            _animator.SetBool(_hashPlayerHasBeenSighted, true);
        }
        else 
        {
            // Since Enemy is chasing the Player, check how far away they are.
            _animator.SetBool(_hashPlayerWithinAttackRange, Vector3.Distance(_player.transform.position, transform.position) <= _attackRange);
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

    /// <summary>
    /// Causes Enemy to attack. Can be overridden to run special attacks or attacks in series, among other things.
    /// </summary>
    /// <returns>The total duration of the executed attack.</returns>
    public float Attack()
    {
        _basicAttackCombo.Attack();
        return _basicAttackCombo.GetCurrentAttackDuration();
    }
}
