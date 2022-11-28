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
    protected const string playerTag = "Player";
    // Minimum x-axis speed Enemy needs to be moving at to change sprite facing direction. Causes jittery flipping if too low.
    protected const float flipSpeedThreshold = 0.85f;

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
    protected Health _health;
    protected int _currentWaypointIndex = 0;
    protected bool _isFacingRight = true;

    // Animation variables need to be hashed before they can be set in code.
    protected readonly int _hashPlayerHasBeenSighted = Animator.StringToHash("PlayerHasBeenSighted");
    protected readonly int _hashPlayerWithinAttackRange = Animator.StringToHash("PlayerWithinAttackRange");
    protected readonly int _hashStunnedStart = Animator.StringToHash("StunnedStart");

    public float GetAttackRange() { return _attackRange; }

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        _player = GameObject.FindWithTag(playerTag);
        _rigidbody = GetComponent<Rigidbody>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _basicAttackCombo = GetComponent<BasicAttackCombo>();
        _health = GetComponent<Health>();

        _health.eventHasDied.AddListener(HasDied);
        _health.eventTookDamage.AddListener(WasHit);
    }

    protected virtual void Start()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // Lock rotation
        transform.rotation = Quaternion.Euler(0, 0, 0);

        // Flip sprite if speed is above a threshold
        if (Mathf.Abs(_navMeshAgent.velocity.x) > flipSpeedThreshold)
        {
            _isFacingRight = _navMeshAgent.velocity.x > 0.0f;
            _spriteRenderer.flipX = !_isFacingRight;
        }

        // Prevents Enemies from overlapping each other due to navigation in 3D space.
        transform.Translate(0, 0, -transform.position.z);

        // Patrolling Enemy has sighted Player
        if (!_animator.GetBool(_hashPlayerHasBeenSighted) && Vector3.Distance(_player.transform.position, transform.position) <= _playerDetectionRange)
        {
            _animator.SetBool(_hashPlayerHasBeenSighted, true);
        }
        // Enemy is able to attack the Player IF Enemy is on the ground AND facing the Player AND within attack range.
        else 
        {
            _animator.SetBool(_hashPlayerWithinAttackRange, 
                !_navMeshAgent.isOnOffMeshLink &&
                _isFacingRight == _player.transform.position.x - transform.position.x > 0.0f &&
                Vector3.Distance(_player.transform.position, transform.position) <= _attackRange);
        }
    }

    /// <summary>
    /// Called on the Update this Enemy runs out of health. Destroys this Enemy. Can be overridden for special behavior.
    /// </summary>
    protected virtual void HasDied()
    {
        Destroy(gameObject);
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
    public virtual float Attack()
    {
        _basicAttackCombo.Activate(gameObject);
        return _basicAttackCombo.GetCurrentAttackDuration();
    }

    /// <summary>
    /// TODO: Testing getting stunned
    /// </summary>
    public virtual void WasHit()
    {
        _animator.SetTrigger(_hashStunnedStart);
    }
}
