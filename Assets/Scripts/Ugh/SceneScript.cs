using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct AnimatorInt
{
    public string name;
    public int value;
}

[RequireComponent(typeof(Animator))]
public class SceneScript : MonoBehaviour
{
    public GameObject[] objects;
    public Animator Animator { get { return animator; } }
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
        SceneScript.Instance.Animator.SetBool(trigger, value);
    }

    public void ToggleBool(string trigger)
    {
        SceneScript.Instance.Animator.SetBool(trigger, SceneScript.Instance.Animator.GetBool(trigger) == false);
    }

    public void SetTrigger(string trigger)
    {
        SceneScript.Instance.Animator.SetTrigger(trigger);
    }

    public void SetFloat(string trigger, float value)
    {
        SceneScript.Instance.Animator.SetFloat(trigger, value);
    }

    public void SetInt(string trigger, int value)
    {
        SceneScript.Instance.Animator.SetInteger(trigger, value);
    }

    public void SetIntJson(string json)
    {
        var intValue = JsonUtility.FromJson<AnimatorInt>(json);
        SceneScript.Instance.Animator.SetInteger(intValue.name, intValue.value);
    }
}
