using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
struct Landmark
{
    public float x;
    public float y;
    public float z;
    public float v;
}

struct Landmarks
{
    public Landmark[] landmarks;
    public Landmark hipRight;
    public Landmark hipLeft;
}


public class RINLBody : MonoBehaviour
{
    [Header("Transforms")]
    public Transform hips;

    [Header("Settings")]
    public float positionScale = 1.0f;
    [Range(0.0f, 1.0f)]
    public float lerpSpeed = 0.8f;
    public float groundOffset = 0.1f;

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


    private void Start()
    {
        // Instantiate 33 spheres for the body landmarks
        for (int i = 0; i < 33; i++)
        {
            bodyLandmarks[i] = Instantiate(bodyLandmarkPrefab, hips);
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

        GetGroundHeight();
        debugString += "ground: " + ground + "\n";

        if (hasData)
        {
            MoveBody();

            MoveHip();

            debugString += "hip right: " + new Vector3(lastLandmarks.hipRight.x, lastLandmarks.hipRight.y, lastLandmarks.hipRight.z).ToString() + "\n";
            debugString += "hip left: " + new Vector3(lastLandmarks.hipLeft.x, lastLandmarks.hipLeft.y, lastLandmarks.hipLeft.z).ToString() + "\n";
        }

        debugText.text = debugString;
    }

    private void OnDrawGizmos()
    {
        // Draw the hips
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(hips.position, 0.1f);

        // Draw the ground
        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector3(0, ground, 0), new Vector3(0.5f, 0.02f, 0.5f));

        // Draw the lowest Y position
        float y = hips.position.y + lowestY; 
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(-0.5f, y, 0), new Vector3(0.5f, y, 0));

        // Draw a point on the locations near the lowest Y position
        if (!Application.isPlaying)
            return;

        for (int i = 0; i < bodyLandmarks.Length; i++)
        {
            Vector3 pos = bodyLandmarks[i].transform.position;
            if (pos.y < y + groundOffset && pos.y > y - groundOffset)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(pos, 0.30f);
            }
        }

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
                lastLandmarks.landmarks[i].x,
                lastLandmarks.landmarks[i].y,
                lastLandmarks.landmarks[i].z
            ) * positionScale;

            // TODO: Take visibility into account
            Vector3 newPos = Vector3.Lerp(lastPos, landmarkPos, lerpSpeed);
            bodyLandmarks[i].transform.localPosition = newPos;

            if (newPos.y < lowestY)
                lowestY = newPos.y;
        }
    }

    private void MoveHip()
    {
        // Get the average x position of the hips
        float landmarkHipX = ((lastLandmarks.hipRight.x + lastLandmarks.hipLeft.x) * 0.5f - 0.5f) * positionScale;
        // float newHipX = landmarkHipX *  lerpSpeed + hips.position.x * (1 - lerpSpeed);

        // Set the height of the hips to the ground + the lowest Y position
        float newHipY = ground - lowestY;

        hips.position = new Vector3(landmarkHipX, newHipY, 0); 
    }

    public void SetBodyPosition(string landmarkString)
    {
        hasData = true;
        lastLandmarks = JsonUtility.FromJson<Landmarks>(landmarkString);

        // Invert X, Y and Z
        for (int i = 0; i < lastLandmarks.landmarks.Length; i++)
        {
            lastLandmarks.landmarks[i].x *= -1;
            lastLandmarks.landmarks[i].y *= -1;
            lastLandmarks.landmarks[i].z *= -1;
        }

        // For the hips, X and Y are in the range 0-1 and Z is the same as on the other landmarks
        lastLandmarks.hipRight.x = 1 - lastLandmarks.hipRight.x; 
        lastLandmarks.hipRight.y = 1 - lastLandmarks.hipRight.y;
        lastLandmarks.hipRight.z *= -1; 

        lastLandmarks.hipLeft.x = 1 - lastLandmarks.hipLeft.x;
        lastLandmarks.hipLeft.y = 1 - lastLandmarks.hipLeft.y;
        lastLandmarks.hipLeft.z *= -1;
    }
}
