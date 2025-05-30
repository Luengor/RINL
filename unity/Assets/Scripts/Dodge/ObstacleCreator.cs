using UnityEngine;

[System.Serializable]
public struct DifficultyConfig
{
    public ObstacleType[] obstacleTypes;
    [Tooltip("Time between obstacles")]
    public AnimationCurve spawnCurve;
    [Tooltip("Time before the obstacle is active")]
    public AnimationCurve warningCurve;
}

public class ObstacleCreator : MonoBehaviour
{
    public DifficultyConfig[] difficultyConfigs;
    public GameObject obstaclePrefab;
    public TMPro.TextMeshProUGUI scoreText;
    
    private float spawnTimer, startTime;
    public bool Gaming {
        get;
        private set;
    } = false;
    private int obstaclesDodged = 0;
    private int lastSpawned = -1;
    private int difficultyIndex = 0;

    public void Dodge()
    {
        obstaclesDodged++;
        scoreText.text = obstaclesDodged + "";
    }

    public void StartGame()
    {
        // Get and reset the difficulty config
        difficultyIndex = SceneScript.Instance.Animator.GetInteger("difficulty") - 1;
        if (difficultyIndex < 0 || difficultyIndex >= difficultyConfigs.Length)
            Debug.LogError("Invalid difficulty index: " + difficultyIndex);
        
        SceneScript.Instance.SetInt("difficulty", 0);

        // Reset the game state
        startTime = Time.time;
        scoreText.text = "0";
        obstaclesDodged = 0;
        spawnTimer = difficultyConfigs[difficultyIndex].spawnCurve.Evaluate(0);

        // Start the game
        Gaming = true;
    }

    public void StopGame()
    {
        Gaming = false;
        SceneScript.Instance.SetTrigger("GameDone");

        string extra_data = "{\"score\": " + obstaclesDodged + "}";
        float now = Time.time;
        int elpased_seconds = (int)(now - startTime);

        GameController.Instance.SetActivityData("Dodge", elpased_seconds, obstaclesDodged, extra_data);

        if (!Application.isEditor)
        {
            Debug.Log("Creating activity");
            GameController.Instance.CreateActivity();
        }
    }

    private void Update()
    {
        if (!Gaming)
            return;
        
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            var difficulty = difficultyConfigs[difficultyIndex];
            var elapsed = Time.time - startTime;

            // Reset the spawn timer
            spawnTimer = difficulty.spawnCurve.Evaluate(elapsed);

            // Choose a random obstacle type, ensuring it's not the same as the last one 
            int type = Random.Range(0, difficulty.obstacleTypes.Length);
            while (type == lastSpawned)
            { type = Random.Range(0, difficulty.obstacleTypes.Length); }
            lastSpawned = type;

            // Spawn the obstacle
            GameObject obstacle = Instantiate(obstaclePrefab, transform.position, Quaternion.identity);

            // Set things on the obstacle 
            var obstacleComponent = obstacle.GetComponent<Obstacle>();
            obstacleComponent.type = difficulty.obstacleTypes[type];
            obstacleComponent.warningTime = difficulty.warningCurve.Evaluate(elapsed);
            obstacleComponent.duration = spawnTimer - obstacleComponent.warningTime;
        }
    }
}
