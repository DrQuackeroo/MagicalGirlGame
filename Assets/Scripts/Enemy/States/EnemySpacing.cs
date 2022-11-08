using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Instead of moving directly to the Player, the Enemy distances themself a little bit from the Player before trying to attack again.
/// </summary>
public class EnemySpacing : StateMachineBehaviour
{
    [SerializeField] private float _spacingDistance = 7.5f;

    private GameObject _player;
    private GameObject _enemyGameObject;
    private NavMeshAgent _navMeshAgent;

    private readonly int _hashSpacingIsGood = Animator.StringToHash("SpacingIsGood");

    private void Awake()
    {
        _player = GameObject.FindWithTag("Player");
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_navMeshAgent == null)
        {
            _navMeshAgent = animator.GetComponent<NavMeshAgent>();
            _enemyGameObject = animator.gameObject;
        }

        if (_enemyGameObject.transform.position.x > _player.transform.position.x)
        {
            _navMeshAgent.SetDestination(new Vector3(_player.transform.position.x + _spacingDistance, _player.transform.position.y, 0.0f));
        }
        else
        {
            _navMeshAgent.SetDestination(new Vector3(_player.transform.position.x - _spacingDistance, _player.transform.position.y, 0.0f));
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance && 
            (!_navMeshAgent.hasPath || _navMeshAgent.velocity.magnitude == 0.0f))
        {
            animator.SetTrigger(_hashSpacingIsGood);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{

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
