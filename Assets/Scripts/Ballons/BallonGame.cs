using System;
using TMPro;
using UnityEngine;

public class BallonGame : StateMachineBehaviour
{
    public string timerObjectName = "Timer";

    private TextMeshProUGUI timerText;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Activate the scene 
        SceneScript.Instance.objects[0].SetActive(true);

        // Start the game
        SceneScript.Instance.objects[1].GetComponent<BalloonCreator>().StartGame();

        // Set the timer
        timerText = SceneScript.Instance.GetObject(timerObjectName).GetComponent<TextMeshProUGUI>();

        // Reset activity points
        GameController.Instance.Body.ResetActivityPoints();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Update the timer
        timerText.text = Math.Max(Math.Ceiling(stateInfo.length * (1 - stateInfo.normalizedTime)), 0).ToString();
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Stop the game
        SceneScript.Instance.objects[1].GetComponent<BalloonCreator>().StopGame();

        // Deactivate the scene
        SceneScript.Instance.objects[0].SetActive(false);
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
