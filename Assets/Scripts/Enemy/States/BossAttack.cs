using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Behavior for when the Boss performs an attack. Exits state based off a timer.
/// </summary>
public class BossAttack : StateMachineBehaviour
{
    // The time until the attack ends. Counts down and exits state when it reaches 0.
    protected float _attackTimer;
    protected Boss _boss;

    // Trigger to exit state
    protected readonly int _hashAttackEnded = Animator.StringToHash("AttackEnded");
    protected readonly int _hashAttackStateIndex = Animator.StringToHash("AttackStateIndex");
    protected readonly int _hashCurrentAttackDuration = Animator.StringToHash("CurrentAttackDuration");

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_boss == null)
            _boss = animator.GetComponent<Boss>();

        // Timer to transition out is an Animator variable set in BossActive and read here.
        animator.SetInteger(_hashAttackStateIndex, -1);
        _attackTimer = animator.GetFloat(_hashCurrentAttackDuration);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _attackTimer -= Time.deltaTime;
        if (_attackTimer <= 0.0f)
            animator.SetTrigger(_hashAttackEnded);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

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
