using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Room contains Enemies for the Player to defeat before they can pass through. After the Player enters through the left, the Room's doors
/// will "close", preventing them from leaving. The camera is locked to only move between set left and right coordinates. After Enemies are defeated,
/// only the right door will open, preventing the Player from going back the way they came. 
/// Enemies are assumed to be placed within this Room before runtime. They are activated once the Player enters the Room collider.
/// The main Room trigger collider shouldn't be touching the door colliders, so the Player doesn't clip through the doors.
/// 
/// Note: While Enemies can navigate through the doors once they are closed, they shouldn't stay out of range for long to since the Player is
/// confined to the room.
/// </summary>
public class Room : MonoBehaviour
{
    private const string _playerTag = "Player";
    private const string _enemyLayerName = "Enemy";

    [Tooltip("An object with a collider that will become active when the Player enters the Room. This door does not open when Enemies are defeated.")]
    [SerializeField] private GameObject _leftDoor;
    [Tooltip("This door opens when all Enemies are defeated.")]
    [SerializeField] private GameObject _rightDoor;
    [Tooltip("How far left (in world space coordinates) the Player's camera can move while in this room.")]
    [SerializeField] private float _minCameraPosition = Mathf.NegativeInfinity;
    [Tooltip("How far right (in world space coordinates) the Player's camera can move while in this room.")]
    [SerializeField] private float _maxCameraPosition = Mathf.Infinity;

    private bool _roomHasBeenCleared = false;
    private bool _roomIsActive = false;
    private int _enemyLayerNumber;
    private List<GameObject> _enemyObjects = new List<GameObject>();
    private int _totalEnemies = 0;
    private CharacterController _player;
    private CameraController _cameraController;

    // Start is called before the first frame update
    void Start()
    {
        _leftDoor.SetActive(false);
        _rightDoor.SetActive(false);

        _enemyLayerNumber = LayerMask.NameToLayer(_enemyLayerName);
        _player = GameObject.FindGameObjectWithTag(_playerTag).GetComponent<CharacterController>();
        _cameraController = GameObject.FindGameObjectWithTag(_playerTag).GetComponentInChildren<CameraController>();

        // TODO: Testing automatic endpoint setting
        BoxCollider box = GetComponent<BoxCollider>();
        _minCameraPosition = transform.position.x - (box.size.x / 2) + box.center.x - _player.radius;
        float distanceFromRightSide = Mathf.Abs(Camera.main.transform.position.z) * Mathf.Tan(Mathf.Deg2Rad * Camera.main.fieldOfView / 2.0f);
        print(distanceFromRightSide);
        // Expected: ~18.6 Actual: 11.55
        _maxCameraPosition = transform.position.x + (box.size.x / 2) + box.center.x - distanceFromRightSide - _player.radius;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_roomHasBeenCleared)
        {
            return;
        }

        // A new Enemy has entered the Room. Add that Enemy to this Room's list and disable it. 
        if (other.gameObject.layer == _enemyLayerNumber && !_enemyObjects.Contains(other.gameObject))
        {
            _enemyObjects.Add(other.gameObject);
            other.gameObject.GetComponent<Health>().eventHasDied.AddListener(EnemyHasDied);
            other.gameObject.SetActive(false);
            _totalEnemies += 1;
        }

        // The Player has entered the Room.
        if (other.tag == _playerTag)
        {
            _rightDoor.SetActive(true);
            _leftDoor.SetActive(true);
            _roomIsActive = true;

            _cameraController.minXPosition = _minCameraPosition;
            _cameraController.maxXPosition = _maxCameraPosition;

            foreach (GameObject enemy in _enemyObjects)
            {
                enemy.SetActive(true);
                print(enemy.name);
            }
        }
    }

    /// <summary>
    /// Called when an Enemy in this Room dies. Lets the Player out of the Room when all Enemies are dead.
    /// </summary>
    private void EnemyHasDied()
    {
        _totalEnemies -= 1;

        if (_roomIsActive && _totalEnemies <= 0)
        {
            _roomHasBeenCleared = true;
            _rightDoor.SetActive(false);
            _cameraController.maxXPosition = Mathf.Infinity;
        }
    }
}
