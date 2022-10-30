using System.Collections;
using System.Collections.Generic;
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
        public string _name;
        [SerializeField] private int _damage;
        [SerializeField] private AttackColliderInfo _attackColliders;
        [SerializeField] private float _windUp;
        [SerializeField] private float _windDown;

        public void Initialize(BasicAttackCombo combo, BasicAttack nextAttack)
        {
            _combo = combo;
            _nextAttack = nextAttack;
        }

        public IEnumerator Attack(GameObject owner)
        {
            yield return new WaitForSeconds(_windUp);

            List<Collider> uniqueEnemyColliders = new List<Collider>();

            for (int i = 0; i < _attackColliders._origins.Count; i++)
            {
                Vector3 origin = _combo.transform.position + new Vector3(_attackColliders._origins[i].x * ((_combo._spriteRenderer.flipX) ? -1 : 1), _attackColliders._origins[i].y, _attackColliders._origins[i].z);

                Collider[] enemyColliders = Physics.OverlapSphere(origin, _attackColliders._radiuses[i], _combo._enemyLayers);
                
                for (int j = 0; j < enemyColliders.Length; j++)
                {
                    if (!uniqueEnemyColliders.Contains(enemyColliders[j]))
                    {
                        uniqueEnemyColliders.Add(enemyColliders[j]);
                    }
                }
            }

            // turns on and set line renderer showing this attack collider
            _combo.DrawCollider(_attackColliders);
            Debug.LogFormat("BasicAttackCombo.Attack(): Attack '{0}' used", _name);
            
            if (uniqueEnemyColliders.Count > 0)
            {
                foreach (Collider c in uniqueEnemyColliders)
                {
                    Health enemyHealth = c.GetComponent<Health>();

                    if (enemyHealth != null)
                        enemyHealth.TakeDamage(_damage, owner);
                }
            }
            

            yield return new WaitForSeconds(_windDown);

            // turn off line renderer showing this attack collider
            _combo.EraseCollider();

            _combo.OnAttackFinish(_nextAttack);
        }

        public float GetDuration()
        {
            return _windUp + _windDown;
        }
    }

    public LayerMask _enemyLayers;
    [SerializeField] private bool _showColliders;
    private LineRenderer _line;
    [SerializeField] private List<BasicAttack> _comboList;
    private BasicAttack _currentAttackState = null;
    private IEnumerator _midAttackCoroutine = null;
    private BasicAttack _comboStart;

    // used to determine where player is facing
    private SpriteRenderer _spriteRenderer;

    // how long it takes before the combo cannot be continued and must be restarted
    [SerializeField] private float _comboResetTimer = .5f;
    private float _timeElapsed = 0f;

    private void Awake()
    {
        for (int i = 0; i < _comboList.Count; i++)
        {
            _comboList[i].Initialize(this, (i != _comboList.Count - 1 ? _comboList[i + 1] : null));
        }

        _comboStart = _comboList[0];

        _spriteRenderer = GetComponent<SpriteRenderer>();

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

        // should probably start animation here. either use the attack's string name or will have to modify BasicAttack class
    }

    private void ResetCombo()
    {
        _currentAttackState = null;
        _timeElapsed = 0f;
    }

    public void OnAttackFinish(BasicAttack nextAttack)
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
            float radius = attackColliders._radiuses[i];

            for (int j = 0; j < 361; j++)
            {
                var rad = Mathf.Deg2Rad * (j * 360f / 361);
                points[pointCount] = new Vector3(Mathf.Sin(rad) * radius, Mathf.Cos(rad) * radius, 0) + new Vector3(origin.x * (_spriteRenderer.flipX ? -1 : 1), origin.y, origin.z);

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
}
