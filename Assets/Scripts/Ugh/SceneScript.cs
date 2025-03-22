using UnityEngine;

public class SceneScript : MonoBehaviour
{
    public GameObject[] objects;

    public static SceneScript Instance { get; private set; }
    private Animator animator;

    void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);

        Instance = this;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public GameObject GetObject(int index)
    {
        return objects[index];
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

    public void SetBool(string trigger, bool value)
    {
        animator.SetBool(trigger, value);
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
