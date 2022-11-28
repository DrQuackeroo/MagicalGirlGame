using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Limits the downward speed of the credits words.
/// </summary>
public class CreditsText : MonoBehaviour
{
    private const float _minYVelocity = -2.0f;

    private Rigidbody _rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_rigidbody.velocity.y < _minYVelocity)
        {
            _rigidbody.AddForce(0.0f, _minYVelocity - _rigidbody.velocity.y, 0.0f, ForceMode.VelocityChange);
        }
    }
}
