using UnityEngine;

public class SceneScript : MonoBehaviour
{
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
