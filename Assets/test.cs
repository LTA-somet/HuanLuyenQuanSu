using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class test : StateMachineBehaviour
{
    [SerializeField]
    private ControllerPracticeAKGun controllerPracticeAKGun;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("StateEnter");
        if (stateInfo.shortNameHash == Animator.StringToHash("Take 001"))
        {
            Debug.Log("dung");
        }


    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log("OnStateUpdate" + stateInfo.normalizedTime + layerIndex);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log()
        //Debug.Log(controllerPracticeAKGun.transform.localPosition);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log("move");
        // Implement code that processes and affects root motion
    }

    // OnStateIK is called right after Animator.OnAnimatorIK()
    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("IK");
        // Implement code that sets up animation IK (inverse kinematics)
    }
    private void Reset()
    {
        controllerPracticeAKGun = GameObject.Find("Sung AK").GetComponent<ControllerPracticeAKGun>();
    }
}
