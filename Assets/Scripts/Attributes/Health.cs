using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Used to communicate parameters when calling Health.TakeDamage(). Data is used to make damage-related calculations. Can be modified to pass more information.
/// </summary>
public struct DamageParameters
{
    public DamageParameters(int d, GameObject character)
    {
        Damage = d;
        AttackingCharacter = character;
        DamagingObject = character;
        HasKnockback = false;
        Knockback = Vector3.zero;
    }

    public DamageParameters(int d, GameObject character, GameObject dObject)
    {
        Damage = d;
        AttackingCharacter = character;
        DamagingObject = dObject;
        HasKnockback = false;
        Knockback = Vector3.zero;
    }

    public DamageParameters(int d, GameObject character, Vector2 knockback)
    {
        Damage = d;
        AttackingCharacter = character;
        DamagingObject = character;
        HasKnockback = knockback != Vector2.zero;
        Knockback = new Vector3(knockback.x, knockback.y, 0.0f);
    }

    public int Damage { get; }
    // The Enemy/Player that caused this damage
    public GameObject AttackingCharacter { get; }
    // Object that directly dealt damage. Different from AttackingCharacter for projectiles.
    public GameObject DamagingObject { get; }
    public bool HasKnockback { get; }
    // Impulse to apply if this attack hits.
    public Vector3 Knockback { get; }
}

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
    // Invoked when owner's health is reduced.
    [HideInInspector] public UnityEvent eventTookDamage;

    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private bool _printToConsole = true;
    [Tooltip("Angle between an attacker and this character for which an attack is considered 'in front' and can be blocked.")]
    [SerializeField] private float _blockAngle = 45.0f;
    [Tooltip("Played when this object takes damage.")]
    [SerializeField] private AudioClip _hitSoundEffect;
    private int _currentHealth;
    private bool _isAlive;
    private bool _facingRight = true;
    // Current amount of knockback impulse that will be applied to this Enemy once they transition into the Stunned state.
    private Vector3 _knockbackToApply = Vector3.zero;
    private AudioSource _audioSource;

    public void SetFacingRight(bool value) { _facingRight = value; }
    public Vector3 GetKnockbackToApply() { return _knockbackToApply; }


    void Start()
    {
        _currentHealth = _maxHealth;
        _isAlive = true;
        _audioSource = GetComponent<AudioSource>();
    }

    public void ResetHealth()
    {
        _currentHealth = _maxHealth;
        _isAlive = true;
    }

    /// <summary>
    /// Attempt to damage this component's owner. Damage could fail to apply if the owner is blocking.
    /// </summary>
    /// <param name="parameters">Struct containing information for this damage attempt.</param>
    public void TakeDamage(DamageParameters parameters)
    {
        if (!_isAlive)
        {
            return;
        }

        // Check if the owner is blocking
        if (isBlocking && parameters.DamagingObject != null)
        {
            // Attacks should only be blocked if they come from in front of this character. Use angle between attacker and blocker to check if in front.
            float angleToAttacker = Mathf.Abs(Vector3.Angle(gameObject.transform.right, parameters.DamagingObject.transform.position - transform.position));
            if (!_facingRight)
                angleToAttacker = 180.0f - angleToAttacker;

            if (angleToAttacker <= _blockAngle)
            {
                eventAttackBlocked.Invoke();
                return;
            }
        }

        // Owner cannot block the damage, so reduce _currentHealth
        _currentHealth -= parameters.Damage;

        if (parameters.HasKnockback)
        {
            _knockbackToApply = parameters.Knockback;
            // Flip knockback direction if attack came from the right.
            if (parameters.DamagingObject.transform.position.x > transform.position.x)
                _knockbackToApply.x *= -1;
        }

        eventTookDamage.Invoke();
        if (_printToConsole is true) {Debug.LogFormat("{0} took {1} damage", gameObject.name, parameters.Damage);}
        // if the object has a DamageIndicator script then we want to create a damage indicator
        if (gameObject.GetComponent<DamageIndicator>() != null)
        {
            gameObject.GetComponent<DamageIndicator>().CreateDamageIndicator(parameters.Damage, transform.position, 
                                                                             gameObject.GetComponent<Collider>().bounds.extents.y);

            // TODO: Testing hit effects
            // Assumes that only Enemies have damage indicators
            //GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControls>().SlowdownTime();
        }

        if (_audioSource != null && _hitSoundEffect != null)
        {
            _audioSource.PlayOneShot(_hitSoundEffect, 1.0f);
        }
        

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
        TakeDamage(new DamageParameters(_currentHealth, null));
    }

    public int GetMaxHealth()
    {
        return _maxHealth;
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
