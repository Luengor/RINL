using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class BalloonPopper : MonoBehaviour
{
    public int type = -1;
    public GameObject explosionPrefab;

    private BalloonCreator creator;

    private void Start()
    {
        creator = FindFirstObjectByType<BalloonCreator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Balloon balloon))
        {
            if (balloon.ballonType == type)
            {
                var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                explosion.GetComponent<ParticleSystemRenderer>().material = balloon.GetComponent<Renderer>().material;
                Destroy(other.gameObject);

                creator.balloonsPopped++;
            }
        }
    }
}
