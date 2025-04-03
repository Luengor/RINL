using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Balloon : MonoBehaviour
{
    public float shrinkTime = 2f;
    public GameObject explosionPrefab;

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
        transform.localScale -= shrinkSpeed * Time.deltaTime * Vector3.one;

        if (transform.localScale.x <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void Popper(int type)
    {
        Debug.Log(type + " popped " + ballonType);
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
        creator.balloonsPopped++;
        var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        explosion.GetComponent<ParticleSystemRenderer>().material = GetComponent<Renderer>().material;

        if (destroy)
            Destroy(gameObject);
        else
            explosion.transform.localScale *= 0.4f; 
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Balloon collided with: " + collision.gameObject.name);
    }
}
