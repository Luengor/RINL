using UnityEngine;

public class ButtonsHelp : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Check if the button tutorial has been seen
        bool hasSeenButtonTutorial = PlayerPrefs.GetInt(GameDataConstants.HasSeenButtonTutorialKey, 0) == 1;
        if (hasSeenButtonTutorial)
        {
            // Skip the animation
            animator.SetBool("SkipButtonTutorial", true);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // The tutorial has been seen, so set the PlayerPrefs key
        PlayerPrefs.SetInt(GameDataConstants.HasSeenButtonTutorialKey, 1);
        PlayerPrefs.Save();
    }
}
