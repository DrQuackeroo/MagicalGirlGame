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
    [SerializeField] private float _dashMod = 48f;

    private float _movement;
    private bool _faceRight = true;

    [SerializeField] private float _jumpsRemaining = 2f;
    [SerializeField] private float _tempTime = 0f;
    [SerializeField] private float _jumpCD = 0.2f;

    private bool _isDashing = false;
    private bool _canDash = true;
    [SerializeField] private float _dashingTime = 0.25f;
    [SerializeField] private float _dashCD = 1f;

    private PlayerInputActions _playerInputActions;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigidbody;
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
        if (_isDashing) return;

        _rigidbody.velocity = new Vector3(_movement * _speedMod, _rigidbody.velocity.y, 0);
    }

    //"Left Arrow/Right Arrow" keys - movement
    private void OnMove(InputAction.CallbackContext context)
    {
        if (_isDashing)
        {
            Debug.Log("Move overriden by Dash");
            return;
        }

        _movement = context.ReadValue<float>();
        //Flips sprite if moving to the LEFT, change if needed
        if (_movement > 0) _spriteRenderer.flipX = false;
        else if (_movement < 0) _spriteRenderer.flipX = true;

        _faceRight = !_spriteRenderer.flipX;

    }

    //"Space" key - jump
    private void OnJump(InputAction.CallbackContext context)
    {
        if (_isDashing)
        {
            Debug.Log("Jump overriden by Dash");
            return;
        }
        Debug.Log("jump");
        if (Physics2D.OverlapCircle(_groundCheck.position, 0.4f, _groundLayer))
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


    private IEnumerator Dash()
    {

        Debug.Log("Dash Start" + transform.localScale.x);
        _canDash = false;
        _isDashing = true;

        float tempGravity = _rigidbody.gravityScale;
        _rigidbody.gravityScale = 0f;

        if (_faceRight)
            _rigidbody.velocity = new Vector2(_dashMod, 0f);
        else
            _rigidbody.velocity = new Vector2(-_dashMod, 0f);

        Debug.Log("Speed:" + _rigidbody.velocity.x);

        yield return new WaitForSeconds(_dashingTime);
        Debug.Log("Dash End");
        _isDashing = false;
        _rigidbody.velocity = new Vector2(0f, 0f);
        _rigidbody.gravityScale = tempGravity;

        yield return new WaitForSeconds(_dashCD);
        Debug.Log("Dash Refresh");
        _canDash = true;

    }


    //"Z" key - basic attack
    private void OnBasicAttack(InputAction.CallbackContext context)
    {
        if (_isDashing)
        {
            Debug.Log("Attack overriden by Dash");
            return;
        }
        Debug.Log("basic attack");
        _basicAttackCombo.Attack();
    }

    //"X" key - ability one
    private void OnAbilityOne(InputAction.CallbackContext context)
    {
        if (_isDashing)
        {
            Debug.Log("Ability 1 overriden by Dash");
            return;
        }
        Debug.Log("ability one: occupied by Dash");

        if (_canDash)
            StartCoroutine(Dash());


    }

    //"C" key - ability two
    private void OnAbilityTwo(InputAction.CallbackContext context)
    {
        if (_isDashing)
        {
            Debug.Log("Ability2 overriden by Dash");
            return;
        }
        Debug.Log("ability two");
    }

    //"Esc" key - escape menu
    private void OnPauseMenu(InputAction.CallbackContext context)
    {

        Debug.Log("pause menu");
    }
}
