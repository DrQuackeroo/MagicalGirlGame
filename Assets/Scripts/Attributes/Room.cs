using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// How to use: scale BoxCollider component so that it covers all the Enemies in this Room at runtime.
/// 
/// A Room contains Enemies for the Player to defeat before they can pass through. After the Player enters through the left, the Room's doors
/// will "close", preventing them from leaving. The camera is locked to only move between set left and right coordinates. After Enemies are defeated,
/// only the right door will open, preventing the Player from going back the way they came. 
/// 
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

    [Tooltip("Object that prevents the Player from leaving the room. Need only be set once with the Room prefab.")]
    [SerializeField] private GameObject _doorPrefab;

    private GameObject _leftDoor;
    private GameObject _rightDoor;
    private float _minCameraPosition = Mathf.NegativeInfinity;
    private float _maxCameraPosition = Mathf.Infinity;

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
        _enemyLayerNumber = LayerMask.NameToLayer(_enemyLayerName);
        _player = GameObject.FindGameObjectWithTag(_playerTag).GetComponent<CharacterController>();
        _cameraController = GameObject.FindGameObjectWithTag(_playerTag).GetComponentInChildren<CameraController>();


        // Set the camera bounds.
        BoxCollider roomBox = GetComponent<BoxCollider>();
        float fovHorizontal = Camera.VerticalToHorizontalFieldOfView(Camera.main.fieldOfView, Camera.main.aspect);
        // The distance between the camera's world x-position and the farthest x-position in the game field that the camera can see.
        float cameraDistanceFromVisibleEdge = Mathf.Abs(Camera.main.transform.position.z) * Mathf.Tan(Mathf.Deg2Rad *fovHorizontal / 2.0f);

        _minCameraPosition = transform.position.x - (roomBox.size.x / 2) + roomBox.center.x - _player.radius;
        _maxCameraPosition = transform.position.x + (roomBox.size.x / 2) + roomBox.center.x - cameraDistanceFromVisibleEdge;


        // Instantiates Doors: colliders that become active once the Player enters the room, then deactive when Enemies are defeated.
        Vector3 doorBoxSize = _doorPrefab.GetComponent<BoxCollider>().size;
        // How large the actual box collider is along the X and Y axes, accounting for GameObject scale and BoxCollider.size
        float scaledDoorWidth = doorBoxSize.x * _doorPrefab.transform.localScale.x;
        float scaledDoorHeight = doorBoxSize.y * _doorPrefab.transform.localScale.y;
        // Doors are placed such that the bottom of their collider is the same height as the bottom of the Room collider.
        float doorYPosition = transform.position.y + roomBox.center.y - (roomBox.size.y / 2) + (scaledDoorHeight) / 2;

        // Doors will be on the edge of the camera for both left and right camera endpoints. The Player will always remain within the bounds of the camera.
        Vector3 leftDoorPosition = new Vector3(_minCameraPosition - cameraDistanceFromVisibleEdge - (scaledDoorWidth) / 2, doorYPosition, 0.0f);
        Vector3 rightDoorPosition = new Vector3(_maxCameraPosition + cameraDistanceFromVisibleEdge + (scaledDoorWidth) / 2, doorYPosition, 0.0f);

        _leftDoor = Instantiate(_doorPrefab, leftDoorPosition, Quaternion.identity, transform);
        _rightDoor = Instantiate(_doorPrefab, rightDoorPosition, Quaternion.identity, transform);
        _leftDoor.SetActive(false);
        _rightDoor.SetActive(false);
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
