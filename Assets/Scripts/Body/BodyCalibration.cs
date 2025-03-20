using System;
using UnityEngine;

[Serializable]
public class CalibrationData
{
    public bool calibrated = false;
    public float groundHeight = 0;
    public float realOverImageRatio = 1;
    public Bounds bounds = new();

    public Landmarks TransformLandmarks(RawLandmarks rawLandmarks)
    {
        // Apply the calibration data to the landmarks
        Landmarks landmarks = new()
        {
            points = new Landmark[33]
        };

        for (int i = 0; i < 33; i++)
        {
            landmarks.points[i].point = rawLandmarks.image[i].ToVector2() * this.realOverImageRatio;
            landmarks.points[i].inImage = rawLandmarks.image[i].InImage();
        }

        landmarks.groundHeight = this.groundHeight;

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

    public void InitialT(RawLandmarks landmarks)
    {
        // Calculate the ratio between the image and the real world using the height
        data.realOverImageRatio = GetRealOverImage(landmarks); 
        Debug.Log("Real / Image = " + data.groundHeight);

        // Calculate the ground height using the image landmarks
        data.groundHeight = CalculateGroundHeight(landmarks);
        Debug.Log("Ground height = " + data.groundHeight);
    }

    public bool GrowBounds(RawLandmarks landmarks)
    {
        return GrowBounds(data.TransformLandmarks(landmarks));
    }

    public bool GrowBounds(Landmarks landmarks)
    {
        bool grown = false;

        // Only grow the bounds if the landmark is in the image
        for (int i = 0; i < 33; i++)
            if (landmarks.points[i].inImage)
            {
                Vector2 point = landmarks.points[i].point;
                if (point.y > data.groundHeight && !data.bounds.Contains(point))
                {
                    data.bounds.Encapsulate(point);
                    grown = true;
                }
            }

        
        return grown;
    }

    public bool IsFloating(RawLandmarks landmarks)
    {
        // Check if the body is floating
        for (int i = 0; i < 33; i++)
            if (landmarks.image[i].InImage())
                if (landmarks.image[i].y < data.groundHeight) 
                    return false;

        return true;
    }

    private float CalculateGroundHeight(RawLandmarks landmarks)
    {
        // Calculate the ground height using the image landmarks
        float leftFootHeight = landmarks.image[(int)LandmarkNames.LeftAnkle].y;
        float rightFootHeight = landmarks.image[(int)LandmarkNames.RightAnkle].y;

        return (leftFootHeight + rightFootHeight) / 2;
    }

    private float GetRealOverImage(RawLandmarks landmarks)
    {
        // Get the height of the person in the image
        float height = GameController.Instance.JsConnector.CurrentShape.height;

        // Calculate the ratio between the image and the real world using the height
        float rightHeight = Math.Abs(landmarks.image[(int)LandmarkNames.RightEar].y - landmarks.image[(int)LandmarkNames.RightAnkle].y);
        float leftHeight = Math.Abs(landmarks.image[(int)LandmarkNames.LeftEar].y - landmarks.image[(int)LandmarkNames.LeftAnkle].y);
        float avgHeight = (rightHeight + leftHeight) / 2;

        // Asume the height from the ear to the top of the head is 15cm
        return (height - .15f) / avgHeight;
    }
}
