using UnityEngine;

public class DoneCalibration : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Set the calibration data to calibrated
        GameController.CalibrationData.calibrated = true;

        Debug.Log("imageGroundHeight: " + GameController.CalibrationData.imageGroundHeight);
        Debug.Log("bounds: " + GameController.CalibrationData.bounds);
        Debug.Log("worldImageRatio: " + GameController.CalibrationData.worldImageRatio);

        // Load the main menu scene
        SceneScript.Instance.SwitchScene("MainMenu");
    }
}
