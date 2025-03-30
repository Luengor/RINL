using UnityEngine;

public class DoneCalibration : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Set the calibration data to calibrated
        GameController.CalibrationData.calibrated = true;

        // Load the main menu scene
        SceneScript.Instance.SwitchScene("MainMenu");
    }
}
