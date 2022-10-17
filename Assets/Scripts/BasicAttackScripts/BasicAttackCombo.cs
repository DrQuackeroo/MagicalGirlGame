using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttackCombo : MonoBehaviour
{
    public class BasicAttack
    {
        private BasicAttackCombo _combo;
        private BasicAttack _nextAttack;
        public string _name;
        private int _damage;
        private float _attackRadius;
        private float _windUp;
        private float _windDown;

        public BasicAttack(BasicAttackCombo combo, BasicAttack nextAttack, string name, int damage, float attackRadius, float windUp, float windDown)
        {
            _combo = combo;
            _nextAttack = nextAttack;
            _name = name;
            _damage = damage;
            _attackRadius = attackRadius;
            _windUp = windUp;
            _windDown = windDown;
        }

        public IEnumerator Attack()
        {
            yield return new WaitForSeconds(_windUp);

            Vector3 origin = _combo.transform.position;
            origin.x += _attackRadius * ((_combo._spriteRenderer.flipX) ? -1 : 1);

            Collider2D[] colliders = Physics2D.OverlapCircleAll(origin, _attackRadius, _combo._enemyLayers);

            Debug.LogFormat("BasicAttackCombo.Attack(): Attack '{0}' used", _name);

            if (colliders.Length > 0)
            {
                foreach (Collider2D c in colliders)
                {
                    Debug.LogFormat("BasicAttackCombo.Attack(): Enemy '{0}' hit", c.gameObject.name);
                    // to be implemented when other team programmers add health scripts for enemies
                    // healthComponent = c.GetComponent for health
                    // healthComponent.TakeDamage(damage);
                }
            }

            yield return new WaitForSeconds(_windDown);

            _combo.OnAttackFinish(_nextAttack);
        }
    }

    public LayerMask _enemyLayers;
    public BasicAttack _currentAttackState = null;
    private IEnumerator _midAttackCoroutine = null;
    private BasicAttack _comboStart;

    // used to determine where player is facing
    private SpriteRenderer _spriteRenderer;

    // how long it takes before the combo cannot be continued and must be restarted
    private const float _comboResetTimer = .5f;
    public float _timeLapsed = 0f;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // initializing the basic attacks of the combo. can be changed whenever
        BasicAttack thirdAttack = new BasicAttack(this, null, "Final Strike", 3, 2.5f, .1f, .25f);
        BasicAttack secondAttack = new BasicAttack(this, thirdAttack, "Slash 2", 1, 1.5f, .05f, .1f);
        BasicAttack firstAttack = new BasicAttack(this, secondAttack, "Slash 1", 1, 1.5f, .05f, .1f);
        
        // keeping a reference, so combo can be started with the first attack
        _comboStart = firstAttack;
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

    // if staggering is implemented, main player script could call this to handle logic for when player is attacked midcombo
    public void OnInterrupt()
    {
        // will only implement if team decides they want stagger
        // _currentAttackState.OnInterrupt();
    }
}
