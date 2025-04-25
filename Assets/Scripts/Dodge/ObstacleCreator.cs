using UnityEngine;

[System.Serializable]
public struct DifficultyConfig
{
    public ObstacleType[] obstacleTypes;
    public float spawnTime;
}

public class ObstacleCreator : MonoBehaviour
{
    public DifficultyConfig[] difficultyConfigs;
    public GameObject obstaclePrefab;
    
    private float spawnTimer, startTime;
    private bool gaming = false;
    private int obstaclesSpawned = 0;

    private void Start()
    {
        StartGame();
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
            RINLBody body = GameController.Instance.Body;
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
            GameObject obstacle = Instantiate(obstaclePrefab, transform.position, Quaternion.identity);
            obstacle.GetComponent<Obstacle>().type = difficultyConfigs[0].obstacleTypes[type];

            obstaclesSpawned++;
        }
    }
}
