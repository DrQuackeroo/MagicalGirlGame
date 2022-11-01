using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : Ability
{
    protected GameObject _player;
    protected PlayerControls _playerControls;
    protected Health _health;
    protected SpriteRenderer _spriteRenderer;

    public override void Activate(GameObject player)
    {
        // Initialize Player variables the first time this ability is activated.
        if (_player == null)
        {
            _player = player;
            _playerControls = player.GetComponent<PlayerControls>();
            _health = player.GetComponent<Health>();
            _spriteRenderer = player.GetComponent<SpriteRenderer>();
        }

        _health.isBlocking = true;
        _playerControls.isInputLocked = true;
        _playerControls.applyFriction = true;

        // Halt x-axis movement when blocking begins.
        _playerControls.velocity.x = 0.0f;

        // TESTING: Changes color when blocking
        _spriteRenderer.color = Color.blue;
    }

    public override void Deactivate(GameObject player)
    {
        _health.isBlocking = false;
        _playerControls.isInputLocked = false;
        _playerControls.applyFriction = false;

        // TESTING: Changes color when blocking
        _spriteRenderer.color = Color.white;

        UIAbilityIconsManager.ShowCooldown(this);
    }
}
