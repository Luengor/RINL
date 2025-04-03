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
        return (landmarks.image[(int)LandmarkNames.LeftAnkle].y + landmarks.image[(int)LandmarkNames.RightAnkle].y) / 2;
    }

    private Vector2 CalculateWorldImage(RawLandmarks landmarks)
    {
        // Using the hips and shoulders to calculate the ratio
        float imageHipDistance = Math.Abs(landmarks.image[24].x - landmarks.image[23].x);
        float imageShoulderDistance = Math.Abs(landmarks.image[11].y - landmarks.image[23].y);

        float worldHipDistance = Math.Abs(landmarks.world[24].x - landmarks.world[23].x);
        float worldShoulderDistance = Math.Abs(landmarks.world[11].y - landmarks.world[23].y);

        return new(worldHipDistance / imageHipDistance, worldShoulderDistance / imageShoulderDistance);
    }
}
