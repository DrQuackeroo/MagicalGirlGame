using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerControls : MonoBehaviour
{
    // If the Player is grounded but y-velocity is < 0, it should be reset so it doesn't accumulate while not falling.
    // However, if the reset value is too close to 0, _characterController.isGrounded won't return reliably because the
    // Player is not being "pushed" into the ground.
    private const float _yVelocityResetThreshold = -0.5f;

    [Header("Other Movement Settings")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundLayer;
    [Header("Player Stats")]

    [SerializeField] private float _speedMod = 10f;
    [SerializeField] private float _jumpMod = 15f;
    [SerializeField] private float _dashMod = 48f;
    [Tooltip("How much more the default Physics.gravity value affects the Player")]
    [SerializeField] private float _gravityMultiplier = 3.0f;

    private float _xInputValue;
    private bool _faceRight = true;
    private Vector3 _velocity = Vector3.zero;

    [SerializeField] private float _jumpsRemaining = 2f;
    [SerializeField] private float _tempTime = 0f;
    [SerializeField] private float _jumpCD = 0.2f;

    private bool _isDashing = false;
    private bool _canDash = true;
    [SerializeField] private float _dashingTime = 0.25f;
    [SerializeField] private float _dashCD = 1f;

    private PlayerInputActions _playerInputActions;
    private SpriteRenderer _spriteRenderer;
    private BasicAttackCombo _basicAttackCombo;
    private CharacterController _characterController;

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _basicAttackCombo = GetComponent<BasicAttackCombo>();
        _characterController = GetComponent<CharacterController>();
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

    void Update()
    {
        // Reset the y-velocity if the Player wasn't actually moving upwards last frame (eg. hit their head on a ceiling)...
        if (!_characterController.isGrounded && _characterController.velocity.y == 0 && _velocity.y > 0.0f)
            _velocity.y = 0;
        // ... or if Player is on the ground but y-velocity is negative.
        if (_characterController.isGrounded && _velocity.y < _yVelocityResetThreshold)
            _velocity.y = _yVelocityResetThreshold;

        if (!_isDashing)
        {
            _velocity.x = _xInputValue * _speedMod;
            _velocity.y += Physics.gravity.y * _gravityMultiplier * Time.deltaTime;
        }

        _characterController.Move(_velocity * Time.deltaTime);
    }

    //"Left Arrow/Right Arrow" keys - movement
    private void OnMove(InputAction.CallbackContext context)
    {
        if (_isDashing)
        {
            Debug.Log("Move overriden by Dash");
            return;
        }

        _xInputValue = context.ReadValue<float>();
        //Flips sprite if moving to the LEFT, change if needed
        if (_xInputValue > 0) _spriteRenderer.flipX = false;
        else if (_xInputValue < 0) _spriteRenderer.flipX = true;

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
        
        if (_characterController.isGrounded)
        {
            _jumpsRemaining = 2;
        }
        if (_jumpsRemaining > 0 && Time.time > _tempTime)
        {
            _tempTime = Time.time + _jumpCD;
            _jumpsRemaining--;
            _velocity.y = _jumpMod;
        }
    }


    private IEnumerator Dash()
    {

        Debug.Log("Dash Start" + transform.localScale.x);
        _canDash = false;
        _isDashing = true;

        if (_faceRight)
            _velocity = new Vector3(_dashMod, 0.0f, 0.0f);
        else
            _velocity = new Vector3(-_dashMod, 0.0f, 0.0f);

        Debug.Log("Speed:" + _velocity.x);

        yield return new WaitForSeconds(_dashingTime);
        Debug.Log("Dash End");
        _isDashing = false;
        _velocity = Vector3.zero;

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
