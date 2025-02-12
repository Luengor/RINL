using UnityEngine;

public class BalloonCreator : MonoBehaviour
{
    public Bounds b;
    public Material[] ballonTypes;
    public GameObject balloonPrefab;
    public AnimationCurve spawnTimeCurve;

    private float spawnTimer, startTime;


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(b.center, b.size);
    }

    private void Start()
    {
        startTime = Time.time;
        spawnTimer = GetSpawnTime();
    }

    private float GetSpawnTime()
    {
        return spawnTimeCurve.Evaluate(Time.time - startTime);
    }

    // Update is called once per frame
    void Update()
    {
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
