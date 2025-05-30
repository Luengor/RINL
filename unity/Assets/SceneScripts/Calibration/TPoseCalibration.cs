using System;
using UnityEngine;

public class TPoseCalibration : StateMachineBehaviour
{
    public float stillTime = 3f, stillThreshold = 0.1f;
    private RawLandmarks lastLandmarks;
    private float timeLeft;
    private BodyCalibration calibration;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        RINLBody body = GameController.Instance.Body;

        // Set body properties 
        body.gameObject.SetActive(true);
        body.fixedPosition = true;

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

        // Check if the user is still
        float diff = lastLandmarks.SqrDistance3(lastLandmarks);
        if (diff > stillThreshold * 33)
        {
            // Reset the timer if not still
            timeLeft = stillTime;
            return;
        }

        // Check if the user is in T-Pose
        if (!IsTPose(lastLandmarks))
        {
            // Reset the timer if not in T-Pose
            timeLeft = stillTime;
            return;
        }

        // Decrease the timer
        timeLeft -= Time.deltaTime;

        // If the timer reaches zero, proceed to the next state
        if (timeLeft <= 0)
        {
            calibration.InitialT(lastLandmarks);

            GameController.CalibrationData = calibration.data;

            // Proceed to the next state
            animator.SetTrigger("Progress");
        }
    }

    private bool IsTPose(RawLandmarks landmarks)
    {
        // Left hand and right hand are at the same height
        if (Mathf.Abs(landmarks.image[15].y - landmarks.image[16].y) > 0.1f)
            return false;

        // Arms are straight
        if (Math.Abs(landmarks.image[15].y - landmarks.image[13].y) > 0.1f ||
            Math.Abs(landmarks.image[13].y - landmarks.image[11].y) > 0.1f ||
            Math.Abs(landmarks.image[16].y - landmarks.image[14].y) > 0.1f ||
            Math.Abs(landmarks.image[14].y - landmarks.image[12].y) > 0.1f)
            return false;

        // Legs are straight
        if (Math.Abs(landmarks.image[23].x - landmarks.image[25].x) > 0.1f ||
            Math.Abs(landmarks.image[25].x - landmarks.image[27].x) > 0.1f ||
            Math.Abs(landmarks.image[24].x - landmarks.image[26].x) > 0.1f ||
            Math.Abs(landmarks.image[26].x - landmarks.image[28].x) > 0.1f)
            return false;

        return true;
    }
}
