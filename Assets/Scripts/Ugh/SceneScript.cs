using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneScript : MonoBehaviour
{
    public GameObject[] objects;
    public static SceneScript Instance { get; private set; }
    private Animator animator;


    private void Awake()
    {
        if (Instance != null) {
            Debug.LogError("Multiple instances of SceneScript found.");
        }

        Instance = this;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public GameObject GetObject(string name)
    {
        foreach (var obj in objects)
        {
            if (obj.name == name)
                return obj;
        }

        Debug.LogError("Object not found: " + name);
        return null;
    }

    public void SwitchScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }

    public void SetBool(string trigger, bool value)
    {
        animator.SetBool(trigger, value);
    }

    public void ToggleBool(string trigger)
    {
        animator.SetBool(trigger, animator.GetBool(trigger) == false);
    }

    public void SetTrigger(string trigger)
    {
        animator.SetTrigger(trigger);
    }

    public void SetFloat(string trigger, float value)
    {
        animator.SetFloat(trigger, value);
    }
}
