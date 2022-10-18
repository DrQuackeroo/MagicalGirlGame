using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttackCombo : MonoBehaviour
{
    [System.Serializable]
    public class BasicAttack
    {
        private BasicAttackCombo _combo;
        private BasicAttack _nextAttack;
        public string _name;
        [SerializeField] private int _damage;
        [SerializeField] private float _attackRadius;
        [SerializeField] private float _windUp;
        [SerializeField] private float _windDown;

        public void Initialize(BasicAttackCombo combo, BasicAttack nextAttack)
        {
            _combo = combo;
            _nextAttack = nextAttack;
        }

        public IEnumerator Attack()
        {
            yield return new WaitForSeconds(_windUp);

            Vector3 origin = _combo.transform.position;
            origin.x += _attackRadius * ((_combo._spriteRenderer.flipX) ? -1 : 1);

            Collider2D[] colliders = Physics2D.OverlapCircleAll(origin, _attackRadius, _combo._enemyLayers);

            // turn on and set line renderer showing this attack collider
            _combo.DrawCollider(origin, _attackRadius);

            Debug.LogFormat("BasicAttackCombo.Attack(): Attack '{0}' used", _name);

            if (colliders.Length > 0)
            {
                foreach (Collider2D c in colliders)
                {
                    Health enemyHealth = c.GetComponent<Health>();

                    if (enemyHealth != null)
                        enemyHealth.TakeDamage(_damage);
                }
            }

            yield return new WaitForSeconds(_windDown);

            // turn off line renderer showing this attack collider
            _combo.EraseCollider();

            _combo.OnAttackFinish(_nextAttack);
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
    private const float _comboResetTimer = .5f;
    public float _timeLapsed = 0f;

    private void Awake()
    {
        for (int i = 0; i < _comboList.Count; i++)
        {
            _comboList[i].Initialize(this, (i != _comboList.Count - 1 ? _comboList[i + 1] : null));
        }

        _comboStart = _comboList[0];

        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        // used to show colliders
        _line = GetComponent<LineRenderer>();
        _line.useWorldSpace = false;
        _line.startWidth = .1f;
        _line.endWidth = .1f;

    }

    private void Update()
    {
        if (_currentAttackState != null && _midAttackCoroutine == null)
        {
            _timeLapsed += Time.deltaTime;

            if (_timeLapsed >= _comboResetTimer)
            {
                ResetCombo();
            }
        }
    }

    // main player script can call this to use basic attack
    public void Attack()
    {
        // prevents player from spamming basic attack while already mid-animation in an attack
        if (_midAttackCoroutine != null)
            return;

        // decides whether or not to begin or continue the combo
        if (_currentAttackState == null)
        {
            _midAttackCoroutine = _comboStart.Attack();
            _currentAttackState = _comboStart;
        }
        else
        {
            _midAttackCoroutine = _currentAttackState.Attack();
        }

        StartCoroutine(_midAttackCoroutine);

        // should probably start animation here. either use the attack's string name or will have to modify BasicAttack class
    }

    private void ResetCombo()
    {
        _currentAttackState = null;
        _timeLapsed = 0f;
    }

    public void OnAttackFinish(BasicAttack nextAttack)
    {
        _currentAttackState = nextAttack;
        StopCoroutine(_midAttackCoroutine);
        _midAttackCoroutine = null;
        _timeLapsed = 0f;
    }

    public void DrawCollider(Vector3 origin, float radius)
    {
        if (!_showColliders)
            return;

        _line.enabled = true;

        var segments = 360;
        _line.positionCount = segments + 1;

        var pointCount = segments + 1;
        var points = new Vector3[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            var rad = Mathf.Deg2Rad * (i * 360f / segments);
            points[i] = new Vector3(Mathf.Sin(rad) * radius, Mathf.Cos(rad) * radius, 0) - (transform.position - origin);
        }
        _line.SetPositions(points);
    }

    public void EraseCollider()
    {
        _line.enabled = false;
    }

    // if staggering is implemented, main player script could call this to handle logic for when player is attacked midcombo
    public void OnInterrupt()
    {
        // will only implement if team decides they want stagger
        // _currentAttackState.OnInterrupt();
    }
}
