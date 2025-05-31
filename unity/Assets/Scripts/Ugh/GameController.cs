using System;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    public static CalibrationData CalibrationData = new();
    public JSConnector JsConnector { get; private set; }

    public bool startCalibrated = true;
    public Dictionary<string, object> gameData = new();

    public RINLBody Body
    {
        get
        {
            if (body == null || !body.gameObject.activeInHierarchy)
                body = FindFirstObjectByType<RINLBody>(FindObjectsInactive.Exclude);

            if (body == null)
                body = FindFirstObjectByType<RINLBody>(FindObjectsInactive.Include);

            return body;
        }
    }

    private RINLBody body = null;
    private Activity activityData = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (startCalibrated)
                CalibrationData.calibrated = true;

        }
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        JsConnector = GetComponent<JSConnector>();
    }

    public void Pause()
    {
        Time.timeScale = 0;
    }

    public void Resume()
    {
        Time.timeScale = 1;
    }

    public void ChangeScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void SetActivityData(string minigame, int duration, int score, string extra_data)
    {
        activityData.date = System.DateTime.Now;
        activityData.minigame = minigame;
        activityData.duration = duration;
        activityData.score = score;
        activityData.extra_data = extra_data;

        // Get the activity points from the body
        activityData.activity_points = Body.GetActivityPoints();
    }

    public void CreateActivity()
    {
        JsConnector.CreateActivity(this.activityData);
    }

    public Activity GetActivityData()
    {
        return activityData;
    }

    public T GetGameData<T>(string key, T defaultValue = default)
    {
        if (gameData.TryGetValue(key, out var value))
            return (T)value;
        
        return defaultValue;
    }
}
