using UnityEngine;

public class BallonCreator : MonoBehaviour
{
    public GameObject ballonPrefab;
    public float spawnRate = 1f;
    public RINLBody body;

    private float nextSpawn = 0f;

    // Update is called once per frame
    void Update()
    {
        if (Time.time > nextSpawn)
        {
            nextSpawn = Time.time + spawnRate;

            // Get a position in the bounds
            float size = body.Bounds.size.x;
            float x = Random.Range(-size / 2, size / 2);

            var ballon = Instantiate(ballonPrefab, new(x, -1, 0), Quaternion.identity);
            ballon.transform.SetParent(transform);
        }
    }
}
