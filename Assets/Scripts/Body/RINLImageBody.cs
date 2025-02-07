using UnityEngine;

public class RINLImageBody : RINLBody 
{
    public float zScale = 0.2f;

    protected override void Start()
    {
        // Init landmarks
        base.Start();
    }

    protected override void UpdateGroundHeight()
    {
        // Raycast from the hips to the lowest Y position
        if (Physics.Raycast(hips.localPosition, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
            ground = hit.point.y;
    }

    protected override void MoveBodyParts()
    {
        
    }

    protected override Vector3 GetLandmarkPosition(int index)
    {
        Vector3 lastPos = bodyLandmarks[index].localPosition;
        Vector3 newPos = new Vector3(
            (lastLandmarks.image[index].x * 2 - 1) * positionScale,
            lastLandmarks.image[index].y * positionScale,
            (lastLandmarks.image[index].z * 2 - 1) * zScale
        ) + hipPosition;

        return Vector3.Lerp(lastPos, newPos, Time.deltaTime * lerpSpeed);
    }
}
