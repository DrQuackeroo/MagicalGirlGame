using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fully activates the Boss once the entry animation finishes.
/// </summary>
public class BossRise : StateMachineBehaviour
{
    private CanvasGroup _bossUI;

    private readonly int _hashIsCameraShaking = Animator.StringToHash("IsCameraShaking");

    private void Awake()
    {
        if (_bossUI == null)
        {
            _bossUI = GameObject.Find("BossUI").GetComponent<CanvasGroup>();
            _bossUI.alpha = 0.0f;
        }
    }

    // Set Boss entrance effect variables.
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Camera.main.GetComponent<Animator>().SetBool(_hashIsCameraShaking, true);
        animator.GetComponent<Health>().isBlocking = true;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // The boss fight has started.
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _bossUI.alpha = 1.0f;
        Camera.main.GetComponent<Animator>().SetBool(_hashIsCameraShaking, false);
        animator.GetComponent<Health>().isBlocking = false;
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
