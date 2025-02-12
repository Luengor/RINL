using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Balloon : MonoBehaviour
{
    public float shrinkTime = 2f;

    [HideInInspector]
    public int ballonType = -1;

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
}
