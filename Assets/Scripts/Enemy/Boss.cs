using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Special subclass for the Boss. Assumed to be unmoving/doesn't use a NavMeshAgent component to move.
/// 
/// === Ideas for attack architecture:
/// Each boss attack will be its own Script, maybe variations of BasicAttackCombo.
/// Enemy.Attack() will be overridden to launch an attack based on parameters. Or could be randomly chosen.
/// Because Attacks are MonoBehaviors, will need to be attached to an object.
///     Could have an ability holder child of the Boss.
///         Would be easier this way to find and get only the attacks, just get all children of the BossAbilityHolder.
///         Boss parent GameObject does not hold Attack scripts itself.
///         Attacking works similar to Player's abilities, except Ability scripts are a direct child of the Boss instead of another GameObject.
/// States - each Attack needs an associated state for animations/effects.
///     How to associate States in the Animator with components in the BossAbilityHolder?
///         States will need to be created separately from the abilities added to the Holder.
///         How does it know which attack state to transition to?
///             Each attack has associated int value = child number. Animator transitions will be activated depending on the value of an int flag.
///             Change Animator int property when corresponding attack is used.
/// </summary>
public class Boss : Enemy
{
    // GameObject that has components for each attack this Boss can perform.
    protected GameObject _bossAbilityHolder;
    protected Ability[] _abilitiesList;
    protected Dictionary<string, Ability> _abilitiesDict = new Dictionary<string, Ability>();

    protected override void Start()
    {
        base.Start();

        _bossAbilityHolder = transform.Find("BossAbilityHolder").gameObject;
        _abilitiesList = _bossAbilityHolder.GetComponents<Ability>();
        foreach (Ability a in _abilitiesList)
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
    /// Special update function. Launches attacks while remaining stationary.
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
    /// Causes Boss to attack using a chosen Ability.
    /// </summary>
    /// <returns>The total duration of the executed attack.</returns>
    public override float Attack()
    {
        // TODO: Change to random value or something. Add checks that are performed before certain attacks can be used.
        _abilitiesList[0].Activate(gameObject);
        //return _abilitiesList[0].GetCurrentAttackDuration();
        return 0.0f;
    }
}
