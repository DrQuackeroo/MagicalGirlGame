using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainProjectile : MonoBehaviour
{
    [SerializeField]
    private float _speed;

    [SerializeField]
    private int _damage;

    [SerializeField]
    private float _knockbackMultiplier;

    [SerializeField]
    private LayerMask _collisionLayers;

    [SerializeField]
    private float _airTime = 10f;

    private IEnumerator _airTimeCoroutine = null;

    private Vector3 _direction = new Vector3(0, -1, 0);

    private void OnEnable()
    {
        _airTimeCoroutine = AirTimeCountdown(_airTime);
        StartCoroutine(_airTimeCoroutine);
    }

    private void OnDisable()
    {
        StopCoroutine(_airTimeCoroutine);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(_direction * _speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_collisionLayers == (_collisionLayers | (1 << other.gameObject.layer)))
        {
            other.gameObject.GetComponent<Health>()?.TakeDamage(new DamageParameters(
                _damage,
                gameObject,
                new Vector2(_direction.x != 0 ? _knockbackMultiplier : 0, _direction.y != 0 ? _knockbackMultiplier : 0)));

            Debug.LogFormat("RainProjectile: {0}, {1}", _direction.x != 0 ? _knockbackMultiplier : 0, _direction.y != 0 ? _knockbackMultiplier : 0);
        }

        gameObject.SetActive(false);
    }

    private IEnumerator AirTimeCountdown(float airTime)
    {
        yield return new WaitForSeconds(airTime);

        gameObject.SetActive(false);
    }
    public void SetDirection(Vector3 direction)
    {
        _direction = direction;
    }
}
