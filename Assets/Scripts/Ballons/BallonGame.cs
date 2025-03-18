using System;
using UnityEngine;

public class BallonGame : StateMachineBehaviour
{
    public int score = 0;
    public int gameDuration = 60;
    public Color timerColor = new(1, 1, 1, 0.5f);
    private float timer = 0;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Activate the scene 
        SceneScript.Instance.objects[0].SetActive(true);

        // Start the game
        SceneScript.Instance.objects[1].GetComponent<BalloonCreator>().StartGame();

        var timerObj = SceneScript.Instance.objects[2].GetComponent<TMPro.TextMeshProUGUI>();
        timerObj.color = timerColor; 
        timerObj.enableAutoSizing = true;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer += Time.deltaTime;

        // Update the timer
        var timerObj = SceneScript.Instance.objects[2].GetComponent<TMPro.TextMeshProUGUI>();
        timerObj.text = Math.Max(gameDuration - (int)timer, 0).ToString();

        if (timer >= gameDuration)
        {
            animator.SetTrigger("GameDone");

            // Stop the game
            SceneScript.Instance.objects[1].GetComponent<BalloonCreator>().StopGame();

            // Deactivate the scene
            SceneScript.Instance.objects[0].SetActive(false);
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
