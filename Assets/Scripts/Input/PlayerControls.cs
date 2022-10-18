using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerControls : MonoBehaviour
{
    [Header("Other Movement Settings")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundLayer;
    [Header("Player Stats")]
    [SerializeField] private float _speedMod = 10f;
    [SerializeField] private float _jumpMod = 15f;
    private PlayerInputActions _playerInputActions;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigidbody;
    private float _movement;
    private float _jumpsRemaining;
    private float _tempTime = 0f;
    private float _jumpCD = 0.2f;
    private BasicAttackCombo _basicAttackCombo;

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _basicAttackCombo = GetComponent<BasicAttackCombo>();
    }

    private void OnEnable()
    {
        _playerInputActions.Player.Movement.Enable();
        _playerInputActions.Player.Movement.started += OnMove;
        _playerInputActions.Player.Movement.performed += OnMove;
        _playerInputActions.Player.Movement.canceled += OnMove;

        _playerInputActions.Player.Jump.Enable();
        _playerInputActions.Player.Jump.performed += OnJump;

        _playerInputActions.Player.Attack.Enable();
        _playerInputActions.Player.Attack.performed += OnBasicAttack;

        _playerInputActions.Player.AbilityOne.Enable();
        _playerInputActions.Player.AbilityOne.performed += OnAbilityOne;

        _playerInputActions.Player.AbilityTwo.Enable();
        _playerInputActions.Player.AbilityTwo.performed += OnAbilityTwo;

        _playerInputActions.Player.PauseMenu.Enable();
        _playerInputActions.Player.PauseMenu.performed += OnPauseMenu;
    }

    private void OnDisable()
    {
        _playerInputActions.Player.Movement.Disable();
        _playerInputActions.Player.Movement.started -= OnMove;
        _playerInputActions.Player.Movement.performed -= OnMove;
        _playerInputActions.Player.Movement.canceled -= OnMove;

        _playerInputActions.Player.Jump.Disable();
        _playerInputActions.Player.Jump.performed -= OnJump;

        _playerInputActions.Player.Attack.Disable();
        _playerInputActions.Player.Attack.performed -= OnBasicAttack;

        _playerInputActions.Player.AbilityOne.Disable();
        _playerInputActions.Player.AbilityOne.performed -= OnAbilityOne;

        _playerInputActions.Player.AbilityTwo.Disable();
        _playerInputActions.Player.AbilityTwo.performed -= OnAbilityTwo;

        _playerInputActions.Player.PauseMenu.Disable();
        _playerInputActions.Player.PauseMenu.performed -= OnPauseMenu;
    }

    void FixedUpdate()
    {
        _rigidbody.velocity = new Vector3(_movement * _speedMod, _rigidbody.velocity.y, 0);
    }

    //"Left Arrow/Right Arrow" keys - movement
    private void OnMove(InputAction.CallbackContext context)
    {
        _movement = context.ReadValue<float>();
        //Flips sprite if moving to the LEFT, change if needed
        if (_movement > 0) _spriteRenderer.flipX = false;
        if (_movement < 0) _spriteRenderer.flipX = true;
    }

    //"Space" key - jump
    private void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("jump");
        if (Physics2D.OverlapCircle(_groundCheck.position, 0.2f, _groundLayer))
        {
            _jumpsRemaining = 2;
        }
        if (_jumpsRemaining > 0 && Time.time > _tempTime)
        {
            _tempTime = Time.time + _jumpCD;
            _jumpsRemaining--;
            //_rigidbody.velocity = new Vector2(0, _rigidbody.velocity.y > 0f ? _jumpMod * 0.5f : _jumpMod);
            _rigidbody.velocity = new Vector2(0, _jumpMod);

        }
    }

    //"Z" key - basic attack
    private void OnBasicAttack(InputAction.CallbackContext context)
    {
        Debug.Log("basic attack");
        _basicAttackCombo.Attack();
    }

    //"X" key - ability one
    private void OnAbilityOne(InputAction.CallbackContext context)
    {
        Debug.Log("ability one");
    }

    //"C" key - ability two
    private void OnAbilityTwo(InputAction.CallbackContext context)
    {
        Debug.Log("ability two");
    }

    //"Esc" key - escape menu
    private void OnPauseMenu(InputAction.CallbackContext context)
    {
        Debug.Log("pause menu");
    }
}
