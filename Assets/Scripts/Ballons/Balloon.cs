using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Balloon : MonoBehaviour
{
    public float shrinkTime = 2f;
    public GameObject explosionPrefab;
    public GameObject balloonRenderer; 

    [HideInInspector]
    public int ballonType = -1;
    [HideInInspector]
    public BalloonCreator creator;

    private float shrinkSpeed;

    void Start()
    {
        shrinkSpeed = 1 / shrinkTime;
    }

    void Update()
    {
        balloonRenderer.transform.localScale -= shrinkSpeed * Time.deltaTime * Vector3.one;

        if (balloonRenderer.transform.localScale.x <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void Popper(int type)
    {
        if (ballonType == type || ballonType == 3)
        {
            Pop(true);
        }
        else if (ballonType == 2)
        {
            if (type == 0)
                ballonType = 1;
            else
                ballonType = 0;

            Pop(false);
        }
    }

    private void Pop(bool destroy)
    {
        var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        explosion.GetComponent<ParticleSystemRenderer>().material = balloonRenderer.GetComponent<Renderer>().material;

        if (destroy) {
            creator.balloonsPopped++;
            Destroy(gameObject);
        }
        else {
            explosion.transform.localScale *= 0.4f; 
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Balloon collided with: " + collision.gameObject.name);
    }
}
