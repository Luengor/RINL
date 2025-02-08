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
        // Move hand to the average of the wrist, index and pinky
        leftHand.localPosition = (
            bodyLandmarks[15].localPosition +
            bodyLandmarks[19].localPosition +
            bodyLandmarks[17].localPosition
        ) / 3 + points.localPosition;

        rightHand.localPosition = (
            bodyLandmarks[16].localPosition +
            bodyLandmarks[20].localPosition +
            bodyLandmarks[18].localPosition
        ) / 3 + points.localPosition;

        // Make them look at the index with up being the direction of the thumb
        leftHand.LookAt(bodyLandmarks[19].localPosition + points.localPosition, bodyLandmarks[21].localPosition - bodyLandmarks[15].localPosition);
        rightHand.LookAt(bodyLandmarks[20].localPosition + points.localPosition, bodyLandmarks[22].localPosition - bodyLandmarks[16].localPosition);

        // Move hips to the average of the hips
        hips.localPosition = (
            bodyLandmarks[23].localPosition +
            bodyLandmarks[24].localPosition
        ) / 2 + points.localPosition;

        Vector3 shoulderCenter = (bodyLandmarks[11].localPosition + bodyLandmarks[12].localPosition) * 0.5f;
        hips.up = shoulderCenter - hips.localPosition;
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
