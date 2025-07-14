using System;
using UnityEngine;

[Serializable]
public class CalibrationData
{
    public bool calibrated = false;
    public float imageGroundHeight = 0.109f;
    public float worldFeetGroundOffset;
    public Vector2 worldImageRatio = new(2.25f, 2.11f);
    public Bounds bounds = new(
        new Vector3(0.13f, 0.95f, 0.03f),
        new Vector3(1.08f, 0.95f, 0.41f)
    );

    public Landmarks TransformLandmarks(RawLandmarks rawLandmarks)
    {

        Landmarks landmarks = new()
        {
            points = new Vector3[Constants.LANDMARKS],
            inBounds = new bool[Constants.LANDMARKS],
            groundHeight = imageGroundHeight * worldImageRatio.y + worldFeetGroundOffset,
            displacedAmount = GetDisplacedAmount(rawLandmarks)
        };

        // Calculate the hip position
        Vector2 imageHipPosition = (rawLandmarks.image[(int)LandmarkNames.LeftHip].ToVector2() + rawLandmarks.image[(int)LandmarkNames.RightHip].ToVector2()) / 2;
        landmarks.hipPosition = new Vector3(
            imageHipPosition.x * worldImageRatio.x,
            imageHipPosition.y * worldImageRatio.y,
            0
        );

        // Calculate the world coordinates of the landmarks and check if they are in the bounds
        for (int i = 0; i < Constants.LANDMARKS; i++)
        {
            landmarks.points[i] = rawLandmarks.world[i].ToVector3();
            landmarks.inBounds[i] = bounds.Contains(landmarks.points[i] + landmarks.hipPosition);
        }

        return landmarks;
    }
    public float GetDisplacedAmount(RawLandmarks landmarks)
    {
        return Math.Min(
            Math.Abs(landmarks.image[(int)LandmarkNames.LeftAnkle].y - imageGroundHeight),
            Math.Abs(landmarks.image[(int)LandmarkNames.RightAnkle].y - imageGroundHeight)
        );
    }

};

public class BodyCalibration
{
    public CalibrationData data;

    public BodyCalibration()
    {
        data = new CalibrationData();
    }

    public BodyCalibration(CalibrationData data)
    {
        this.data = data;
    }

    public void InitialT(RawLandmarks landmarks)
    {
        // Calculate the ratio between the image and world coordinates
        data.worldImageRatio = CalculateWorldImage(landmarks);

        // Calculate the ground height using the image landmarks
        data.imageGroundHeight = CalculateGroundHeight(landmarks);

        // Calculate the world ground height (relative to the hips) using the image ground height
        data.worldFeetGroundOffset = CalculateWorldFeetGroundOffset(landmarks);

        // Prepare the bounds to the full image (starting from the ground height)
        data.bounds.min = new Vector3(
            data.bounds.min.x,
            data.imageGroundHeight * data.worldImageRatio.y + data.worldFeetGroundOffset,
            data.bounds.min.z
        );

        data.bounds.max = new Vector3(
            data.bounds.min.x,
            data.worldImageRatio.y,
            data.bounds.min.z
        );
    }

    private bool leftHandOutside = false;
    private float lastGrowthTime = -1f;

    public bool GrowBounds(RawLandmarks rawLandmarks, float maxNonGrowthTime = 5f)
    {
        Landmarks landmarks = data.TransformLandmarks(rawLandmarks);

        // If the timer is -1, reset it to the current time
        if (lastGrowthTime < 0f)
            lastGrowthTime = Time.time;

        // If hand is outside the image or it has already been detected or the timer has passed, we don't want to grow the bounds
        if (leftHandOutside
            || Math.Abs(Math.Abs(rawLandmarks.image[(int)LandmarkNames.LeftWrist].x) - (rawLandmarks.image[0].ar * .5f)) < 0.1f
            || Time.time - lastGrowthTime > maxNonGrowthTime)
        {
            leftHandOutside = true;
            return true;
        }

        // Grow the bounds to both sides
        var point = landmarks.points[(int)LandmarkNames.LeftWrist] + landmarks.hipPosition;

        if (!data.bounds.Contains(point))
        {
            // Update the last growth time
            lastGrowthTime = Time.time;

            // Grow the bounds
            data.bounds.Encapsulate(point);
            data.bounds.Encapsulate(new Vector3(
                -point.x,
                point.y,
                point.z
            ));
        }

        return false;
    }

    public Vector3 GetCombinedWorldLandmark(Landmarks landmarks, int index)
    {
        return landmarks.points[index] + landmarks.hipPosition;
    }

    private float CalculateGroundHeight(RawLandmarks landmarks)
    {
        float initialHeight = (landmarks.image[(int)LandmarkNames.LeftAnkle].y + landmarks.image[(int)LandmarkNames.RightAnkle].y) / 2;
        return initialHeight;
    }

    private Vector2 CalculateWorldImage(RawLandmarks landmarks)
    {
        /*
         * Using the hips, shoulders and knees to calculate the ratio
         * TODO: Because how the camera works, the shoulder and knee ratios are not the same. In fact, the ratio changes when the
         *       thing to measure is closer to the center of the camera. This is because the camera is not orthographic.
         *       It would be a good idea to calculate a bunch of ratios and lerp between them depending on the vertical position of the
         *       landmark.
         */
        float imageHipDistance = Math.Abs(landmarks.image[(int)LandmarkNames.LeftHip].x - landmarks.image[(int)LandmarkNames.RightHip].x);
        float imageShoulderDistance = Math.Abs(landmarks.image[(int)LandmarkNames.LeftShoulder].y - landmarks.image[(int)LandmarkNames.LeftHip].y);
        float imageKneeDistance = Math.Abs(landmarks.image[(int)LandmarkNames.LeftKnee].y - landmarks.image[(int)LandmarkNames.LeftAnkle].y);

        float worldHipDistance = Math.Abs(landmarks.world[(int)LandmarkNames.LeftHip].x - landmarks.world[(int)LandmarkNames.RightHip].x);
        float worldShoulderDistance = Math.Abs(landmarks.world[(int)LandmarkNames.LeftShoulder].y - landmarks.world[(int)LandmarkNames.LeftHip].y);
        float worldKneeDistance = Math.Abs(landmarks.world[(int)LandmarkNames.LeftKnee].y - landmarks.world[(int)LandmarkNames.LeftAnkle].y);

        Vector2 initialRatio = new(worldHipDistance / imageHipDistance, (worldShoulderDistance / imageShoulderDistance + worldKneeDistance / imageKneeDistance) / 2);
        // Vector2 initialRatio = new(worldHipDistance / imageHipDistance, worldKneeDistance / imageKneeDistance);
        return initialRatio;
    }

    private float CalculateWorldFeetGroundOffset(RawLandmarks landmarks)
    {
        /** Compensate the ground height by calculating where the feet are and should be:
         *    1. Get the world ground height.
         *    2. Calculate the feet position in world coordinates using the hips
         *           (feet.world + hipPosition)
         *    3. The feet position in world coordinates should be the same as the image ground height.
         *    4. Correct the world ground height by the difference to ensure the feet are on the ground.
         */

        float imageHipPosition = (landmarks.image[(int)LandmarkNames.LeftHip].y + landmarks.image[(int)LandmarkNames.RightHip].y) / 2;
        float hipPosition = imageHipPosition * data.worldImageRatio.y;

        float groundHeight = data.imageGroundHeight * data.worldImageRatio.y;
        float feetPosition = Mathf.Min(landmarks.world[(int)LandmarkNames.LeftAnkle].y, landmarks.world[(int)LandmarkNames.RightAnkle].y) + hipPosition;

        float offset = feetPosition - groundHeight;
        return offset;
    }
}
