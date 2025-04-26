using UnityEngine;

[System.Serializable]
public struct DifficultyConfig
{
    public ObstacleType[] obstacleTypes;
    public float spawnTime;
    public float warningTime;
}

public class ObstacleCreator : MonoBehaviour
{
    public DifficultyConfig[] difficultyConfigs;
    public GameObject obstaclePrefab;
    public TMPro.TextMeshProUGUI scoreText;
    
    private float spawnTimer, startTime;
    private bool gaming = false;
    private int obstaclesSpawned = 0;
    private int obstaclesDodged = 0;
    private int lastSpawned = -1;

    private void Start()
    {
        StartGame();
    }

    public void Dodge()
    {
        obstaclesDodged++;
        scoreText.text = obstaclesDodged + "";
    }

    public void StartGame()
    {
        startTime = Time.time;
        gaming = true;
        spawnTimer = difficultyConfigs[0].spawnTime;
    }

    public void StopGame()
    {
        gaming = false;

        string extra_data = "{\"score\": " + obstaclesSpawned + "}";
        float now = Time.time;
        int elpased_seconds = (int)(now - startTime);

        GameController.Instance.SetActivityData("Dodge", elpased_seconds, obstaclesSpawned, extra_data);

        if (!Application.isEditor)
        {
            Debug.Log("Creating activity");
            GameController.Instance.CreateActivity();
        }
    }

    private void Update()
    {
        if (!gaming)
            return;
        
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            spawnTimer = difficultyConfigs[0].spawnTime;

            int type = Random.Range(0, difficultyConfigs[0].obstacleTypes.Length);
            while (type == lastSpawned)
            { type = Random.Range(0, difficultyConfigs[0].obstacleTypes.Length); }
            lastSpawned = type;

            GameObject obstacle = Instantiate(obstaclePrefab, transform.position, Quaternion.identity);
            var obstacleComponent = obstacle.GetComponent<Obstacle>();
            obstacleComponent.type = difficultyConfigs[0].obstacleTypes[type];
            obstacleComponent.Invoke("ActivateObstacle", difficultyConfigs[0].warningTime);

            obstaclesSpawned++;
        }
    }
}
