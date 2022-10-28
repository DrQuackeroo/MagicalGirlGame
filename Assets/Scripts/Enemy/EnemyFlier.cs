using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A special type of Enemy that can move through the environment. Does not rely on NavMesh for movement.
/// </summary>
public class EnemyFlier : Enemy
{
    // Difference of y-axis position for which the Flyer will attempt to shoot the Player. Flyer will not shoot if value is too low.
    protected const float shootingVerticalRange = 0.25f;

    [Tooltip("How fast this Flier moves in units/second.")]
    [SerializeField] protected float _speed;

    // True if Flier is unable to move this Update.
    [HideInInspector] public bool isStopped = false;

    // Where this Flier is currently trying to move to.
    protected Vector3 _currentDestination;
    // How far from _currentDestination the Flier will stop. Will be this far from the destination when it arrives.
    protected float _stoppingDistance = 0.0f;
    // Has the Flier arrived at its destination?
    protected bool _hasArrivedAtDestination = true;
    // How much the Flier moved this Update
    protected Vector3 _currentMovement;

    public bool GetHasArrivedAtDestination() { return _hasArrivedAtDestination; }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        _currentDestination = transform.position;
    }

    /// <summary>
    /// Slightly different movement compared to base class. Could make more functions in Enemy.cs and override them here to make this
    /// look better, but copying works for now.
    /// </summary>
    protected override void Update()
    {
        // Lock rotation
        transform.rotation = Quaternion.Euler(0, 0, 0);

        // Move Flier 
        if (!isStopped)
            MoveToDestination();

        // Flip sprite if speed is above a threshold
        if (Mathf.Abs(_currentMovement.x) * _speed > 0.01f)
        {
            _isFacingRight = _currentMovement.x > 0.0f;
            _spriteRenderer.flipX = !_isFacingRight;
        }

        // Prevents Enemies from overlapping each other due to navigation in 3D space.
        transform.Translate(0, 0, -transform.position.z);

        // Patrolling Enemy has sighted Player
        if (!_animator.GetBool(_hashPlayerHasBeenSighted) && Vector3.Distance(_player.transform.position, transform.position) <= _playerDetectionRange)
        {
            _animator.SetBool(_hashPlayerHasBeenSighted, true);
        }
        // Enemy is able to attack the Player IF Enemy is facing the Player AND within attack range AND has about the same y-coordinate.
        else
        {
            _animator.SetBool(_hashPlayerWithinAttackRange,
                _isFacingRight == _player.transform.position.x - transform.position.x > 0.0f &&
                Vector3.Distance(_player.transform.position, transform.position) <= _attackRange &&
                Mathf.Abs(transform.position.y - _player.transform.position.y) <= shootingVerticalRange);
        }
    }

    /// <summary>
    /// Set the Flier's current destination. It will try to moves towards this position each Update.
    /// </summary>
    /// <param name="destination">World space coordinates of the destination.</param>
    /// <param name="stoppingDistance">How far from the destination the Flier will be before stopping.</param>
    public void SetDestination(Vector3 destination, float stoppingDistance=0.0f)
    {
        _hasArrivedAtDestination = false;
        _currentDestination = destination;
        _stoppingDistance = stoppingDistance;
    }

    /// <summary>
    /// Move closer to the destination each Update.
    /// </summary>
    protected void MoveToDestination()
    {
        _currentMovement = (_currentDestination - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, _currentDestination) - _stoppingDistance;

        // Don't overshoot the destination.
        _currentMovement *= Mathf.Min(_speed * Time.deltaTime, distance);

        transform.Translate(_currentMovement);

        if (Vector3.Distance(transform.position, _currentDestination) <= _stoppingDistance)
        {
            ArrivedAtDestination();
        }
    }

    /// <summary>
    /// Called on the Update the Flier reaches the destination position. Sets a bool indicating it has arrived.
    /// </summary>
    protected void ArrivedAtDestination()
    {
        _hasArrivedAtDestination = true;
    }

    /// <summary>
    /// Causes EnemyFlier to shoot. Fliers are assumed to be ranged, but this can be modified for melee attacks too.
    /// </summary>
    /// <returns>The total duration of the executed attack.</returns>
    public override float Attack()
    {
        // TODO: Make shooting
        return 1.0f;
    }
}
