using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Functionality for when the Player triggers the Boss fight.
/// </summary>
public class BossTrigger : MonoBehaviour
{
    private const string _playerTag = "Player";

    // True when the Player touches the trigger for the first time.
    private bool _activated = false;

    private void OnTriggerEnter(Collider other)
    {
        // Begin Boss fight
        if (!_activated && other.tag == _playerTag)
        {
            _activated = true;
            CameraController _cameraController = other.GetComponentInChildren<CameraController>();
            _cameraController.minXPosition = transform.position.x;
            _cameraController.maxXPosition = transform.position.x;
        }
    }
}
