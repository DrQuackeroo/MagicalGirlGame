using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Functionality for when the Player triggers the Boss fight.
/// </summary>
public class BossTrigger : MonoBehaviour
{
    private const string _playerTag = "Player";

    [Tooltip("What Boss in the scene this trigger is associated with.")]
    [SerializeField] private GameObject _bossObject;

    // True when the Player touches the trigger for the first time.
    private bool _activated = false;
    private GameObject _leftDoor;

    private void Start()
    {
        _leftDoor = transform.Find("LeftDoor").gameObject;

        if (_bossObject == null)
        {
            _bossObject = transform.parent.Find("Enemies").Find("Boss").gameObject;
        }
        _bossObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Begin Boss fight
        if (!_activated && other.tag == _playerTag)
        {
            _activated = true;
            CameraController _cameraController = other.GetComponentInChildren<CameraController>();
            _cameraController.minXPosition = other.transform.position.x;
            _cameraController.maxXPosition = other.transform.position.x;
            _leftDoor.SetActive(true);

            _bossObject.SetActive(true);
        }
    }
}
