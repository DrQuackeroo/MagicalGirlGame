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
    // How much knockback is applied to the Player if they block an attack. Could be changed to a settable field.
    private const float _blockKnockbackVelocity = 5.0f;
    // Amount in Unity units/second that velocity changes by when decelerating under friction. Cound be changed to settable field.
    private const float _frictionDeceleration = 20.0f;

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
    // If true, Player can use directional and ability controls.
    // TODO: Possbily redundant with _isDashing
    private bool _isInputLocked = false;
    // If true, gravity will be applied this Update to the player's y-velocity.
    private bool _applyGravity = true;
    // If true, Player's x-velocity will reduce to 0 over time.
    private bool _applyFriction = false;
    // The Player's current velocity. Used to determine movement each Update.
    private Vector3 _velocity = Vector3.zero;

    [Header("Jumps")]
    [SerializeField] private float _jumpsRemaining = 2f;
    [SerializeField] private float _tempTime = 0f;
    [SerializeField] private float _jumpCD = 0.2f;

    [Header("Dash")]
    private bool _isDashing = false;
    private bool _canDash = true;
    [SerializeField] private float _dashingTime = 0.25f;
    [SerializeField] private float _dashCD = 1f;

    private PlayerInputActions _playerInputActions;
    private SpriteRenderer _spriteRenderer;
    private BasicAttackCombo _basicAttackCombo;
    private CharacterController _characterController;
    private Health _health;

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _basicAttackCombo = GetComponent<BasicAttackCombo>();
        _characterController = GetComponent<CharacterController>();
        _health = GetComponent<Health>();

        _health.eventAttackBlocked.AddListener(AttackBlocked);
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

        _playerInputActions.Player.AbilityThree.Enable();
        _playerInputActions.Player.AbilityThree.performed += OnAbilityThree;
        _playerInputActions.Player.AbilityThree.canceled += EndAbilityThree;

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

        _playerInputActions.Player.AbilityThree.Disable();
        _playerInputActions.Player.AbilityThree.performed -= OnAbilityThree;
        _playerInputActions.Player.AbilityThree.canceled -= EndAbilityThree;

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

        if (!_isInputLocked)
            _velocity.x = _xInputValue * _speedMod;
        if (_applyGravity)
            _velocity.y += Physics.gravity.y * _gravityMultiplier * Time.deltaTime;
        if (_applyFriction && _velocity.x != 0.0f)
            _velocity.x = Mathf.Max(0.0f, Mathf.Abs(_velocity.x) - _frictionDeceleration * Time.deltaTime) * (_velocity.x / Mathf.Abs(_velocity.x));

        _characterController.Move(_velocity * Time.deltaTime);
    }

    //"Left Arrow/Right Arrow" keys - movement
    private void OnMove(InputAction.CallbackContext context)
    {
        if (_isDashing || _isInputLocked)
        {
            Debug.Log("Move overriden by Dash");
            return;
        }

        _xInputValue = context.ReadValue<float>();
        //Flips sprite if moving to the LEFT, change if needed
        //if (_xInputValue > 0)  = false;
        //else if (_xInputValue < 0) _spriteRenderer.flipX = true;
        if (_xInputValue > 0 && !_faceRight)
            Rotate();
        else if (_xInputValue < 0 && _faceRight)
            Rotate();
    }

    private void Rotate()
    {
        _faceRight = !_faceRight;
        _health.SetFacingRight(_faceRight);
        //transform.Rotate(0f, 180f, 0f);
        _spriteRenderer.flipX = !_spriteRenderer.flipX;
    }

    /// <summary>
    /// Invoked when the Player blocks an attack. Currently just pushes them back a bit.
    /// </summary>
    private void AttackBlocked()
    {
        if (_faceRight)
            _velocity.x -= _blockKnockbackVelocity;
        else
            _velocity.x += _blockKnockbackVelocity;
    }


    //"Space" key - jump
    private void OnJump(InputAction.CallbackContext context)
    {
        if (_isDashing || _isInputLocked)
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
        Debug.Log("Dash Start");
        
        
        _canDash = false;
        _isDashing = true;
        _isInputLocked = true;
        _applyGravity = false;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);


        if (_faceRight)
            _velocity = new Vector3(_dashMod, 0.0f, 0.0f);
        else
            _velocity = new Vector3(-_dashMod, 0.0f, 0.0f);

        yield return new WaitForSeconds(_dashingTime);

        
        Debug.Log("Dash End");

        _isDashing = false;
        _isInputLocked = false;
        _applyGravity = true;
        _velocity = Vector3.zero;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);

        yield return new WaitForSeconds(_dashCD);
        Debug.Log("Dash Refresh");
        _canDash = true;
        


    }

    //"Z" key - basic attack
    private void OnBasicAttack(InputAction.CallbackContext context)
    {
        if (_isDashing || _isInputLocked)
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
        if (_isDashing || _isInputLocked)
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
        if (_isDashing || _isInputLocked)
        {
            Debug.Log("Ability2 overriden by Dash");
            return;
        }
        Debug.Log("ability two: occupied by Beam");
        StartCoroutine(Beam());
    }

    //"V" key - ability three
    private void OnAbilityThree(InputAction.CallbackContext context)
    {
        if (_isDashing || _isInputLocked)
        {
            Debug.Log("Ability3 overriden by Dash");
            return;
        }
        Debug.Log("ability three: Block");
        _health.isBlocking = true;
        _isInputLocked = true;
        _applyFriction = true;
    }

    private void EndAbilityThree(InputAction.CallbackContext context)
    {
        print("Ability three canceled");
        _health.isBlocking = false;
        _isInputLocked = false;
        _applyFriction = false;
    }

    //"Esc" key - escape menu
    private void OnPauseMenu(InputAction.CallbackContext context)
    {

        Debug.Log("pause menu");
    }








    [Header("Beam Location")]
    public GameObject FirePoint;
    private IEnumerator Beam()
    {
        //FirePoint.GetComponent<SpriteRenderer>().flipX = !FirePoint.GetComponent<SpriteRenderer>().flipX;
        //FirePoint.GetComponent<Transform>().RotateAround(FirePoint.GetComponent<Position>, , 180f);

        /*
        bool rotated = false;
        if (!_faceRight)
        {
            FirePoint.GetComponent<SpriteRenderer>().flipX = true; 
            rotated = true;
        }*/
        FirePoint.GetComponent<Renderer>().enabled = true;
        Debug.Log("Beam Start" + transform.localScale.x);
        //_canDash = false;
        //_isDashing = true;
        //float tempGravity = _rigidbody.gravityScale;
        //_rigidbody.gravityScale = 0f;
        //if (_faceRight)
        //     _rigidbody.velocity = new Vector2(_dashMod, 0f);
        //else
        //    _rigidbody.velocity = new Vector2(-_dashMod, 0f);
        //Debug.Log("Speed:" + _rigidbody.velocity.x);
        yield return new WaitForSeconds(_dashingTime);
        Debug.Log("Beam End");
        //_isDashing = false;
        //_rigidbody.velocity = new Vector2(0f, 0f);
        //_rigidbody.gravityScale = tempGravity;
        FirePoint.GetComponent<Renderer>().enabled = false;
        yield return new WaitForSeconds(_dashCD);
        Debug.Log("Beam Refresh");
        //_canDash = true;
    }


















}
