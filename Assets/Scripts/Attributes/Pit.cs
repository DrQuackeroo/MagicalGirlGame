using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for bottomless pit colliders. Kills any object with a Health.cs component that touches it.
/// </summary>
public class Pit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Health otherHealth = other.GetComponent<Health>();

        if (otherHealth != null)
        {
            otherHealth.Kill();
        }
    }
}
