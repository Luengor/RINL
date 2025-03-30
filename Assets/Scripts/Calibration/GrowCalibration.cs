using UnityEngine;

public class GrowCalibration : StateMachineBehaviour
{
    public float stillTime = 3f, stillThreshold = 0.1f;
    private RawLandmarks lastLandmarks;
    private float timeLeft;
    private BodyCalibration calibration;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        calibration = new BodyCalibration(GameController.CalibrationData);
        lastLandmarks = GameController.Instance.JsConnector.LatestLandmarks;
        timeLeft = stillTime;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Fetch the latest landmarks
        lastLandmarks = GameController.Instance.JsConnector.LatestLandmarks;
        if (lastLandmarks.world == null)
            return;

        // Add the landmarks to the bounds
        if (calibration.GrowBounds(lastLandmarks))
        {
            // Reset the timer if the bounds are updated
            timeLeft = stillTime;
            return;
        }

        // Check if the user is still
        float diff = lastLandmarks.SqrDistance3(lastLandmarks);
        if (diff > stillThreshold * 33)
        {
            // Reset the timer if not still
            timeLeft = stillTime;
            return;
        }

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0)
        {
            // Update the calibration data
            GameController.CalibrationData = calibration.data;

            // Proceed to the next state
            animator.SetTrigger("Progress");
        }
    }
}
