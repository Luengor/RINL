using System;
using UnityEngine;

enum CalibrationState
{
    TPose,
    Done
}

public class Calibrator : MonoBehaviour
{
    public RINLBody body;
    public float stillTime = 3f, stillThreshold = 0.1f;

    private CalibrationState state = CalibrationState.TPose;
    private BodyCalibration calibration = new BodyCalibration();
    private Landmarks lastLandmarks = new();
    private float timeLeft;

    private void Start()
    {
        lastLandmarks = body.Landmakrs;
        timeLeft = stillTime;
    }

    void FixedUpdate()
    {
        Landmarks newLandmarks = body.Landmakrs;

        if (lastLandmarks.world == null)
        {
            lastLandmarks = newLandmarks; 
            return;
        }
        
        switch (state)
        {
            case CalibrationState.TPose:
                TPoseCalibration(newLandmarks);
                break;
            
            case CalibrationState.Done:
                break;
        }

        lastLandmarks = newLandmarks;
    }

    private void TPoseCalibration(Landmarks newLandmarks)
    {
        // Check if the body is in a T pose
        if (!IsTPose(newLandmarks))
        {
            timeLeft = stillTime;
            return;
        }

        // Check if the body is still
        float diff = 0;
        for (int i = 0; i < 33; i++)
        {
            diff += Vector3.SqrMagnitude(
                new Vector3(newLandmarks.world[i].x - lastLandmarks.world[i].x,
                            newLandmarks.world[i].y - lastLandmarks.world[i].y,
                            newLandmarks.world[i].z - lastLandmarks.world[i].z));
        }

        if (diff > stillThreshold * 33)
        {
            timeLeft = stillTime;
            return;
        }

        timeLeft -= Time.fixedDeltaTime;
        if (timeLeft <= 0)
        {
            calibration.InitialT(newLandmarks);
            state = CalibrationState.Done;
        }
    }

    private bool IsTPose(Landmarks landmarks)
    {
        // Left hand and right hand are at the same height
        if (Mathf.Abs(landmarks.image[15].y - landmarks.image[16].y) > 0.1f)
            return false;
        
        // Arms are straight
        if (Math.Abs(landmarks.image[15].y - landmarks.image[13].y) > 0.1f ||
            Math.Abs(landmarks.image[13].y - landmarks.image[11].y) > 0.1f ||
            Math.Abs(landmarks.image[16].y - landmarks.image[14].y) > 0.1f ||
            Math.Abs(landmarks.image[14].y - landmarks.image[12].y) > 0.1f)
            return false;

        // Legs are straight
        if (Math.Abs(landmarks.image[23].x - landmarks.image[25].x) > 0.1f ||
            Math.Abs(landmarks.image[25].x - landmarks.image[27].x) > 0.1f ||
            Math.Abs(landmarks.image[24].x - landmarks.image[26].x) > 0.1f ||
            Math.Abs(landmarks.image[26].x - landmarks.image[28].x) > 0.1f)
            return false;
        
        return true;
    }
}
