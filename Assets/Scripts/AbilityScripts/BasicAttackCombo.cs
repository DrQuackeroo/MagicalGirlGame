using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BasicAttackCombo : Ability
{
    [System.Serializable]
    public class AttackColliderInfo
    {
        public List<Vector3> _origins;
        public List<float> _radiuses;
    }

    [System.Serializable]
    public class BasicAttack
    {
        private BasicAttackCombo _combo;
        private BasicAttack _nextAttack;
        // Player GameObject reference. With the new Ability hierarchy, this will be different than _combo.GameObject and should be changed after initialization.
        private GameObject _player;
        private List<Collider> _hitColliders = new List<Collider>();
        public string _name;
        [SerializeField] private int _damage;
        [SerializeField] private AttackColliderInfo _attackColliders;
        [SerializeField] private float _windUp;
        [SerializeField] private float _windDown;
        [Tooltip("Knockback to apply if this attack hits. Positive x-values will push targets away from the attacker.")]
        [SerializeField] private Vector2 _knockbackImpulse = Vector2.zero;

        public List<Collider> GetHitColliders() { return _hitColliders; }
        public void SetPlayer(GameObject player) { _player = player; }
        public void SetDamage(int damage) { _damage = damage; }
        public void SetWindDown(float windDown) { _windDown = windDown; }

        public void Initialize(BasicAttackCombo combo, BasicAttack nextAttack)
        {
            _combo = combo;
            _nextAttack = nextAttack;
            _player = _combo.gameObject;
        }

        public IEnumerator Attack(GameObject owner)
        {
            yield return new WaitForSeconds(_windUp);

            _hitColliders = GetUniqueColliders();

            // turns on and set line renderer showing this attack collider
            _combo.DrawCollider(_attackColliders);

            // Actually damage hit objects.
            DamageHitColliders(_hitColliders, owner);
            
            yield return new WaitForSeconds(_windDown);

            // turn off line renderer showing this attack collider
            _combo.EraseCollider();

            _combo.OnAttackFinish(_nextAttack);
        }

        /// <summary>
        /// Special attack function that will only apply damage to targets that were not hit by a previous attack.
        /// </summary>
        /// <param name="owner">Who is attacking?</param>
        /// <param name="previousEnemies">Colliders that were hit by a previous attack or who should not be considered for this attack.</param>
        /// <returns></returns>
        public IEnumerator AttackNewCollidersOnly(GameObject owner, List<Collider> previousColliders)
        {
            yield return new WaitForSeconds(_windUp);

            // Get only the colliders that haven't been hit already.
            _hitColliders = GetUniqueColliders().Except(previousColliders).ToList();

            // turns on and set line renderer showing this attack collider
            _combo.DrawCollider(_attackColliders);

            // Actually damage hit objects.
            DamageHitColliders(_hitColliders, owner);

            yield return new WaitForSeconds(_windDown);

            // turn off line renderer showing this attack collider
            _combo.EraseCollider();

            _combo.OnAttackFinish(_nextAttack);
        }

        public float GetDuration()
        {
            return _windUp + _windDown;
        }

        /// <summary>
        /// Get a list of all unique colliders hit by this BasicAttack.
        /// </summary>
        private List<Collider> GetUniqueColliders()
        {
            List<Collider> uniqueEnemyColliders = new List<Collider>();

            for (int i = 0; i < _attackColliders._origins.Count; i++)
            {
                Vector3 origin = _player.transform.position + new Vector3(_attackColliders._origins[i].x * ((_combo._spriteRenderer.flipX) ? -1 : 1), _attackColliders._origins[i].y, _attackColliders._origins[i].z);

                Collider[] enemyColliders = Physics.OverlapSphere(origin, _attackColliders._radiuses[i], _combo._enemyLayers);

                for (int j = 0; j < enemyColliders.Length; j++)
                {
                    if (!uniqueEnemyColliders.Contains(enemyColliders[j]))
                    {
                        uniqueEnemyColliders.Add(enemyColliders[j]);
                    }
                }
            }

            return uniqueEnemyColliders;
        }

        /// <summary>
        /// Attempt to damage the colliders that were hit.
        /// </summary>
        /// <param name="uniqueEnemyColliders">Colliders to damage.</param>
        /// <param name="owner">Who is attacking?</param>
        private void DamageHitColliders(List<Collider> uniqueEnemyColliders, GameObject owner)
        {
            if (uniqueEnemyColliders.Count > 0)
            {
                foreach (Collider c in uniqueEnemyColliders)
                {
                    Health enemyHealth = c.GetComponent<Health>();

                    if (enemyHealth != null)
                        enemyHealth.TakeDamage(new DamageParameters(_damage, owner, _knockbackImpulse));
                }
            }
        }
    }

    public LayerMask _enemyLayers;
    [SerializeField] private bool _showColliders;
    protected LineRenderer _line;
    [SerializeField] protected List<BasicAttack> _comboList;
    protected BasicAttack _currentAttackState = null;
    protected IEnumerator _midAttackCoroutine = null;
    protected BasicAttack _comboStart;
    protected AudioSource _audioSource;

    // used to determine where player is facing
    protected SpriteRenderer _spriteRenderer;

    // how long it takes before the combo cannot be continued and must be restarted
    [SerializeField] protected float _comboResetTimer = .5f;
    protected float _timeElapsed = 0f;

    private void Awake()
    {
        for (int i = 0; i < _comboList.Count; i++)
        {
            _comboList[i].Initialize(this, (i != _comboList.Count - 1 ? _comboList[i + 1] : null));
        }

        _comboStart = _comboList[0];

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();

        // used to show colliders of attacks
        _line = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (_currentAttackState != null && _midAttackCoroutine == null)
        {
            _timeElapsed += Time.deltaTime;

            if (_timeElapsed >= _comboResetTimer)
            {
                ResetCombo();
            }
        }
    }

    // main player script can call this to use basic attack
    public override void Activate(GameObject player)
    {
        // prevents player from spamming basic attack while already mid-animation in an attack
        if (_midAttackCoroutine != null)
            return;

        // Play SFX
        if (_audioSource != null && _soundEffect != null)
        {
            _audioSource.PlayOneShot(_soundEffect, 1.0f);
        }

        // decides whether to begin or continue the combo
        if (_currentAttackState == null)
        {
            _midAttackCoroutine = _comboStart.Attack(gameObject);
            _currentAttackState = _comboStart;
        }
        else
        {
            _midAttackCoroutine = _currentAttackState.Attack(gameObject);
        }

        StartCoroutine(_midAttackCoroutine);

        // Play Animation. If this Ability has an animation, Animator should have a trigger with the same name as "_displayName".
        // Otherwise, set _displayName to "Empty".
        Animator animator = player.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger(_displayName);
        }
    }

    public override void Deactivate(GameObject player)
    {

    }

    private void ResetCombo()
    {
        _currentAttackState = null;
        _timeElapsed = 0f;
    }

    public virtual void OnAttackFinish(BasicAttack nextAttack)
    {
        _currentAttackState = nextAttack;
        StopCoroutine(_midAttackCoroutine);
        _midAttackCoroutine = null;
        _timeElapsed = 0f;
    }

    public void DrawCollider(AttackColliderInfo attackColliders)
    {
        if (!_showColliders)
            return;

        // set up for LineRenderer
        _line.useWorldSpace = false;
        _line.startWidth = .1f;
        _line.endWidth = .1f;

        _line.enabled = true;

        var segments = 361 * attackColliders._origins.Count;
        _line.positionCount = segments;

        var points = new Vector3[segments];

        int pointCount = 0;

        for (int i = 0; i < attackColliders._origins.Count; i++)
        {
            Vector3 origin = attackColliders._origins[i];
            float radius = attackColliders._radiuses[i] / transform.localScale.x;

            for (int j = 0; j < 361; j++)
            {
                var rad = Mathf.Deg2Rad * (j * 360f / 361);
                points[pointCount] = new Vector3(Mathf.Sin(rad) * radius, Mathf.Cos(rad) * radius, 0) + new Vector3(origin.x * (_spriteRenderer.flipX ? -1 : 1) / transform.localScale.x, origin.y / transform.localScale.y, origin.z);

                pointCount++;
            }
        }

        _line.SetPositions(points);
    }

    public void EraseCollider()
    {
        _line.enabled = false;
    }

    /// <returns>How long the current running attack takes in total, or 0.0f if no attack is running.</returns>
    public float GetCurrentAttackDuration()
    {
        if (_currentAttackState == null)
            return 0.0f;

        return _currentAttackState.GetDuration();
    }

    /// <returns>Sum of all individual attack durations.</returns>
    public float GetTotalAttackDuration()
    {
        float total = 0.0f;

        foreach (BasicAttack attack in _comboList)
        {
            total += attack.GetDuration();
        }

        return total;
    }
}
