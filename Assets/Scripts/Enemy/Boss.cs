using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Special subclass for the Boss. Assumed to be unmoving/doesn't use a NavMeshAgent component to move.
/// Boss has multiple Ability components, one for each attack.
/// Different from the base Enemy class, the Animator state machine handles almost all of the state transitions for the Boss.
/// </summary>
public class Boss : Enemy
{
    protected BasicAttackCombo[] _attacksList;
    protected Dictionary<string, Ability> _abilitiesDict = new Dictionary<string, Ability>();

    protected readonly int _hashAttackStateIndex = Animator.StringToHash("AttackStateIndex");

    protected override void Start()
    {
        base.Start();

        _attacksList = GetComponents<BasicAttackCombo>();
        foreach (Ability a in _attacksList)
        {
            _abilitiesDict.Add(a.GetName(), a);
        }

        // Hack. Manually flip the Boss to face left because sprites face right by default. Might change later once sprite is drawn.
        _isFacingRight = false;
        _spriteRenderer.flipX = !_isFacingRight;

        // TODO: Boss fight starts automatically for testing.
        Activate();
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
        _animator.SetBool(_hashPlayerHasBeenSighted, true);
    }

    /// <summary>
    /// Causes Boss to attack using a chosen Ability. Sets relevant values on Animator.
    /// </summary>
    /// <returns>The total duration of the executed attack.</returns>
    public override float Attack()
    {
        int attackIndexToPerform = Random.Range(0, _attacksList.Length);

        _attacksList[attackIndexToPerform].Activate(gameObject);
        _animator.SetInteger(_hashAttackStateIndex, attackIndexToPerform);

        return _attacksList[attackIndexToPerform].GetCurrentAttackDuration();
    }
}