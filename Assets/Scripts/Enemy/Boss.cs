using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Special subclass for the Boss. Assumed to be unmoving/doesn't use a NavMeshAgent component to move.
/// Boss has multiple Ability components, one for each attack.
/// Different from the base Enemy class, the Animator state machine handles almost all of the state transitions for the Boss.
/// </summary>
public class Boss : Enemy
{
    [Tooltip("The UI slider for the Boss HP bar")]
    [SerializeField] protected Slider _hpBar;

    protected List<BasicAttackCombo> _attacksList = new List<BasicAttackCombo>();
    protected Dictionary<string, Ability> _abilitiesDict = new Dictionary<string, Ability>();

    protected readonly int _hashAttackStateIndex = Animator.StringToHash("AttackStateIndex");
    protected readonly int _hashHasDied = Animator.StringToHash("HasDied");

    protected override void Start()
    {
        base.Start();

        _attacksList = new List<BasicAttackCombo>(GetComponents<BasicAttackCombo>());
        for (int i = 0; i < _attacksList.Count; i++)
        {
            if (_attacksList[i].isActiveAndEnabled)
            {
                _abilitiesDict.Add(_attacksList[i].GetName(), _attacksList[i]);
            }
            else
            {
                _attacksList.Remove(_attacksList[i]);
                i--;
            }
        }

        _health = GetComponent<Health>();
        _health.eventTookDamage.AddListener(UpdateHPBar);

        // Hack. Manually flip the Boss to face left because sprites face right by default. Might change later once sprite is drawn.
        _isFacingRight = false;
        _spriteRenderer.flipX = !_isFacingRight;
        _health.SetFacingRight(false);
    }

    /// <summary>
    /// Special update function. Does nothing to override default Enemy Update() function. Boss state machine transitions are handled by 
    /// the "Active" state in the Animator.
    /// </summary>
    protected override void Update()
    {
        
    }

    /// <summary>
    /// Call to have the Boss start attacking the Player. Can add other functionality that needs to happen when the boss fight starts.
    /// </summary>
    public void Activate()
    {
        gameObject.SetActive(true);
        _animator.SetBool(_hashPlayerHasBeenSighted, true);
    }

    /// <summary>
    /// Causes Boss to attack using a chosen Ability. Sets relevant values on Animator.
    /// </summary>
    /// <returns>The total duration of the executed attack.</returns>
    public override float Attack()
    {
        int attackIndexToPerform = Random.Range(0, _attacksList.Count);

        _attacksList[attackIndexToPerform].Activate(gameObject);
        _animator.SetInteger(_hashAttackStateIndex, attackIndexToPerform);

        return _attacksList[attackIndexToPerform].GetTotalAttackDuration();
    }

    private void UpdateHPBar()
    {
        _hpBar.value = ((float)_health.GetHealth() / (float)_health.GetMaxHealth()) * 100;
    }

    /// <summary>
    /// Called on the Update this Boss runs out of health. Begins end of game events.
    /// </summary>
    protected override void HasDied()
    {
        _animator.SetTrigger(_hashHasDied);
        transform.Find("BossParticles").gameObject.SetActive(true);
        transform.Find("BossParticles").SetParent(null);
    }
}
