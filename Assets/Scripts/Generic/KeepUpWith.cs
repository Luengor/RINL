using UnityEngine;

public class KeepUpWith: MonoBehaviour
{
    public Transform target;
    public bool positionX = true;
    public bool positionY = true;
    public bool positionZ = true;


    private Vector3 offset;

    void Start()
    {
        offset = transform.position - target.position;
    }

    void Update()
    {
        Vector3 newPosition = transform.position;

        if (positionX)
            newPosition.x = target.position.x + offset.x;

        if (positionY)
            newPosition.y = target.position.y + offset.y;

        if (positionZ)
            newPosition.z = target.position.z + offset.z;

        transform.position = newPosition;
    }
}
