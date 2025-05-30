using UnityEngine;
using UnityEngine.SceneManagement;

public class Calibrate : StateMachineBehaviour
{
    public string sceneName = "Calibration";
    private GameObject mainMenu;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mainMenu = SceneScript.Instance.GetObject("MainMenu");
        mainMenu.SetActive(false);
        SceneManager.LoadSceneAsync(sceneName);
    }
}
