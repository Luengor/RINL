using UnityEngine;

public class RotateAround : MonoBehaviour
{
    public Transform target;
    public float speed = 1.0f;

    void Update()
    {
        transform.RotateAround(target.position, Vector3.up, speed * Time.deltaTime);
    }
}
