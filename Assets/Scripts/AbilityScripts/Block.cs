using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : Ability
{
    [Tooltip("A shield sprite that will appear when the Player blocks. Will be used during testing for now.")]
    [SerializeField] protected GameObject _shieldPrefab;

    protected GameObject _player;
    protected PlayerControls _playerControls;
    protected Health _health;
    protected SpriteRenderer _spriteRenderer;
    protected GameObject _shield;
    protected Animator _animator;

    // Animation transition values
    protected readonly int _hashBlockStart = Animator.StringToHash("BlockStart");
    protected readonly int _hashBlockEnd = Animator.StringToHash("BlockEnd");

    public override void Activate(GameObject player)
    {
        // Initialize Player variables the first time this ability is activated.
        if (_player == null)
        {
            _player = player;
            _playerControls = player.GetComponent<PlayerControls>();
            _health = player.GetComponent<Health>();
            _spriteRenderer = player.GetComponent<SpriteRenderer>();
            _animator = player.GetComponent<Animator>();


            // Make the shield sprite
            _shield = Instantiate(_shieldPrefab, _player.transform.position, _player.transform.rotation, _player.transform);
        }

        if (_playerControls.IsFacingRight())
            _shield.transform.localScale = new Vector3(1, 1, 1);
        else
            _shield.transform.localScale = new Vector3(-1, 1, 1);

        _health.isBlocking = true;
        _playerControls.isInputLocked = true;
        _playerControls.applyFriction = true;
        _shield.SetActive(true);
        _animator.SetTrigger(_hashBlockStart);

        // Halt x-axis movement when blocking begins.
        _playerControls.velocity.x = 0.0f;
    }

    public override void Deactivate(GameObject player)
    {
        // Hack solution. To prevent Deactivate() from being called when Block was never activated to begin with, check isBlocking first.
        // Should be generalized by calling Ability.Deactivate in PlayerControls.cs only if the associated ability is running.
        if (_health.isBlocking)
        {
            _health.isBlocking = false;
            _playerControls.isInputLocked = false;
            _playerControls.applyFriction = false;
            _shield.SetActive(false);
            _animator.SetTrigger(_hashBlockEnd);

            UIAbilityIconsManager.ShowCooldown(this);
        }
    }
}
