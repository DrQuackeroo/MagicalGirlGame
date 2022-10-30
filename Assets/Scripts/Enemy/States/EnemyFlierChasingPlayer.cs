using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFlierChasingPlayer : StateMachineBehaviour
{
    private GameObject _player;
    private EnemyFlier _enemyFlier;
    private float _attackRange;
    private float _minAttackRange;

    // Values set each Update are declared here.
    private bool _isFlierLeftOfPlayer;
    private float _distanceToPlayer;
    private Vector3 _newDestination;

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
            _minAttackRange = _enemyFlier.GetMinAttackRange();
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _isFlierLeftOfPlayer = _enemyFlier.transform.position.x < _player.transform.position.x;
        _distanceToPlayer = Vector3.Distance(_player.transform.position, _enemyFlier.transform.position);
        _newDestination = new Vector3(0.0f, _player.transform.position.y, 0.0f);

        // If the Player is out of range, move to the closest position where the Flier can shoot at the Player.
        if (_distanceToPlayer > _attackRange)
        {
            if (_isFlierLeftOfPlayer)
                _newDestination.x = _player.transform.position.x - _attackRange;
            else
                _newDestination.x = _player.transform.position.x + _attackRange;
        }
        // If the Player is too close to the Flier, move away.
        else if (_distanceToPlayer < _minAttackRange)
        {
            if (_isFlierLeftOfPlayer)
                _newDestination.x = _player.transform.position.x - _minAttackRange;
            else
                _newDestination.x = _player.transform.position.x + _minAttackRange;
        }
        // If the Flier is within range of the Player but not shooting at them, then the Player is at a different y-axis.
        // Move up or down until they are aligned.
        else
        {
            _newDestination.x = _enemyFlier.transform.position.x;
        }

        _enemyFlier.SetDestination(_newDestination);
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
