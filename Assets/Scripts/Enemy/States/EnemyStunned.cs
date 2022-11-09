using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStunned : StateMachineBehaviour
{
    private Rigidbody _rigidbody;
    private NavMeshAgent _navMeshAgent;
    private Enemy _enemy;
    private Health _health;
    private float _currentStunTime = 0.0f;
    private float _minimumStunTime = 0.1f;

    private readonly int _hashStunnedEnd = Animator.StringToHash("StunnedEnd");

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_rigidbody == null)
        {
            _rigidbody = animator.GetComponent<Rigidbody>();
            _navMeshAgent = animator.GetComponent<NavMeshAgent>();
            _enemy = animator.GetComponent<Enemy>();
            _health = animator.GetComponent<Health>();
        }

        // NavMeshAgent needs to be disabled and Rigidbody.isKinematic needs to be set to false for physics-based forces to be applied.
        // Reverse the process to return to NavMesh movement.
        // For example, if the Enemy is launched by a Player attack, Enemy movement would switch to being controlled by physics.
        // https://docs.unity3d.com/Manual/nav-MixingComponents.html for more details.

        //GetComponent<NavMeshAgent>().enabled = false;
        //GetComponent<Rigidbody>().isKinematic = false;
        //GetComponent<Rigidbody>().AddForce(Vector3.up * 1000.0f);
        _navMeshAgent.enabled = false;
        _rigidbody.isKinematic = false;
        _rigidbody.AddForce(_health.GetKnockbackToApply(), ForceMode.Impulse);
        _currentStunTime = 0.0f;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Enemy resumes movement once they stop moving. Should probably be modified later because it doesn't work in all cases.
        if (_currentStunTime > _minimumStunTime && _rigidbody.velocity.sqrMagnitude == 0.0f)
        {
            animator.SetTrigger(_hashStunnedEnd);
        }
        else
        {
            _currentStunTime += Time.deltaTime;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Exit state");
        if (_navMeshAgent != null)
            _navMeshAgent.enabled = true;
        if (_rigidbody != null)
            _rigidbody.isKinematic = true;
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
