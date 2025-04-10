using System;
using UnityEngine;

[Serializable]
public class CalibrationData
{
    public bool calibrated = false;
    public float imageGroundHeight = 0;
    public Vector2 worldImageRatio = Vector2.zero;
    public Bounds bounds = new();

    public Landmarks TransformLandmarks(RawLandmarks rawLandmarks)
    {
        Landmarks landmarks = new()
        {
            points = new Vector3[Constants.LANDMARKS],
            groundHeight = imageGroundHeight * worldImageRatio.y
        };

        for (int i = 0; i < Constants.LANDMARKS; i++)
            landmarks.points[i] = rawLandmarks.world[i].ToVector3();

        // Calculate the hip position
        Vector2 imageHipPosition = (rawLandmarks.image[(int)LandmarkNames.LeftHip].ToVector2() + rawLandmarks.image[(int)LandmarkNames.RightHip].ToVector2()) / 2;
        landmarks.hipPosition = new Vector3(
            imageHipPosition.x * worldImageRatio.x,
            imageHipPosition.y * worldImageRatio.y,
            0
        );

        return landmarks;
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
    }

    public bool GrowBounds(RawLandmarks rawLandmarks)
    {
        Landmarks landmarks = data.TransformLandmarks(rawLandmarks);
        bool grown = false;

        // Only grow the bounds if the landmark is in the image
        for (int i = 0; i < Constants.LANDMARKS; i++)
            if (rawLandmarks.image[i].InImage())
            {
                Vector3 point = landmarks.points[i] + landmarks.hipPosition;
                if (point.y > landmarks.groundHeight && !data.bounds.Contains(point))
                {
                    data.bounds.Encapsulate(point);
                    grown = true;
                }
            }


        return grown;
    }

    public Vector3 GetCombinedWorldLandmark(Landmarks landmarks, int index)
    {
        return landmarks.points[index] + landmarks.hipPosition;
    }

    public bool IsFloating(RawLandmarks landmarks)
    {
        // Check if the body is floating
        for (int i = 0; i < 33; i++)
            if (landmarks.image[i].InImage())
                if (landmarks.image[i].y < data.imageGroundHeight)
                    return false;

        return true;
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
        float imageKneeDistance = Math.Abs(landmarks.image[(int)LandmarkNames.LeftKnee].y - landmarks.image[(int)LandmarkNames.LeftHip].y);

        float worldHipDistance = Math.Abs(landmarks.world[(int)LandmarkNames.LeftHip].x - landmarks.world[(int)LandmarkNames.RightHip].x);
        float worldShoulderDistance = Math.Abs(landmarks.world[(int)LandmarkNames.LeftShoulder].y - landmarks.world[(int)LandmarkNames.LeftHip].y);
        float worldKneeDistance = Math.Abs(landmarks.world[(int)LandmarkNames.LeftKnee].y - landmarks.world[(int)LandmarkNames.LeftHip].y);

        Vector2 initialRatio = new(worldHipDistance / imageHipDistance, (worldShoulderDistance / imageShoulderDistance + worldKneeDistance / imageKneeDistance) / 2);
        return initialRatio; 
    }
}
