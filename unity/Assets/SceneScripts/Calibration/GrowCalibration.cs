using UnityEngine;

public class GrowCalibration : StateMachineBehaviour
{
    private RawLandmarks lastLandmarks;
    private BodyCalibration calibration;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        calibration = new BodyCalibration(GameController.CalibrationData);
        lastLandmarks = GameController.Instance.JsConnector.LatestLandmarks;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Fetch the latest landmarks
        lastLandmarks = GameController.Instance.JsConnector.LatestLandmarks;
        if (lastLandmarks.world == null)
            return;

        // Check if the player has already moved the hand outside the bounds
        if (calibration.GrowBounds(lastLandmarks))
        {
            // Update the calibration data
            GameController.CalibrationData = calibration.data;

            // Proceed to the next state
            animator.SetTrigger("Progress");
        }
    }
}
