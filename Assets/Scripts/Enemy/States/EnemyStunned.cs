using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStunned : StateMachineBehaviour
{
    private Rigidbody _rigidbody;
    private NavMeshAgent _navMeshAgent;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_rigidbody == null)
        {
            _rigidbody = animator.GetComponent<Rigidbody>();
            _navMeshAgent = animator.GetComponent<NavMeshAgent>();
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
        _rigidbody.AddForce(Vector3.up * 500.0f);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

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