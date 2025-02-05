using UnityEngine;

public class GrowWithBounds : MonoBehaviour
{
    public RINLBody body;
    public float extra = .3f;
    public bool scaleX = true, scaleY = false, scaleZ = true;
    public float smoothTime = 0.3f;

    private Vector3 speed;

    private void FixedUpdate()
    {
        if (body.Bounds.size != Vector3.zero)
        {
            Vector3 newSize = new Vector3(
                scaleX ? body.Bounds.size.x + extra : transform.localScale.x,
                scaleY ? body.Bounds.size.y + extra : transform.localScale.y,
                scaleZ ? body.Bounds.size.z + extra : transform.localScale.z
            );

            transform.localScale = Vector3.SmoothDamp(transform.localScale, newSize, ref speed, smoothTime);
        }
    }
}

