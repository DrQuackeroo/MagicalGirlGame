using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base script for any projectile. "Thrower" refers to the object that creates this projectile. 
/// "Target" refers to an object that this projectile can damage.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Tooltip("How fast this projectile moves along the x-axis.")]
    [SerializeField] private float _speed;
    [Tooltip("How much damage this projectile does. Can be set in inspector or by the thrower.")]
    [SerializeField] private int _damage;
    [Tooltip("The names of object layers this projectile will attempt to damage.")]
    [SerializeField] private List<string> _targetLayerNames;

    // List of Layer names convered to LayerMask representation
    private List<LayerMask> _masks = new List<LayerMask>();
    private LayerMask _ground;
    // Object that created this projectile. Set after creation.
    private GameObject _thrower;
    private SpriteRenderer _spriteRenderer;

    void Awake()
    {
        _ground = LayerMask.NameToLayer("Ground");
        _spriteRenderer = GetComponent<SpriteRenderer>();

        foreach (string layer in _targetLayerNames)
            _masks.Add(LayerMask.NameToLayer(layer));
    }

    // Update is called once per frame
    void Update()
    {
        // Moves right (+X) by default unless the sprite is flipped.
        if (!_spriteRenderer.flipX)
            transform.Translate(_speed * Time.deltaTime, 0, 0);
        else
            transform.Translate(-_speed * Time.deltaTime, 0, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == _ground)
            OnHitGround();

        if (_masks.Contains(other.gameObject.layer))
            OnHitTarget(other.gameObject);
    }

    /// <summary>
    /// Initialize the parameters for this Projectile. Should be called right after instantiation.
    /// </summary>
    /// <param name="thrower">The GameObject that created this projectile.</param>
    /// <param name="isFlipped">True if sprite is flipped on the x-axis</param>
    /// <param name="damage">Damage Projectile does. Can be negative or blank to use prefab value.</param>
    /// <param name="speed">How fast Projectile moves. Can be negative or blank to use prefab value.</param>
    public void Initialize(GameObject thrower, bool isFlipped = false, int damage=-1, float speed=-1.0f)
    {
        _thrower = thrower;
        _spriteRenderer.flipX = isFlipped;
        if (damage >= 0.0f)
            _damage = damage;
        if (speed >= 0.0f)
            _speed = speed;
    }

    /// <summary>
    /// What the projectile does when it hits the environemnt. Destroys the projectile by default. Can be overriden for special behaior.
    /// </summary>
    protected void OnHitGround()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// What the projectile does when it hits a target. Destroys this projectile and damages the target by default. Can be overriden
    /// for special behavior
    /// </summary>
    /// <param name="target">The GameObject this Projectile hit.</param>
    protected void OnHitTarget(GameObject target)
    {
        Health enemyHealth = target.GetComponent<Health>();

        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(new DamageParameters(_damage, gameObject));
            Destroy(gameObject);
        }
    }
}
