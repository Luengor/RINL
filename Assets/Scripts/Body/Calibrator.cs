using System;
using TMPro;
using UnityEngine;

enum CalibrationState
{
    TPose,
    Bounds,
    Done,
    Exit
}

public class Calibrator : MonoBehaviour
{
    public RINLBody body;
    public float stillTime = 3f, stillThreshold = 0.1f;
    public TextMeshProUGUI infoText;

    private CalibrationState state = CalibrationState.TPose;
    private readonly BodyCalibration calibration = new();
    private RawLandmarks lastLandmarks;
    private float timeLeft;

    private void Start()
    {
        timeLeft = stillTime;

        // Change body settings
        body.useGroundHeight = false;
        body.fixedPosition = true;

        // Set the camera position to have the body in the right
        Camera cam = Camera.main;
        float camWidth = cam.orthographicSize * cam.aspect;
        cam.orthographic = true;
        cam.transform.position = new Vector3(camWidth * 0.6f, 0, 10);
        cam.transform.eulerAngles = new Vector3(0, 180, 0);


        // Set the info text
        infoText.text = "Colócate justo enfrente de la cámara, lo suficientemente lejos como para que se vea tu cuerpo al completo.\nExtiende los brazos y pon las piernas a la altura de los hombros, haciendo una T con tu cuerpo.\nMantén la posición unos segundos.";
    }

    void FixedUpdate()
    {
        if (state == CalibrationState.Exit)
            return;

        RawLandmarks newLandmarks = GameController.Instance.JsConnector.LatestLandmarks;

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
            
            case CalibrationState.Bounds:
                BoundsCalibration(newLandmarks);
                break;
            
            case CalibrationState.Done:
                calibration.data.calibrated = true;

                GameController.CalibrationData = calibration.data;
                SceneScript.Instance.SetBool("Calibrated", true);
                break;
        }

        lastLandmarks = newLandmarks;
    }

    private void TPoseCalibration(RawLandmarks newLandmarks)
    {
        // Check if the body is in a T pose
        if (!IsTPose(newLandmarks))
        {
            timeLeft = stillTime;
            return;
        }

        // Check if the body is still
        float diff = newLandmarks.SqrDistance3(lastLandmarks);

        if (diff > stillThreshold * 33)
        {
            timeLeft = stillTime;
            return;
        }

        timeLeft -= Time.fixedDeltaTime;
        if (timeLeft <= 0)
        {
            calibration.InitialT(newLandmarks);

            state = CalibrationState.Bounds;
            timeLeft = stillTime;
            infoText.text = "Muevete hacia la derecha y la izquierda hasta donde puedas sin salir de la cámara.\nCuando acabes, mantén la posición unos segundos.";
        }
    }

    private void BoundsCalibration(RawLandmarks newLandmarks)
    {
        // Add the landmarks to the bounds
        if (calibration.GrowBounds(newLandmarks))
            timeLeft = stillTime;
        
        // Check if the body is still
        float diff = newLandmarks.SqrDistance3(lastLandmarks);
        if (diff > stillThreshold * 33)
        {
            timeLeft = stillTime;
            return;
        }
        
        timeLeft -= Time.fixedDeltaTime;
        if (timeLeft <= 0)
        {
            state = CalibrationState.Done;
            infoText.text = "Calibración completada.";
        }
    }

    private bool IsTPose(RawLandmarks landmarks)
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
