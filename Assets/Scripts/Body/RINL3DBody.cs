using UnityEngine;

public class RINL3DBody : RINLBody 
{
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
        // Move and rotate the hips
        hips.localPosition = (bodyLandmarks[24].localPosition + bodyLandmarks[23].localPosition) * 0.5f + points.localPosition;

        Vector3 shoulderCenter = (bodyLandmarks[11].localPosition + bodyLandmarks[12].localPosition) * 0.5f;
        Vector3 hipUp = shoulderCenter - hipPosition;
        Vector3 hipRight = bodyLandmarks[24].localPosition - bodyLandmarks[23].localPosition;

        // ah yes, math
        Vector3 forward = Vector3.Cross(hipRight, hipUp) * (flipX ? 1 : -1);

        hips.LookAt(hips.localPosition + forward, hipUp);

        // Move and rotate the head 
        Vector3 headCenter = (bodyLandmarks[7].localPosition + bodyLandmarks[8].localPosition) * 0.5f;
        head.localPosition = headCenter + points.localPosition;
        Vector3 headForward = bodyLandmarks[0].localPosition - headCenter;
        Vector3 headRight = bodyLandmarks[8].localPosition - bodyLandmarks[7].localPosition;
        Vector3 headUp = Vector3.Cross(headForward, headRight) * (flipX ? 1 : -1);

        head.LookAt(head.localPosition + headForward, headUp);

        // Move and rotate the hands
        rightHand.localPosition = bodyLandmarks[16].localPosition + points.localPosition;
        Vector3 rhBack = bodyLandmarks[14].localPosition - bodyLandmarks[16].localPosition;
        Vector3 rhForward = bodyLandmarks[20].localPosition - bodyLandmarks[16].localPosition;
        Vector3 rhAvgForward = (-rhBack.normalized + rhForward.normalized) / 2;
        rightHand.LookAt(rightHand.localPosition + rhAvgForward, Vector3.up);

        leftHand.localPosition = bodyLandmarks[15].localPosition + points.localPosition;
        Vector3 lhBack = bodyLandmarks[13].localPosition - bodyLandmarks[15].localPosition;
        Vector3 lhForward = bodyLandmarks[19].localPosition - bodyLandmarks[15].localPosition;
        Vector3 lhAvgForward = (-lhBack.normalized + lhForward.normalized) / 2;
        leftHand.LookAt(leftHand.localPosition + lhAvgForward, Vector3.up);
    }

    protected override Vector3 GetLandmarkPosition(int index)
    {
        Vector3 lastPos = bodyLandmarks[index].localPosition;
        Vector3 landmarkPos = new Vector3(
            lastLandmarks.world[index].x,
            lastLandmarks.world[index].y,
            lastLandmarks.world[index].z
        ) * positionScale + hipPosition;

        return Vector3.Lerp(lastPos, landmarkPos, lerpSpeed);
    }
}
