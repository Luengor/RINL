using System;
using UnityEngine;

public class RINLImageBody : RINLBody 
{
    public float zScale = 0.2f;

    public float x1 = 1, x2 = 1, x3 = 1;
    public float calibration_m = 1, calibration_b = 0;

    protected override void Start()
    {
        // Init landmarks
        base.Start();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            x1 = lastLandmarks.image[23].x;

        if (Input.GetMouseButtonDown(1))
        {
            x2 = lastLandmarks.image[24].x;
            x3 = lastLandmarks.image[23].x;

            calibration_m = 2 * x1 / (x3 - x2);
            calibration_b = x1 * (1 - calibration_m);
        }
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
            lastLandmarks.image[index].x,
            lastLandmarks.image[index].y,
            lastLandmarks.image[index].z * zScale
        ) * positionScale;

        newPos.x = Math.Sign(newPos.x) * (calibration_m * Math.Abs(newPos.x) + calibration_b);

        return Vector3.Lerp(lastPos, newPos, Time.deltaTime * lerpSpeed);
    }
}
