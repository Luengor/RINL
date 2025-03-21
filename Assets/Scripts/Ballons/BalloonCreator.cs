using UnityEngine;

public class BalloonCreator : MonoBehaviour
{
    public Bounds b;
    public Material[] ballonTypes;
    public GameObject balloonPrefab;
    public AnimationCurve spawnTimeCurve;

    [HideInInspector]
    public int balloonsPopped = 0;

    private float spawnTimer, startTime;
    private bool gaming = false;


    private float GetSpawnTime()
    {
        return spawnTimeCurve.Evaluate(Time.time - startTime);
    }

    public void StartGame()
    {
        startTime = Time.time;
        spawnTimer = GetSpawnTime();
        gaming = true;

        b = GameController.Instance.Body.GetBounds();
    }

    public void StopGame()
    {
        gaming = false;
        GameController.Instance.SetScore(balloonsPopped, 60, "{\"score\": " + balloonsPopped + "}");    
    }

    // Update is called once per frame
    void Update()
    {
        if (!gaming)
            return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0)
        {
            spawnTimer = GetSpawnTime();

            Vector3 pos = new(
                Random.Range(b.min.x, b.max.x),
                Random.Range(b.min.y, b.max.y),
                Random.Range(b.min.z, b.max.z)
            );

            GameObject balloon = Instantiate(balloonPrefab, pos, Quaternion.identity);

            int type = Random.Range(0, ballonTypes.Length);
            balloon.GetComponent<Renderer>().material = ballonTypes[type];
            balloon.GetComponent<Balloon>().ballonType = type; 
        }
    }
}
