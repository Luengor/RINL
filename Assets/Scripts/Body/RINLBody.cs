using System;
using System.Runtime.InteropServices;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

[Serializable]
public struct Landmark
{
    public float x;
    public float y;
    public float z;
}

public struct Landmarks
{
    public Landmark[] world;
    public Landmark[] image;
}


public abstract class RINLBody : MonoBehaviour
{
    [Header("Transforms")]
    public Transform head;
    public Transform rightHand, leftHand;
    public Transform hips;
    public Transform points;

    [Header("Settings")]
    public float positionScale = 1.0f;
    public bool flipX = true;
    public float lerpSpeed = 0.8f;
    public LayerMask groundLayer;
    
    /// Properties
    public Bounds Bounds {
        get {
            return bounds;
        }
    }

    /// Privatee
    protected readonly Transform[] bodyLandmarks = new Transform[33];

    protected bool hasData = false;
    protected Landmarks lastLandmarks = new();

    protected float lowestY = 0.0f, ground = 0;

    protected Vector3 hipPosition = Vector3.zero;
    protected Bounds bounds = new();

    [DllImport("__Internal")]
    protected static extern void sendToReact(string message);


    protected virtual void Start()
    {
        // Get the 33 body landmarks from the points object 
        for (int i = 0; i < 33; i++)
            bodyLandmarks[i] = points.GetChild(i);
    }

    private void FixedUpdate()
    {
        UpdateGroundHeight();

        if (hasData)
        {
            MoveBody();

            MoveHip();

            MoveBodyParts();
        }
    }

    protected virtual void OnDrawGizmos()
    {
        // Draw the hips
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(hips.position, 0.1f);
        Gizmos.DrawLine(hips.position, hips.position + hips.up * 0.5f);
        Gizmos.DrawLine(hips.position, hips.position + hips.forward * 0.5f);

        // Draw the head
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(head.position, 0.1f);
        Gizmos.DrawLine(head.position, head.position + head.up * 0.5f);
        Gizmos.DrawLine(head.position, head.position + head.forward * 0.5f);

        // Draw the ground
        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector3(hips.localPosition.x, ground, hips.localPosition.z), new Vector3(0.5f, 0.02f, 0.5f));

        // Draw the bounds
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(bounds.center + transform.position, bounds.size);
    }


    protected virtual void MoveHip()
    {
        // Get the average x position of the hips
        float landmarkHipX = ((lastLandmarks.image[23].x + lastLandmarks.image[24].x) * 0.5f - 0.5f) * positionScale;

        // Set the height of the hips to the ground + the lowest Y position
        float newHipY = ground - lowestY;

        hipPosition = new Vector3(landmarkHipX, newHipY, 0);
        points.localPosition = hipPosition;
    }

    protected void MoveBody()
    {
        // Get z average of the hips
        lowestY = float.MaxValue; 

        for (int i = 0; i < 33; i++)
        {
            Vector3 newPos = GetLandmarkPosition(i);
            bodyLandmarks[i].localPosition = newPos;

            if (newPos.y < lowestY)
                lowestY = newPos.y;
            
            // Only update the bounds if the landmark is on the image
            if (lastLandmarks.image[i].x > 0 && lastLandmarks.image[i].x < 1 && lastLandmarks.image[i].y > 0 && lastLandmarks.image[i].y < 1)
                bounds.Encapsulate(newPos + points.localPosition);
        }
    }


    protected abstract Vector3 GetLandmarkPosition(int index);

    protected abstract void UpdateGroundHeight();

    protected abstract void MoveBodyParts();


    public void SetBodyPosition(string landmarkString)
    {
        hasData = true;
        lastLandmarks = JsonUtility.FromJson<Landmarks>(landmarkString);

        // Invert X, Y and Z
        for (int i = 0; i < lastLandmarks.world.Length; i++)
        {
            if (flipX)
                lastLandmarks.world[i].x *= -1;

            lastLandmarks.world[i].y *= -1;
            lastLandmarks.world[i].z *= -1;

            // For the image landmarks, X and Y are in the range 0-1 and Z is the same as on the other landmarks
            if (flipX)
                lastLandmarks.image[i].x = 1 - lastLandmarks.image[i].x;
            lastLandmarks.image[i].y = 1 - lastLandmarks.image[i].y;
            lastLandmarks.image[i].z *= -1;
        }
    }
}
