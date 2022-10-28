using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFlierChasingPlayer : StateMachineBehaviour
{
    private GameObject _player;
    private EnemyFlier _enemyFlier;
    private float _attackRange;

    private void Awake()
    {
        _player = GameObject.FindWithTag("Player");
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_enemyFlier == null)
        {
            _enemyFlier = animator.GetComponent<EnemyFlier>();
            _attackRange = _enemyFlier.GetAttackRange();
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // If the Player is out of range, move closer until they are.
        if (Vector3.Distance(_player.transform.position, _enemyFlier.transform.position) > _attackRange)
        {
            _enemyFlier.SetDestination(_player.transform.position, _attackRange);
        }
        // If the Player is within range but Flier is not shooting at them, then the Player is at a different y-axis.
        // Move up or down until they are aligned, and move a little bit closer so the movement doesn't look weird.
        else
        {
            float xDifference = _player.transform.position.x - _enemyFlier.transform.position.x;
            _enemyFlier.SetDestination(new Vector3(_enemyFlier.transform.position.x + xDifference * 0.1f, _player.transform.position.y, 0.0f));
        }
    }

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
