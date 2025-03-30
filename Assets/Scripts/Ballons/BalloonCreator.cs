using UnityEngine;

public class BalloonCreator : MonoBehaviour
{
    public Bounds b;
    public int farEnoughAttempts = 3;
    public float minDistance = 0.5f;
    public float floorHeight = 0.3f;

    [System.Serializable]
    public struct BalloonAndPopper
    {
        public Material balloon;
        public GameObject popper;
    }

    public BalloonAndPopper[] balloonTypes;
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

            int type = 0;
            Vector3 pos = new(
                Random.Range(b.min.x, b.max.x),
                Random.Range(b.min.y + floorHeight, b.max.y),
                Random.Range(0.3f * b.min.z, 0.3f * b.max.z)
            );

            for (int i = 0; i < farEnoughAttempts; i++)
            {
                type = Random.Range(0, balloonTypes.Length);
                if (Vector3.SqrMagnitude(pos - balloonTypes[type].popper.transform.position) > minDistance * minDistance)
                    break;

                pos = new(
                    Random.Range(b.min.x, b.max.x),
                    Random.Range(b.min.y + floorHeight, b.max.y),
                    Random.Range(0.3f * b.min.z, 0.3f * b.max.z)
                );
            }

            GameObject balloon = Instantiate(balloonPrefab, pos, Quaternion.identity);

            balloon.GetComponent<Renderer>().material = balloonTypes[type].balloon;
            balloon.GetComponent<Balloon>().ballonType = type;
        }
    }
}
