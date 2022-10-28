using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A special type of Enemy that can move through the environment. Does not rely on NavMesh for movement.
/// </summary>
public class EnemyFlier : Enemy
{
    [Tooltip("How fast this Flier moves in units/second.")]
    [SerializeField] private float _speed;

    // Where this Flier is currently trying to move to.
    private Vector3 _currentDestination;
    // How far from _currentDestination the Flier will stop, at most. Will be at least this far from the destination when it arrives.
    private float _maxStoppingDistance = 0.2f;
    // Has the Flier arrived at its destination?
    private bool _hasArrivedAtDestination = true;

    public bool GetHasArrivedAtDestination() { return _hasArrivedAtDestination; }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        _currentDestination = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        MoveToDestination();
    }

    /// <summary>
    /// Set the Flier's current destination. It will try to moves towards this position each Update.
    /// </summary>
    /// <param name="destination">World space coordinates of the destination.</param>
    /// <param name="maxStoppingDistance">At least how far from the destination the Flier will be before stopping.</param>
    public void SetDestination(Vector3 destination, float maxStoppingDistance=0.2f)
    {
        _hasArrivedAtDestination = false;
        _currentDestination = destination;
        _maxStoppingDistance = maxStoppingDistance;
    }

    /// <summary>
    /// Move closer to the destination each Update.
    /// </summary>
    protected void MoveToDestination()
    {
        Vector3 step = (_currentDestination - transform.position).normalized * _speed * Time.deltaTime;
        transform.Translate(step);

        if (Vector3.Distance(transform.position, _currentDestination) <= _maxStoppingDistance)
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
}
