using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class BalloonPopper : MonoBehaviour
{
    public int type = -1;

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Balloon balloon))
            balloon.Popper(type);
    }
}
