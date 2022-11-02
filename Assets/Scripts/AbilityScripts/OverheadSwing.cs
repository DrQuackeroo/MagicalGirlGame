using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Can be modified to perform any number of attack hitboxes in series.
/// </summary>
public class OverheadSwing : BasicAttackCombo
{
    [Tooltip("How much damage this attack does. If negative, damage will be set to Damage values in 'Combo List'.")]
    [SerializeField] protected int _damage = 40;
    [Tooltip("How long this attack takes in total. If negative, duration will be total Wind Up + Wind Down times in 'Combo List'.")]
    [SerializeField] protected float _duration = 0.25f;
    [Tooltip("Should the Player's y-position remain the same during this Ability?")]
    [SerializeField] protected bool _stopMidairWhileExecuting = false;

    // True if Activate() has been called before on this script.
    protected bool hasBeenInitialized = false;
    protected GameObject _player;
    protected PlayerControls _playerControls;
    // List of all colliders hit so far by one execution of this attack
    protected List<Collider> _currentHitColliders = new List<Collider>();

    // Set variables in _comboList if override values were set in inspector.
    private void Start()
    {
        if (_damage > 0)
        {
            foreach (BasicAttack attack in _comboList)
            {
                attack.SetDamage(_damage);
            }
        }

        if (_duration > 0.0f)
        {
            float interval = _duration / _comboList.Count;
            foreach (BasicAttack attack in _comboList)
            {
                attack.SetWindDown(interval);
            }
        }
    }

    public override void Activate(GameObject player)
    {
        // Needs to set variables since Ability scripts aren't attached to the Player GameObject.
        if (!hasBeenInitialized)
        {
            _player = player;
            _spriteRenderer = _player.GetComponent<SpriteRenderer>();
            _line = _player.GetComponent<LineRenderer>();
            _playerControls = _player.GetComponent<PlayerControls>();

            for (int i = 0; i < _comboList.Count; i++)
            {
                _comboList[i].SetPlayer(_player);
            }

            hasBeenInitialized = true;
        }

        // Halt player movement. Might be a better way to do this in general, if you want to improve it.
        _playerControls.isInputLocked = true;
        _playerControls.velocity.x = 0.0f;
        if (_stopMidairWhileExecuting)
        {
            _playerControls.applyGravity = false;
            _playerControls.velocity.y = 0.0f;
        }

        // prevents player from spamming basic attack while already mid-animation in an attack
        if (_midAttackCoroutine != null)
            return;

        _currentHitColliders.Clear();
        _midAttackCoroutine = _comboStart.AttackNewCollidersOnly(_player, new List<Collider>());
        _currentAttackState = _comboStart;

        StartCoroutine(_midAttackCoroutine);
    }

    public override void OnAttackFinish(BasicAttack nextAttack)
    {
        _currentHitColliders.AddRange(_currentAttackState.GetHitColliders());
        _currentAttackState = nextAttack;
        StopCoroutine(_midAttackCoroutine);
        _timeElapsed = 0f;

        // If there is a next attack, start it automatically. Otherwise, this Ability ends and variables should be reset.
        if (_currentAttackState != null)
        {
            _midAttackCoroutine = _currentAttackState.AttackNewCollidersOnly(_player, _currentHitColliders);
            StartCoroutine(_midAttackCoroutine);
        }
        else
        {
            _midAttackCoroutine = null;
            _playerControls.isInputLocked = false;
            _playerControls.applyGravity = true;
            UIAbilityIconsManager.ShowCooldown(this);
        }
    }
}
