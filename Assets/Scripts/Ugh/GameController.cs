using Unity.VisualScripting;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    public static CalibrationData CalibrationData = null;
    public JSConnector JsConnector { get; private set; }

    public RINLBody Body {
        get {
            if (body == null || !body.gameObject.activeInHierarchy)
                body = FindFirstObjectByType<RINLBody>(FindObjectsInactive.Exclude);

            if (body == null)
                body = FindFirstObjectByType<RINLBody>(FindObjectsInactive.Include);

            return body;
        }
    }

    private RINLBody body = null;
    private Activity activityData = new();
    private int score = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        JsConnector = GetComponent<JSConnector>(); 
    }


    public void ChangeScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void SetScore(int score, int duration, string extra_data)
    {
        // Save the duration and other data
        activityData.duration = duration;
        activityData.extra_data = extra_data;
        this.score = score;

        if (body == null)
            body = FindFirstObjectByType<RINLBody>(FindObjectsInactive.Include);
        
        // Get the activity points from the body
        activityData.activity_points = body.GetActivityPoints();
    }

    public int GetScore()
    {
        return score;
    }

    public Activity GetActivityData()
    {
        return activityData;
    }
}
