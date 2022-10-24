using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Health : MonoBehaviour
{
    // Invoked when script owner takes fatal damage.
    public UnityEvent eventHasDied;

    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private bool _printToConsole = true;
    private int _currentHealth;
    private bool _isAlive;

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

    public void TakeDamage(int damageTaken)
    {
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
        TakeDamage(_currentHealth);
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
