using System;
using UnityEngine;

[System.Serializable]
public struct CalibrationData
{
    public float imageGroundHeight;
    public Vector2 worldImageRatio;

    public CalibrationData(float groundHeight)
    {
        imageGroundHeight = 0;
        worldImageRatio = Vector2.zero;
    }
};

public class BodyCalibration
{
    public CalibrationData data;

    public BodyCalibration()
    {
        data = new CalibrationData(0.0f);
    }

    public void InitialT(Landmarks landmarks)
    {
        // Calculate the ratio between the image and world coordinates
        data.worldImageRatio = CalculateWorldImage(landmarks);
        Debug.Log(data.worldImageRatio);

        // Calculate the ground height using the image landmarks
        data.imageGroundHeight = CalculateGroundHeight(landmarks);
        Debug.Log(data.imageGroundHeight);
    }

    private float CalculateGroundHeight(Landmarks landmarks)
    {
        // Calculate the ground height using the image landmarks
        float leftFootHeight = landmarks.image[29].y;
        float rightFootHeight = landmarks.image[30].y;

        return (leftFootHeight + rightFootHeight) / 2;
    }

    private Vector2 CalculateWorldImage(Landmarks landmarks)
    {
        // Using the hips and shoulders to calculate the ratio
        float imageHipDistance = Math.Abs(landmarks.image[24].x - landmarks.image[23].x); 
        float imageShoulderDistance = Math.Abs(landmarks.image[11].y - landmarks.image[23].y); 

        float worldHipDistance = Math.Abs(landmarks.world[24].x - landmarks.world[23].x);
        float worldShoulderDistance = Math.Abs(landmarks.world[11].y - landmarks.world[23].y);

        return new (worldHipDistance / imageHipDistance, worldShoulderDistance / imageShoulderDistance);
    }
}
