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

    [Tooltip("How much more the default Physics.gravity value affects the Player")]
    [SerializeField] private float _gravityMultiplier = 3.0f;

    // If true, Player cannot use directional and ability controls.
    [HideInInspector] public bool isInputLocked = false;
    // If true, gravity will be applied this Update to the Player's y-velocity.
    [HideInInspector] public bool applyGravity = true;
    // If true, Player's x-velocity will reduce to 0 over time.
    [HideInInspector] public bool applyFriction = false;
    // The Player's current velocity. Used to determine movement each Update.
    [HideInInspector] public Vector3 velocity = Vector3.zero;

    private float _xInputValue;
    private bool _faceRight = true;

    [Header("Abilities")]
    [SerializeField] private bool _abilitySelectOnSpawn = false;
    private List<Ability> abilities = new List<Ability>();


    [Header("Jumps")]
    [SerializeField] private float _jumpsRemaining = 2f;
    [SerializeField] private float _tempTime = 0f;
    [SerializeField] private float _jumpCD = 0.2f;


    [Header("Beam")]
    [SerializeField] private float distanceRay = 20; //Has not implemented changing hitbox size yet
    public GameObject laserHitBox;
    private bool _canBeam = true;

    public bool HasBasicControlsEnabled { get; private set; }
    public bool HasPauseControlsEnabled { get; private set; }

    //private PlayerInputActions _playerInputActions;
    private PlayerInput _playerInput;
    private SpriteRenderer _spriteRenderer;
    private BasicAttackCombo _basicAttackCombo;
    private CharacterController _characterController;
    private Health _health;
    private Transform _playerTransform;
    private CameraController _cameraController;
    private Animator _animator;

    // Animation variables need to be hashed before they can be set in code.
    protected readonly int _hashTookDamage = Animator.StringToHash("TookDamage");

    // Getters
    public bool IsFacingRight() { return _faceRight; }

    private void Awake()
    {
        //_playerInputActions = new PlayerInputActions();
        _playerInput = GetComponent<PlayerInput>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _basicAttackCombo = GetComponent<BasicAttackCombo>();
        _characterController = GetComponent<CharacterController>();
        _cameraController = transform.GetComponentInChildren<CameraController>();
        _health = GetComponent<Health>();
        _health.eventAttackBlocked.AddListener(AttackBlocked);
        _health.eventTookDamage.AddListener(TookDamage);
        _health.eventHasDied.AddListener(OnPlayerDeath);
        _animator = GetComponent<Animator>();




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
        EnablePauseControl();
        AbilityHandler.OnSetAbility += GetAbilities;
        PauseHandler.OnPauseEnable += DisablePlayerControls;
        PauseHandler.OnPauseDisable += EnablePlayerControls;
        PauseHandler.OnControlScreenEnable += DisablePauseControl;
        PauseHandler.OnControlScreenDisable += EnablePauseControl;
    }

    private void OnDisable()
    {
        DisablePlayerControls();
        DisablePauseControl();
        AbilityHandler.OnSetAbility -= GetAbilities;
        PauseHandler.OnPauseEnable -= DisablePlayerControls;
        PauseHandler.OnPauseDisable -= EnablePlayerControls;
        PauseHandler.OnControlScreenEnable -= DisablePauseControl;
        PauseHandler.OnControlScreenDisable -= EnablePauseControl;
    }

    public void EnablePauseControl()
    {
        //_playerInputActions.Player.PauseMenu.Enable();
        //_playerInputActions.Player.PauseMenu.performed += OnPauseMenu;
        
        HasPauseControlsEnabled = true;
    }

    public void DisablePauseControl()
    {
        //_playerInputActions.Player.PauseMenu.Disable();
        //_playerInputActions.Player.PauseMenu.performed -= OnPauseMenu;
       
        HasPauseControlsEnabled = false;
    }

    //Made public so other classes can enable/disable player's control
    public void EnablePlayerControls()
    {
        //_playerInputActions.Player.Movement.Enable();
        //_playerInputActions.Player.Movement.started += OnMove;
        //_playerInputActions.Player.Movement.performed += OnMove;
        //_playerInputActions.Player.Movement.canceled += OnMove;

        //_playerInputActions.Player.Jump.Enable();
        //_playerInputActions.Player.Jump.performed += OnJump;

        //_playerInputActions.Player.Attack.Enable();
        //_playerInputActions.Player.Attack.performed += OnBasicAttack;

        //_playerInputActions.Player.AbilityOne.Enable();
        //_playerInputActions.Player.AbilityOne.performed += OnAbilityOne;
        //_playerInputActions.Player.AbilityOne.canceled += EndAbilityOne;

        //_playerInputActions.Player.AbilityTwo.Enable();
        //_playerInputActions.Player.AbilityTwo.performed += OnAbilityTwo;
        //_playerInputActions.Player.AbilityTwo.canceled += EndAbilityTwo;

        //_playerInputActions.Player.AbilityThree.Enable();
        //_playerInputActions.Player.AbilityThree.performed += OnAbilityThree;
        //_playerInputActions.Player.AbilityThree.canceled += EndAbilityThree;
        
        HasBasicControlsEnabled = true;
    }

    //Made public so other classes can enable/disable player's control
    public void DisablePlayerControls()
    {
        //_playerInputActions.Player.Movement.Disable();
        //_playerInputActions.Player.Movement.started -= OnMove;
        //_playerInputActions.Player.Movement.performed -= OnMove;
        //_playerInputActions.Player.Movement.canceled -= OnMove;

        //_playerInputActions.Player.Jump.Disable();
        //_playerInputActions.Player.Jump.performed -= OnJump;

        //_playerInputActions.Player.Attack.Disable();
        //_playerInputActions.Player.Attack.performed -= OnBasicAttack;

        //_playerInputActions.Player.AbilityOne.Disable();
        //_playerInputActions.Player.AbilityOne.performed -= OnAbilityOne;
        //_playerInputActions.Player.AbilityOne.canceled -= EndAbilityOne;

        //_playerInputActions.Player.AbilityTwo.Disable();
        //_playerInputActions.Player.AbilityTwo.performed -= OnAbilityTwo;
        //_playerInputActions.Player.AbilityTwo.canceled -= EndAbilityTwo;

        //_playerInputActions.Player.AbilityThree.Disable();
        //_playerInputActions.Player.AbilityThree.performed -= OnAbilityThree;
        //_playerInputActions.Player.AbilityThree.canceled -= EndAbilityThree;

        HasBasicControlsEnabled = false;
    }

    private void Start()
    {
        if (_abilitySelectOnSpawn) AbilitySelectionOnStart(); 
        else abilities = AbilityHandler.CurrentAbilities;
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
        if (!_characterController.isGrounded && _characterController.velocity.y == 0 && velocity.y > 0.0f)
            velocity.y = 0;
        // ... or if Player is on the ground but y-velocity is negative.
        if (_characterController.isGrounded && velocity.y < _yVelocityResetThreshold)
            velocity.y = _yVelocityResetThreshold;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //    This isn't as clean as the events used previously but according to tutorials,                         //
        //    interactive rebinding is easiest to use this rather than rely on the C# PlayerInputActions class.     //
        //    Reference: https://www.youtube.com/watch?v=csqVa2Vimao                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //_xInputValue = _playerInput.actions["Movement"].ReadValue<float>();

        // Composites and rebinding have a lot of edge cases according to YouTube comments so now move left and move right are seperate.
        // Therefore to calculate overall movement direction, move left and move right can either be 0 or 1, so add the two while multiplying left by -1.
        _xInputValue = _playerInput.actions["Move Left"].ReadValue<float>() * -1 + _playerInput.actions["Move Right"].ReadValue<float>();

        // Coroutines allow for each function to not be blocking, meaning overall gameplay will feel smoother
        if (HasBasicControlsEnabled || !isInputLocked)
        {
            if (_playerInput.actions["Jump"].WasPerformedThisFrame()) StartCoroutine(OnJump());
            if (_playerInput.actions["Attack"].WasPerformedThisFrame()) StartCoroutine(OnBasicAttack());
            if (_playerInput.actions["Ability One"].WasPerformedThisFrame()) StartCoroutine(OnAbilityOne());
            if (_playerInput.actions["Ability One"].WasReleasedThisFrame()) StartCoroutine(EndAbilityOne());
            if (_playerInput.actions["Ability Two"].WasPerformedThisFrame()) StartCoroutine(OnAbilityTwo());
            if (_playerInput.actions["Ability Two"].WasReleasedThisFrame()) StartCoroutine(EndAbilityTwo());
            if (_playerInput.actions["Ability Three"].WasPerformedThisFrame()) StartCoroutine(OnAbilityThree());
            if (_playerInput.actions["Ability Three"].WasReleasedThisFrame()) StartCoroutine(EndAbilityThree());
        }
        if (HasPauseControlsEnabled)
        {
            if (_playerInput.actions["Pause"].WasPerformedThisFrame()) StartCoroutine(OnPauseMenu());
        }

        // Input is calculated but not applied while input is locked.
        if (!isInputLocked)
        {
            velocity.x = _xInputValue * _speedMod;

            // Rotate character based on input direction.
            if (_xInputValue > 0 && !_faceRight)
                Rotate();
            else if (_xInputValue < 0 && _faceRight)
                Rotate();
        }
        if (applyGravity)
            velocity.y += Physics.gravity.y * _gravityMultiplier * Time.deltaTime;
        if (applyFriction && velocity.x != 0.0f)
            velocity.x = Mathf.Max(0.0f, Mathf.Abs(velocity.x) - _frictionDeceleration * Time.deltaTime) * (velocity.x / Mathf.Abs(velocity.x));

        _characterController.Move(velocity * Time.deltaTime);

        // Update the camera position at the same time the Player moves.
        _cameraController.UpdatePosition();
    }

    // "Left Arrow/Right Arrow" keys - movement
    private void OnMove(InputAction.CallbackContext context)
    {
        _xInputValue = context.ReadValue<float>();
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
            velocity.x -= _blockKnockbackVelocity;
        else
            velocity.x += _blockKnockbackVelocity;
    }

    /// <summary>
    /// Invoked when the Player takes damage. Causes blinking animation.
    /// </summary>
    private void TookDamage()
    {
        print("Took damage");
        _animator.SetTrigger(_hashTookDamage);
    }


    //"Space" key - jump
    public IEnumerator OnJump()
    {
        if (isInputLocked)
        {
            Debug.Log("Input Locked");
            yield break;
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
            velocity.y = _jumpMod;
        }
        yield break;
    }




    //"Z" key - basic attack
    private IEnumerator OnBasicAttack()
    {
        if (isInputLocked)
        {
            Debug.Log("Input Locked");
            yield break;
        }

        Debug.Log("Input: Basic Attack");
        _basicAttackCombo.Activate(gameObject);
        yield break;
    }

    //"X" key - ability one
    private IEnumerator OnAbilityOne()
    {
        if (isInputLocked)
        {
            Debug.Log("Input Locked");
            yield break;
        }

        //vv new ability system, WIP
        int abilityNum = 0; //num will be one less than method name
        if (abilityNum < 0 || abilityNum >= abilities.Count) yield break;
        if (abilities[abilityNum].IsOnCooldown) yield break;
        abilities[abilityNum].Activate(gameObject);
        yield break;
        //^^ For new ability system
    }

    private IEnumerator EndAbilityOne()
    {
        //vv new ability system, WIP
        int abilityNum = 0; //num will be one less than method name
        if (abilityNum >= 0 && abilityNum < abilities.Count) abilities[abilityNum].Deactivate(gameObject);
        yield break;
        //^^ For new ability system
    }

    //"C" key - ability two
    private IEnumerator OnAbilityTwo()
    {
        if (isInputLocked)
        {
            Debug.Log("Input Locked");
            yield break;
        }

        //vv new ability system, WIP
        int abilityNum = 1; //num will be one less than method name
        if (abilityNum < 0 || abilityNum >= abilities.Count) yield break;
        if (abilities[abilityNum].IsOnCooldown) yield break;
        abilities[abilityNum].Activate(gameObject);
        //^^ For new ability system
    }

    private IEnumerator EndAbilityTwo()
    {
        //vv new ability system, WIP
        int abilityNum = 1; //num will be one less than method name
        if (abilityNum >= 0 && abilityNum < abilities.Count) abilities[abilityNum].Deactivate(gameObject);
        yield break;
        //^^ For new ability system
    }

    //"V" key - ability three
    private IEnumerator OnAbilityThree()
    {
        if (isInputLocked)
        {
            Debug.Log("Input Locked");
            yield break;
        }

        //vv new ability system, WIP
        int abilityNum = 2; //num will be one less than method name
        if (abilityNum < 0 || abilityNum >= abilities.Count) yield break;
        if (abilities[abilityNum].IsOnCooldown) yield break;
        abilities[abilityNum].Activate(gameObject);
        //^^ For new ability system
    }

    private IEnumerator EndAbilityThree()
    {
        //vv new ability system, WIP
        int abilityNum = 2; //num will be one less than method name
        if (abilityNum >= 0 && abilityNum < abilities.Count) abilities[abilityNum].Deactivate(gameObject);
        yield break;
        //^^ For new ability system
    }


    //"Esc" key - escape menu
    private IEnumerator OnPauseMenu()
    {
        PauseHandler.TogglePause();
        Debug.Log("pause menu");
        yield break;
    }


    private IEnumerator Beam()
    {
        //laserHitBox.Transform.scale = new Vector3(distanceRay, laserHitBox.Transform.Scale.y, laserHitBox.Transform.Scale.z);
        _canBeam = false;
        isInputLocked = true;
        applyGravity = false;
        velocity.x = 0f;
        velocity.y = 0f;


        Debug.Log("Beam Start");

        laserHitBox.GetComponent<Renderer>().enabled = true;

        yield return new WaitForSeconds(1.5f);

        Debug.Log("Beam End");

        laserHitBox.GetComponent<Renderer>().enabled = false;

        isInputLocked = false;
        applyGravity = true;

        yield return new WaitForSeconds(4f);
        Debug.Log("Beam Refresh");
        _canBeam = true;

    }


    // TODO: Testing some hit effect stuff
    private bool _isTimeSlowed = false;
    public void SlowdownTime()
    {
        print("Slow down time now");
        //IEnumerable slowdownCoroutine = Slowdown();
        if (_isTimeSlowed)
        {
            StopCoroutine(Slowdown());
        }
        StartCoroutine(Slowdown());
    }

    // TODO: Testing hit effects
    private IEnumerator Slowdown()
    {
        // TODO: Fix bad interaction with pause/unpause menu.

        Time.timeScale = 0.025f;
        _isTimeSlowed = true;

        yield return new WaitForSecondsRealtime(0.05f);

        Time.timeScale = 1.0f;
        _isTimeSlowed = false;
    }

    public void OnPlayerDeath()
    {
        gameObject.SetActive(false);
        _cameraController.UnlinkCamera();
    }
}
