using System;
using UnityEngine;

[Serializable]
public class CalibrationData
{
    public float imageGroundHeight = 0;
    public Vector2 worldImageRatio = Vector2.zero;
    public Bounds bounds = new();
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

    public void InitialT(Landmarks landmarks)
    {
        // Calculate the ratio between the image and world coordinates
        data.worldImageRatio = CalculateWorldImage(landmarks);
        Debug.Log(data.worldImageRatio);

        // Calculate the ground height using the image landmarks
        data.imageGroundHeight = CalculateGroundHeight(landmarks);
        Debug.Log(data.imageGroundHeight);
    }

    public bool GrowBounds(Landmarks landmarks)
    {
        bool grown = false;
        // Only grow the bounds if the landmark is in the image
        for (int i = 0; i < 33; i++)
            if (landmarks.image[i].InImage())
            {
                Vector3 point = GetCombinedWorldLandmark(landmarks, i);
                if (point.y > data.imageGroundHeight * data.worldImageRatio.y && !data.bounds.Contains(point))
                {
                    data.bounds.Encapsulate(point);
                    grown = true;
                }
            }

        
        return grown;
    }

    public Vector3 GetCombinedWorldLandmark(Landmarks landmarks, int index)
    {
        return landmarks.world[index].ToVector3() + new Vector3(
            landmarks.image[index].x * data.worldImageRatio.x,
            landmarks.image[index].y * data.worldImageRatio.y,
            0
        );
    }

    public bool IsFloating(Landmarks landmarks)
    {
        // Check if the body is floating
        for (int i = 0; i < 33; i++)
            if (landmarks.image[i].InImage())
                if (landmarks.image[i].y < data.imageGroundHeight) 
                    return false;

        return true;
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
