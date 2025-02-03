using System;
using TMPro;
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
    public Transform hips;
    public Transform points;
    public Transform head;

    [Header("Settings")]
    public float positionScale = 1.0f;
    [Range(0.0f, 1.0f)]
    public float lerpSpeed = 0.8f;

    [Header("Prefabs")]
    public GameObject bodyLandmarkPrefab;
    public GameObject edgePrefab;

    [Header("Other things")]
    public Material leftMaterial;
    public Material rightMaterial;
    public TextMeshProUGUI debugText;

    private readonly bool[] landmarkHasTrail = new bool[33] {
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, true,
        true, false, false, false, false, false, false, false, false, false,
        false, true, true
    };
    
    private readonly GameObject[] bodyLandmarks = new GameObject[33];

    private bool hasData = false;
    private Landmarks lastLandmarks = new();

    private float lowestY = 0.0f, ground = 0;

    // Distance between the hips in the landmark data and the hips in the image 
    private float hipDistanceRatio = 1.0f;
    private Vector3 hipPosition = Vector3.zero;


    private void Start()
    {
        // Instantiate 33 spheres for the body landmarks
        for (int i = 0; i < 33; i++)
        {
            bodyLandmarks[i] = Instantiate(bodyLandmarkPrefab, points);
            bodyLandmarks[i].name = "BodyLandmark" + i;

            // Enable trail renderer for some landmarks
            if (landmarkHasTrail[i])
                bodyLandmarks[i].transform.GetChild(0).GetComponent<TrailRenderer>().enabled = true;
            
            // Set the material for the left and right side
            if (PointSide(i) == 1)
                bodyLandmarks[i].transform.GetChild(0).GetComponent<MeshRenderer>().material = rightMaterial;
            else if (PointSide(i) == -1)
                bodyLandmarks[i].transform.GetChild(0).GetComponent<MeshRenderer>().material = leftMaterial;
        }
    }

    private void FixedUpdate()
    {
        string debugString = "";
        debugString += "hip distance ratio: " + Math.Round(hipDistanceRatio, 2) + "\n";

        GetGroundHeight();

        if (hasData)
        {
            MoveBody();

            MoveHip();

            MoveBodyParts();
        }

        debugText.text = debugString;
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
        Gizmos.DrawCube(new Vector3(0, ground, 0), new Vector3(0.5f, 0.02f, 0.5f));

        // Draw the lowest Y position
        float y = hips.position.y + lowestY; 
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(-0.5f, y, 0), new Vector3(0.5f, y, 0));
    }


    private int PointSide(int p)
    {
        return p switch
        {
            0 => 0,
            2 => 1,
            5 => -1,
            _ => p % 2 == 0 ? 1 : -1,
        };
    }

    private void GetGroundHeight()
    {
        // Raycast from the hips to the lowest Y position
        if (Physics.Raycast(hips.position, Vector3.down, out RaycastHit hit))
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
            Vector3 lastPos = bodyLandmarks[i].transform.position;
            Vector3 landmarkPos = new Vector3(
                lastLandmarks.world[i].x,
                lastLandmarks.world[i].y,
                lastLandmarks.world[i].z
            ) * positionScale + hipPosition;

            Vector3 newPos = Vector3.Lerp(lastPos, landmarkPos, lerpSpeed);
            bodyLandmarks[i].transform.localPosition = newPos;

            if (newPos.y < lowestY)
                lowestY = newPos.y;
        }
    }

    private void MoveHip()
    {
        // Get the average x position of the hips
        float landmarkHipX = ((lastLandmarks.image[23].x + lastLandmarks.image[24].x) * 0.5f - 0.5f) * positionScale * hipDistanceRatio;
        // float newHipX = landmarkHipX *  lerpSpeed + hips.position.x * (1 - lerpSpeed);

        // Set the height of the hips to the ground + the lowest Y position
        float newHipY = ground - lowestY;

        hipPosition = new Vector3(landmarkHipX, newHipY, 0);
    }

    private void MoveBodyParts()
    {
        // Move and rotate the hips
        hips.position = hipPosition;

        Vector3 shoulderCenter = (bodyLandmarks[11].transform.position + bodyLandmarks[12].transform.position) * 0.5f;
        Vector3 hipUp = shoulderCenter - hipPosition;
        Vector3 hipRight = bodyLandmarks[24].transform.position - bodyLandmarks[23].transform.position;

        // ah yes, math
        Vector3 forward = Vector3.Cross(hipRight, hipUp); 

        hips.LookAt(hips.position + forward, hipUp);

        // Move and rotate the head 
        head.position = (bodyLandmarks[7].transform.position + bodyLandmarks[8].transform.position) * 0.5f;
        Vector3 headForward = bodyLandmarks[0].transform.position - head.position;
        Vector3 headRight = bodyLandmarks[8].transform.position - bodyLandmarks[7].transform.position;
        Vector3 headUp = Vector3.Cross(headForward, headRight);

        head.LookAt(head.position + headForward, headUp);
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

    public void Calibrate()
    {
        SetHipRatio();
    }

    private void SetHipRatio()
    {
        // Set the hip distance ratio
        float realHipDistance = Math.Abs(lastLandmarks.image[23].x - lastLandmarks.image[24].x);
        float weirdHipDistance = Math.Abs(lastLandmarks.world[23].x - lastLandmarks.world[24].x);
        hipDistanceRatio = weirdHipDistance / realHipDistance;
    }
}
