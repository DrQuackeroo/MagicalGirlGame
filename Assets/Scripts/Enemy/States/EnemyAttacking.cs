using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAttacking : StateMachineBehaviour
{
    // The time until the attack ends. Counts down and exits state when it reaches 0.
    protected float _attackTimer;

    protected NavMeshAgent _navMeshAgent;
    protected Enemy _enemy;

    // Trigger to exit state
    protected readonly int _hashAttackEnded = Animator.StringToHash("AttackEnded");

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_navMeshAgent == null)
            _navMeshAgent = animator.GetComponent<NavMeshAgent>();
        if (_enemy == null)
            _enemy = animator.GetComponent<Enemy>();

        _navMeshAgent.isStopped = true;
        _attackTimer = _enemy.Attack();
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
        _navMeshAgent.isStopped = false;
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
