using System;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
struct Landmark
{
    public float x;
    public float y;
    public float z;
}

struct Landmarks
{
    public Landmark[] world;
    public Landmark[] image;
}


public class RINLBody : MonoBehaviour
{
    [Header("Transforms")]
    public Transform head;
    public Transform rightHand, leftHand;
    public Transform hips;
    public Transform points;

    [Header("Settings")]
    public float positionScale = 1.0f;
    [Range(0.0f, 1.0f)]
    public float lerpSpeed = 0.8f;
    

    /// Private
    private readonly Transform[] bodyLandmarks = new Transform[33];

    private bool hasData = false;
    private Landmarks lastLandmarks = new();

    private float lowestY = 0.0f, ground = 0;

    private Vector3 hipPosition = Vector3.zero;

    [DllImport("__Internal")]
    private static extern void sendToReact(string message);


    private void Start()
    {
        // Get the 33 body landmarks from the points object 
        for (int i = 0; i < 33; i++)
        {
            bodyLandmarks[i] = points.GetChild(i);
        }
    }

    private void FixedUpdate()
    {
        GetGroundHeight();

        if (hasData)
        {
            MoveBody();

            MoveHip();

            MoveBodyParts();
        }
    }

    private void OnDrawGizmos()
    {
        // Draw the hips
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(hips.position, 0.1f);
        Gizmos.DrawLine(hips.position, hips.position + hips.up * 0.5f);
        Gizmos.DrawLine(hips.position, hips.position + hips.right * 0.5f);
        Gizmos.DrawLine(hips.position, hips.position + hips.forward * 0.5f);

        // Draw the head
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(head.position, 0.1f);
        Gizmos.DrawLine(head.position, head.position + head.up * 0.5f);
        Gizmos.DrawLine(head.position, head.position + head.right * 0.5f);
        Gizmos.DrawLine(head.position, head.position + head.forward * 0.5f);

        // Draw the ground
        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector3(hips.localPosition.x, ground, hips.localPosition.z), new Vector3(0.5f, 0.02f, 0.5f));
    }


    private void GetGroundHeight()
    {
        // Raycast from the hips to the lowest Y position
        if (Physics.Raycast(hips.localPosition, Vector3.down, out RaycastHit hit))
        {
            ground = hit.point.y;
        }
    }

    private void MoveBody()
    {
        // Get z average of the hips
        lowestY = float.MaxValue; 

        for (int i = 0; i < 33; i++)
        {
            Vector3 lastPos = bodyLandmarks[i].localPosition;
            Vector3 landmarkPos = new Vector3(
                lastLandmarks.world[i].x,
                lastLandmarks.world[i].y,
                lastLandmarks.world[i].z
            ) * positionScale + hipPosition;

            Vector3 newPos = Vector3.Lerp(lastPos, landmarkPos, lerpSpeed);
            bodyLandmarks[i].localPosition = newPos;

            if (newPos.y < lowestY)
                lowestY = newPos.y;
        }
    }

    private void MoveHip()
    {
        // Get the average x position of the hips
        float landmarkHipX = ((lastLandmarks.image[23].x + lastLandmarks.image[24].x) * 0.5f - 0.5f) * positionScale;
        // float newHipX = landmarkHipX *  lerpSpeed + hips.localPosition.x * (1 - lerpSpeed);

        // Set the height of the hips to the ground + the lowest Y position
        float newHipY = ground - lowestY;

        hipPosition = new Vector3(landmarkHipX, newHipY, 0);
        points.localPosition = hipPosition;
    }

    private void MoveBodyParts()
    {
        // Move and rotate the hips
        hips.localPosition = (bodyLandmarks[24].localPosition + bodyLandmarks[23].localPosition) * 0.5f + points.localPosition;

        Vector3 shoulderCenter = (bodyLandmarks[11].localPosition + bodyLandmarks[12].localPosition) * 0.5f;
        Vector3 hipUp = shoulderCenter - hipPosition;
        Vector3 hipRight = bodyLandmarks[24].localPosition - bodyLandmarks[23].localPosition;

        // ah yes, math
        Vector3 forward = Vector3.Cross(hipRight, hipUp); 

        hips.LookAt(hips.localPosition + forward, hipUp);

        // Move and rotate the head 
        Vector3 headCenter = (bodyLandmarks[7].localPosition + bodyLandmarks[8].localPosition) * 0.5f;
        head.localPosition = headCenter + points.localPosition;
        Vector3 headForward = bodyLandmarks[0].localPosition - headCenter;
        Vector3 headRight = bodyLandmarks[8].localPosition - bodyLandmarks[7].localPosition;
        Vector3 headUp = Vector3.Cross(headForward, headRight);

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

    public void SetBodyPosition(string landmarkString)
    {
        hasData = true;
        lastLandmarks = JsonUtility.FromJson<Landmarks>(landmarkString);

        // Invert X, Y and Z
        for (int i = 0; i < lastLandmarks.world.Length; i++)
        {
            lastLandmarks.world[i].x *= -1;
            lastLandmarks.world[i].y *= -1;
            lastLandmarks.world[i].z *= -1;

            // For the image landmarks, X and Y are in the range 0-1 and Z is the same as on the other landmarks
            lastLandmarks.image[i].x = 1 - lastLandmarks.image[i].x;
            lastLandmarks.image[i].y = 1 - lastLandmarks.image[i].y;
            lastLandmarks.image[i].z *= -1;
        }
    }
}
