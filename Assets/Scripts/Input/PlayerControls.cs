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

    private PlayerInputActions _playerInputActions;
    private SpriteRenderer _spriteRenderer;
    private BasicAttackCombo _basicAttackCombo;
    private CharacterController _characterController;
    private Health _health;
    private Transform _playerTransform;

    // Getters
    public bool IsFacingRight() { return _faceRight; }

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
        EnablePauseControl();
        AbilityHandler.OnSetAbility += GetAbilities;
        PauseHandler.OnPauseEnable += DisablePlayerControls;
        PauseHandler.OnPauseDisable += EnablePlayerControls;
    }

    private void OnDisable()
    {
        DisablePlayerControls();
        DisablePauseControl();
        AbilityHandler.OnSetAbility -= GetAbilities;
        PauseHandler.OnPauseEnable -= DisablePlayerControls;
        PauseHandler.OnPauseDisable -= EnablePlayerControls;
    }

    public void EnablePauseControl()
    {
        _playerInputActions.Player.PauseMenu.Enable();
        _playerInputActions.Player.PauseMenu.performed += OnPauseMenu;
    }

    public void DisablePauseControl()
    {
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
        _playerInputActions.Player.AbilityOne.canceled += EndAbilityOne;

        _playerInputActions.Player.AbilityTwo.Enable();
        _playerInputActions.Player.AbilityTwo.performed += OnAbilityTwo;
        _playerInputActions.Player.AbilityTwo.canceled += EndAbilityTwo;

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
        _playerInputActions.Player.AbilityOne.canceled -= EndAbilityOne;

        _playerInputActions.Player.AbilityTwo.Disable();
        _playerInputActions.Player.AbilityTwo.performed -= OnAbilityTwo;
        _playerInputActions.Player.AbilityTwo.canceled -= EndAbilityTwo;

        _playerInputActions.Player.AbilityThree.Disable();
        _playerInputActions.Player.AbilityThree.performed -= OnAbilityThree;
        _playerInputActions.Player.AbilityThree.canceled -= EndAbilityThree;
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


    //"Space" key - jump
    private void OnJump(InputAction.CallbackContext context)
    {
        if (isInputLocked)
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
            velocity.y = _jumpMod;
        }
    }




    //"Z" key - basic attack
    private void OnBasicAttack(InputAction.CallbackContext context)
    {
        if (isInputLocked)
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
        if (isInputLocked)
        {
            Debug.Log("Input Locked");
            return;
        }

        //vv new ability system, WIP
        int abilityNum = 0; //num will be one less than method name
        if (abilityNum < 0 || abilityNum >= abilities.Count) return;
        if (abilities[abilityNum].IsOnCooldown) return;
        StartCoroutine(abilities[abilityNum].ActivateCooldown());
        abilities[abilityNum].Activate(gameObject);
        //^^ For new ability system
    }

    private void EndAbilityOne(InputAction.CallbackContext context)
    {
        //vv new ability system, WIP
        int abilityNum = 0; //num will be one less than method name
        if (abilityNum >= 0 && abilityNum < abilities.Count) abilities[abilityNum].Deactivate(gameObject);
        //^^ For new ability system
    }

    //"C" key - ability two
    private void OnAbilityTwo(InputAction.CallbackContext context)
    {
        if (isInputLocked)
        {
            Debug.Log("Input Locked");
            return;
        }

        //vv new ability system, WIP
        int abilityNum = 1; //num will be one less than method name
        if (abilityNum < 0 || abilityNum >= abilities.Count) return;
        if (abilities[abilityNum].IsOnCooldown) return;
        StartCoroutine(abilities[abilityNum].ActivateCooldown());
        abilities[abilityNum].Activate(gameObject);
        //^^ For new ability system

        Debug.Log("Input: Beam");
        if (_canBeam)
            StartCoroutine(Beam());
    }

    private void EndAbilityTwo(InputAction.CallbackContext context)
    {
        //vv new ability system, WIP
        int abilityNum = 1; //num will be one less than method name
        if (abilityNum >= 0 && abilityNum < abilities.Count) abilities[abilityNum].Deactivate(gameObject);
        //^^ For new ability system
    }

    //"V" key - ability three
    private void OnAbilityThree(InputAction.CallbackContext context)
    {
        if (isInputLocked)
        {
            Debug.Log("Input Locked");
            return;
        }

        //vv new ability system, WIP
        int abilityNum = 2; //num will be one less than method name
        if (abilityNum < 0 || abilityNum >= abilities.Count) return;
        if (abilities[abilityNum].IsOnCooldown) return;
        StartCoroutine(abilities[abilityNum].ActivateCooldown());
        abilities[abilityNum].Activate(gameObject);
        //^^ For new ability system
    }

    private void EndAbilityThree(InputAction.CallbackContext context)
    {
        //vv new ability system, WIP
        int abilityNum = 2; //num will be one less than method name
        if (abilityNum >= 0 && abilityNum < abilities.Count) abilities[abilityNum].Deactivate(gameObject);
        //^^ For new ability system
    }


    //"Esc" key - escape menu
    private void OnPauseMenu(InputAction.CallbackContext context)
    {
        PauseHandler.TogglePause();
        Debug.Log("pause menu");

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



}
