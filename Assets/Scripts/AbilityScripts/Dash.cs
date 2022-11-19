using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : Ability
{
    [Tooltip("How fast (in units/second) the Player moves while dashing.")]
    [SerializeField] protected float _dashSpeed = 48f;
    [Tooltip("How long the dash lasts.")]
    [SerializeField] protected float _dashingTime = 0.25f;

    protected PlayerControls _playerControls;

    public override void Activate(GameObject player)
    {
        // Initialize variables if first time calling
        if (_playerControls == null)
        {
            _playerControls = player.GetComponent<PlayerControls>();
        }

        StartCoroutine(DashCoroutine());
    }

    public override void Deactivate(GameObject player)
    {
        
    }

    private IEnumerator DashCoroutine()
    {
        _playerControls.isInputLocked = true;
        _playerControls.applyGravity = false;
        //Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);


        if (_playerControls.IsFacingRight())
            _playerControls.velocity = new Vector3(_dashSpeed, 0.0f, 0.0f);
        else
            _playerControls.velocity = new Vector3(-_dashSpeed, 0.0f, 0.0f);

        yield return new WaitForSeconds(_dashingTime);


        _playerControls.isInputLocked = false;
        _playerControls.applyGravity = true;
        _playerControls.velocity = Vector3.zero;
        //Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);

        StartCoroutine(ActivateCooldown());
    }
}
