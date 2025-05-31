using UnityEngine;

public class DoneCalibration : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Set the calibration data to calibrated
        GameController.CalibrationData.calibrated = true;
        GameController.Instance.gameData[GameDataConstants.CalibratedKey] = true;

        // Load the main menu scene
        SceneScript.Instance.SwitchScene("MainMenu");
    }
}
