using UnityEngine;

public class BalloonCreator : MonoBehaviour
{
    [Tooltip("Number of attempts to find a position far enough from poppers")]
    public int farEnoughAttempts = 3;
    [Tooltip("Minimum distance between balloons and poppers")]
    public float minDistance = 0.5f;
    [Tooltip("Minimum height of the balloon spawn area")]
    public float floorHeight = 0.3f;
    [Tooltip("Minimum time left to instantly spawn a balloon after popping one")]
    public float minSkipTime = 0.2f;

    [System.Serializable]
    public struct BalloonAndPopper
    {
        public Material balloon;
        public GameObject popper;
    }

    [Tooltip("Left balloon, right balloon, double balloon, common balloon")]
    public BalloonAndPopper[] balloonTypes;
    public GameObject balloonPrefab;
    public AnimationCurve spawnTimeCurve;

    private int balloonsPopped = 0;

    private float spawnTimer, startTime;
    private bool gaming = false;
    private int difficulty = 0;
    private Bounds b;


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

        difficulty = SceneScript.Instance.Animator.GetInteger("difficulty");
    }

    public void StopGame()
    {
        gaming = false;

        string extra_data = "{\"score\": " + balloonsPopped + "}";
        GameController.Instance.SetActivityData("Balloons", 60, balloonsPopped, extra_data);

        if (!Application.isEditor)
        {
            RINLBody body = GameController.Instance.Body;
            Debug.Log("Creating activity");
            GameController.Instance.CreateActivity();
        }
    }

    public void BalloonPopped()
    {
        balloonsPopped++;

        if (difficulty == 3 && spawnTimer > minSkipTime && spawnTimer < GetSpawnTime() - minSkipTime)
            spawnTimer = 0;
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

            int type = GetBalloonType();
            Vector3 pos = new(
                Random.Range(b.min.x, b.max.x),
                Random.Range(b.min.y + floorHeight, b.max.y),
                Random.Range(0.3f * b.min.z, 0.3f * b.max.z)
            );

            for (int i = 0; i < farEnoughAttempts; i++)
            {
                type = GetBalloonType(); 
                if (type > 1)
                {
                    // Check distance to both poppers 
                    if (Vector3.SqrMagnitude(pos - balloonTypes[0].popper.transform.position) > minDistance * minDistance &&
                        Vector3.SqrMagnitude(pos - balloonTypes[1].popper.transform.position) > minDistance * minDistance)
                        break;
                }
                else
                {
                    // Check distance to 1 popper
                    if (Vector3.SqrMagnitude(pos - balloonTypes[type].popper.transform.position) > minDistance * minDistance)
                        break;
                }

                pos = new(
                    Random.Range(b.min.x, b.max.x),
                    Random.Range(b.min.y + floorHeight, b.max.y),
                    Random.Range(0.3f * b.min.z, 0.3f * b.max.z)
                );
            }

            GameObject balloonObj = Instantiate(balloonPrefab, pos, Quaternion.identity);
            Balloon balloon = balloonObj.GetComponent<Balloon>();

            balloon.balloonRenderer.GetComponent<Renderer>().material = balloonTypes[type].balloon;
            balloon.ballonType = type;
            balloon.creator = this;
        }
    }

    private int GetBalloonType()
    {
        switch (difficulty)
        {
            case 1:
                return 3;
            
            case 2:
                return Random.Range(0, 2);
            
            case 3:
                return Random.Range(0, 3);
            
            default:
                Debug.LogError("Difficulty not set");
                return 3; 
        }
    }
}
