using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    // Amount in units/second^2 that velocity changes by when decelerating under friction. Cound be changed to settable field.
    private const float _frictionDeceleration = 20.0f;

    [Header("Player Stats")]
    [SerializeField] private float _speedMod = 10f;
    [SerializeField] private float _jumpMod = 15f;
    [SerializeField] private float _dashMod = 48f;

    [Tooltip("How much more the default Physics.gravity value affects the Player")]
    [SerializeField] private float _gravityMultiplier = 3.0f;

    private float _xInputValue;
    private bool _faceRight = true;
    // If true, Player can use directional and ability controls.

    private bool _isInputLocked = false;
    // If true, gravity will be applied this Update to the Player's y-velocity.
    private bool _applyGravity = true;
    // If true, Player's x-velocity will reduce to 0 over time.
    private bool _applyFriction = false;
    // The Player's current velocity. Used to determine movement each Update.
    private Vector3 _velocity = Vector3.zero;

    [Header("Abilities")]
    [SerializeField] private bool _requireAbilitySelectionOnStart;
    private List<Ability> abilities = new List<Ability>();


    [Header("Jumps")]
    [SerializeField] private float _jumpsRemaining = 2f;
    [SerializeField] private float _tempTime = 0f;
    [SerializeField] private float _jumpCD = 0.2f;

    [Header("Dash")]
    private bool _canDash = true;
    [SerializeField] private float _dashingTime = 0.25f;
    [SerializeField] private float _dashCD = 1f;


    [Header("Beam")]
    [SerializeField] private float distanceRay = 20; //Has not implemented changing hitbox size yet
    public GameObject laserHitBox;
    private bool _canBeam = true;

    private PlayerInputActions _playerInputActions;
    private SpriteRenderer _spriteRenderer;
    private BasicAttackCombo _basicAttackCombo;
    private CharacterController _characterController;
    private Health _health;
    private Transform _playerTransform;

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _basicAttackCombo = GetComponent<BasicAttackCombo>();
        _characterController = GetComponent<CharacterController>();
        _health = GetComponent<Health>();
        _health.eventAttackBlocked.AddListener(AttackBlocked);




        //Below are made for the Beam()
        _playerTransform = GetComponent<Transform>();


    }

    private void GetAbilities(List<Ability> abs)
    {
        abilities = abs;
    }

    private void OnEnable()
    {
        EnablePlayerControls();
        AbilityHandler.OnSetAbility += GetAbilities;
        PauseHandler.OnPauseEnable += DisablePlayerControls;
        PauseHandler.OnPauseDisable += EnablePlayerControls;
        _playerInputActions.Player.PauseMenu.Enable();
        _playerInputActions.Player.PauseMenu.performed += OnPauseMenu;
    }

    private void OnDisable()
    {
        DisablePlayerControls();
        AbilityHandler.OnSetAbility -= GetAbilities;
        PauseHandler.OnPauseEnable -= DisablePlayerControls;
        PauseHandler.OnPauseDisable -= EnablePlayerControls;
        _playerInputActions.Player.PauseMenu.Disable();
        _playerInputActions.Player.PauseMenu.performed -= OnPauseMenu;
    }

    //Made public so other classes can enable/disable player's control
    public void EnablePlayerControls()
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
    }

    //Made public so other classes can enable/disable player's control
    public void DisablePlayerControls()
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
    }

    private void Start()
    {
        if (_requireAbilitySelectionOnStart) AbilitySelectionOnStart();
    }

    private void AbilitySelectionOnStart()
    {
        //pause the game at the start
        PauseHandler.UnPause(false);
        //clear currently stored abilities
        AbilityHandler.ClearCurrentAbilities();
        //make the player select abilities
        AbilityHandler.EnterAbilityMenu();
    }

    void Update()
    {

        // Reset the y-velocity if the Player wasn't actually moving upwards last frame (eg. hit their head on a ceiling)...
        if (!_characterController.isGrounded && _characterController.velocity.y == 0 && _velocity.y > 0.0f)
            _velocity.y = 0;
        // ... or if Player is on the ground but y-velocity is negative.
        if (_characterController.isGrounded && _velocity.y < _yVelocityResetThreshold)
            _velocity.y = _yVelocityResetThreshold;

        // Input is calculated but not applied while input is locked.
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
        if (_isInputLocked)
        {
            Debug.Log("Input Locked");
            return;
        }

        _xInputValue = context.ReadValue<float>();

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
        laserHitBox.transform.RotateAround(_playerTransform.position, Vector3.up, 180);
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
        if (_isInputLocked)
        {
            Debug.Log("Input Locked");
            return;
        }
        Debug.Log("Input: Jump");

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




    //"Z" key - basic attack
    private void OnBasicAttack(InputAction.CallbackContext context)
    {
        if (_isInputLocked)
        {
            Debug.Log("Input Locked");
            return;
        }

        Debug.Log("Input: Basic Attack");
        _basicAttackCombo.Activate(gameObject);
    }

    //"X" key - ability one
    private void OnAbilityOne(InputAction.CallbackContext context)
    {
        //vv new ability system, WIP
        int abilityNum = 0; //num will be one less than method name
        if (abilityNum >= 0 && abilityNum < abilities.Count) abilities[abilityNum].Activate(gameObject);
        //^^ For new ability system

        if (_isInputLocked)
        {
            Debug.Log("Input Locked");
            return;
        }
        Debug.Log("Input: Dash");

        if (_canDash)
            StartCoroutine(Dash());


    }

    //"C" key - ability two
    private void OnAbilityTwo(InputAction.CallbackContext context)
    {
        //vv new ability system, WIP
        int abilityNum = 1; //num will be one less than method name
        if (abilityNum >= 0 && abilityNum < abilities.Count) abilities[abilityNum].Activate(gameObject);
        //^^ For new ability system

        if (_isInputLocked)
        {
            Debug.Log("Input Locked");
            return;
        }
        Debug.Log("Input: Beam");
        if (_canBeam)
            StartCoroutine(Beam());
    }

    //"V" key - ability three
    private void OnAbilityThree(InputAction.CallbackContext context)
    {
        //vv new ability system, WIP
        int abilityNum = 2; //num will be one less than method name
        if (abilityNum >= 0 && abilityNum < abilities.Count) abilities[abilityNum].Activate(gameObject);
        //^^ For new ability system

        if (_isInputLocked)
        {
            Debug.Log("Input Locked");
            return;
        }
        Debug.Log("Input: Block");
        _health.isBlocking = true;
        _isInputLocked = true;
        _applyFriction = true;

        // Halt x-axis movement when blocking begins.
        _velocity.x = 0.0f;

        // TESTING: Changes color when blocking
        _spriteRenderer.color = Color.blue;
    }

    private void EndAbilityThree(InputAction.CallbackContext context)
    {
        print("Ability three canceled");
        _health.isBlocking = false;
        _isInputLocked = false;
        _applyFriction = false;

        // TESTING: Changes color when blocking
        _spriteRenderer.color = Color.white;
    }


    //"Esc" key - escape menu
    private void OnPauseMenu(InputAction.CallbackContext context)
    {
        PauseHandler.TogglePause();
        Debug.Log("pause menu");

    }



    private IEnumerator Dash()
    {
        Debug.Log("Dash Start");

        _canDash = false;
        _isInputLocked = true;
        _applyGravity = false;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);


        if (_faceRight)
            _velocity = new Vector3(_dashMod, 0.0f, 0.0f);
        else
            _velocity = new Vector3(-_dashMod, 0.0f, 0.0f);

        yield return new WaitForSeconds(_dashingTime);

        Debug.Log("Dash End");

        _isInputLocked = false;
        _applyGravity = true;
        _velocity = Vector3.zero;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);

        yield return new WaitForSeconds(_dashCD);
        Debug.Log("Dash Refresh");
        _canDash = true;

    }


    private IEnumerator Beam()
    {
        //laserHitBox.Transform.scale = new Vector3(distanceRay, laserHitBox.Transform.Scale.y, laserHitBox.Transform.Scale.z);
        _canBeam = false;
        _isInputLocked = true;
        _applyGravity = false;
        _velocity.x = 0f;
        _velocity.y = 0f;


        Debug.Log("Beam Start");

        laserHitBox.GetComponent<Renderer>().enabled = true;

        yield return new WaitForSeconds(1.5f);

        Debug.Log("Beam End");

        laserHitBox.GetComponent<Renderer>().enabled = false;

        _isInputLocked = false;
        _applyGravity = true;

        yield return new WaitForSeconds(4f);
        Debug.Log("Beam Refresh");
        _canBeam = true;

    }



}
