using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main logic state for the Boss. Determines which attack to use by setting an Animator variable.
/// </summary>
public class BossActive : StateMachineBehaviour
{
    protected GameObject _player;
    protected GameObject _bossGameObject;
    protected Boss _boss;

    protected readonly int _hashAttackStateIndex = Animator.StringToHash("AttackStateIndex");
    protected readonly int _hashCurrentAttackDuration = Animator.StringToHash("CurrentAttackDuration");

    private void Awake()
    {
        _player = GameObject.FindWithTag("Player");
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_bossGameObject == null)
        {
            _bossGameObject = animator.gameObject;
            _boss = animator.GetComponent<Boss>();
        }

        // Perform a random attack and transition into that attack state.
        float currentAttackDuration = _boss.Attack();
        animator.SetFloat(_hashCurrentAttackDuration, currentAttackDuration);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
        
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
