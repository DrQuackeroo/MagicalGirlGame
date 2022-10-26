using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// Basic component for any object that has health.
/// 
/// Sometimes, an owner might not take damage due to its current state. Therefore, Health.cs needs some way of communicating with its owner.
/// One way to do this is for Health.cs to have public variables that the owner's controlling script can set. Health.cs checks those variables 
/// before applying damage.
/// </summary>
public class Health : MonoBehaviour
{
    // Invoked when script owner takes fatal damage.
    [HideInInspector] public UnityEvent eventHasDied;
    // Invoked when an attack was just blocked.
    [HideInInspector] public UnityEvent eventAttackBlocked;
    [HideInInspector] public bool isBlocking;

    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private bool _printToConsole = true;
    [Tooltip("Angle between an attacker and this character for which an attack is considered 'in front' and can be blocked.")]
    [SerializeField] private float _blockAngle = 45.0f;
    private int _currentHealth;
    private bool _isAlive;
    private bool _facingRight = true;

    public void SetFacingRight(bool value) { _facingRight = value; }


    void Start()
    {
        _currentHealth = _maxHealth;
        _isAlive = true;
    }

    public void ResetHealth()
    {
        _currentHealth = _maxHealth;
        _isAlive = true;
    }

    /// <summary>
    /// Attempt to damage this component's owner. Damage could fail to apply if the owner is blocking.
    /// </summary>
    /// <param name="damageTaken">Amount of damage attacker is trying to apply.</param>
    /// <param name="attacker">What object is trying to damage this owner. Should be another character or a projectile.</param>
    public void TakeDamage(int damageTaken, GameObject attacker)
    {
        // Check if the owner is blocking
        if (isBlocking && attacker != null)
        {
            // Attacks should only be blocked if they come from in front of this character. Use angle between attacker and blocker to check if in front.
            float angleToAttacker = Mathf.Abs(Vector3.Angle(gameObject.transform.right, attacker.transform.position - transform.position));
            if (!_facingRight)
                angleToAttacker = 180.0f - angleToAttacker;

            if (angleToAttacker <= _blockAngle)
            {
                eventAttackBlocked.Invoke();
                return;
            }
        }

        // Owner cannot block the damage, so reduce _currentHealth
        _currentHealth -= damageTaken;
        if (_printToConsole is true) {Debug.LogFormat("{0} took {1} damage", gameObject.name, damageTaken);}

        if (_currentHealth <= 0)
        {
            _isAlive = false;
            eventHasDied.Invoke();
            if (_printToConsole is true) {Debug.LogFormat("{0} died", gameObject.name);}
        }
    }

    public void HealHealth(int healthHealed)
    {
        _currentHealth += healthHealed;
        if (_printToConsole is true) {Debug.LogFormat("{0} healed {1} health", gameObject.name, healthHealed);}

        if (_currentHealth >= _maxHealth)
        {
            _currentHealth = _maxHealth;
            if (_printToConsole is true) {Debug.LogFormat("{0} is at full health", gameObject.name);}
        }
    }

    public void Kill()
    {
        TakeDamage(_currentHealth, null);
    }

    public int GetHealth()
    {
        return _currentHealth;
    }

    public bool CheckIsAlive()
    {
        return _isAlive;
    }

}
