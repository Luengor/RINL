using UnityEngine;

public class Ballon : MonoBehaviour
{
    public float speed = 1f;
    public float maxHight = 10f;
    
    private void FixedUpdate()
    {
        transform.position += speed * Time.deltaTime * Vector3.up;
        if (transform.position.y > maxHight)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            Destroy(gameObject);
    }
}
