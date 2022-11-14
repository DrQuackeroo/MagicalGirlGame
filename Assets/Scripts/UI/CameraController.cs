using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to control the position of the Camera relative to the Player and the World. Through this script is similar to a Constraint Component,
/// this has the benefit of allowing the Camera to be part of the Player prefab.
/// 
/// Might be able to change these to static methods because there should only be one CameraController per scene.
/// </summary>
public class CameraController : MonoBehaviour
{
    // The y-coordinate the camera moves along. Set during Start, equal to the Camera's initial y-position. 
    [HideInInspector] public float _worldSpaceYPosition;

    private void Start()
    {
        _worldSpaceYPosition = transform.position.y;
    }

    /// <summary>
    /// Call to update the Camera's position so that it is always on the same y-axis despite being a child of Player.
    /// Must be called in Player's Update() function to prevent jittery movement.
    /// </summary>
    public void UpdatePosition()
    {
        transform.Translate(0.0f, _worldSpaceYPosition - transform.position.y, 0.0f);
    }

    public void UnlinkCamera()
    {
        transform.SetParent(null);
    }
}
