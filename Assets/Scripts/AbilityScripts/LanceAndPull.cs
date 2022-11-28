using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lance and Pull is an ability that throws the players umbrella forward like a lance spearing and pushing
/// all enemies caught in its path. Once the umbrella reaches a certain distance, it opens up and gets pulled
/// back towards the player dragging along all enemies caught in its path.
///
/// The ability instantiates the "Lance" prefab upon use which has its own "LanceBehavior" script to manage
/// its collisions. 
///
/// TO DO: Probably going to have to make it interact with the boss differently so the boss doesn't get pushed
/// TO DO: Once there is a player sprite with the weapon, delete the weapon from the players hand while the lance
///        is out and then delete the lance and give the player sprite their weapon back
/// </summary>
public class LanceAndPull : Ability
{
    [SerializeField] protected GameObject _weapon;
    [SerializeField] protected float _distance;
    [SerializeField] protected float _speed;
    [SerializeField] protected int _damage;
    protected GameObject _player;
    protected PlayerControls _playerControls;

    
    public override void Activate(GameObject player)
    {
        if (_player == null)   // Initialize the variables upon the first activation
        {
            _player = player;
            _playerControls = player.GetComponent<PlayerControls>();
        }
        // Lock player input until the umbrella has come back to the player
        _playerControls.isInputLocked = true;  
        _playerControls.velocity.x = 0.0f;
        StartCoroutine(LanceMotion());
    }


    public override void Deactivate(GameObject player)
    {
        
    }


    private IEnumerator LanceMotion()
    {
        Transform playerCenter = _player.transform.GetChild(3);
        Vector3 finalPosition;
        GameObject _umbrella;
        bool moveForward = true;

        if (_playerControls.IsFacingRight())
        {
            _umbrella = Instantiate(_weapon, playerCenter.transform.position, playerCenter.transform.rotation);
            finalPosition = playerCenter.transform.position + new Vector3(_distance, 0, 0);
            _umbrella.GetComponent<LanceBehavior>().damage = _damage;
            float SE = 0.05f;

            // Move the Lance forward while it hasn't reached the final position
            while (_umbrella.transform.position.x < finalPosition.x - SE && moveForward)
            {
                _umbrella.transform.position = Vector3.Lerp(_umbrella.transform.position, finalPosition, Time.deltaTime * _speed);
                if (_umbrella.GetComponent<LanceBehavior>().CollidedWithWall() == true) { moveForward = false; }
                yield return null;
            }
            moveForward = false;

            // Move the now expanded umbrella back towards the player
            _umbrella.GetComponent<LanceBehavior>().OpenUmbrellaSprite();
            while (_umbrella.transform.position.x - SE > playerCenter.transform.position.x && !moveForward)
            {
                _umbrella.transform.position = Vector3.Lerp(_umbrella.transform.position, playerCenter.transform.position , Time.deltaTime * _speed);
                yield return null;
            }
        }

        else  // Player is facing the left
        {
            // offset moves the spawn position of the lance to the other side of the player
            Vector3 offset = new Vector3 (playerCenter.transform.localPosition.x * -2.0f, 0, 0);  
            _umbrella = Instantiate(_weapon, playerCenter.transform.position + offset, playerCenter.transform.rotation);
            finalPosition = playerCenter.transform.position + new Vector3(-_distance, 0, 0) + offset;
            _umbrella.GetComponent<LanceBehavior>().damage = _damage;
            float SE = 0.05f;   // Standard Error: used with lerp because lerp never reaches the full distance

            // Flip the sprite
            _umbrella.GetComponent<SpriteRenderer>().flipX = true;

            // Move the Lance forward while it hasn't reached the final position
            while (_umbrella.transform.position.x > finalPosition.x + SE && moveForward)
            {
                _umbrella.transform.position = Vector3.Lerp(_umbrella.transform.position, finalPosition, Time.deltaTime * _speed);
                if (_umbrella.GetComponent<LanceBehavior>().CollidedWithWall() == true) { moveForward = false; }
                yield return null;
            }
            moveForward = false;

            // Move the now expanded umbrella back towards the player
            _umbrella.GetComponent<LanceBehavior>().OpenUmbrellaSprite();
            while (_umbrella.transform.position.x + SE < playerCenter.transform.position.x + offset.x && !moveForward)
            {
                _umbrella.transform.position = Vector3.Lerp(_umbrella.transform.position, playerCenter.transform.position + offset, Time.deltaTime * _speed);
                yield return null;
            }
        }

        _playerControls.isInputLocked = false;  
        _umbrella.transform.DetachChildren();
        Destroy(_umbrella);  // Destroy the umbrella once it has returned so the player can hold the weapon again 
        StartCoroutine(ActivateCooldown());  // Start cooldown after the umbrella has come back to the player
    }
}
