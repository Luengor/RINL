using System.Collections.Generic;
using UnityEngine;

public class RINLBody : MonoBehaviour
{
    [Header("Transforms")]
    public Transform points;

    [Header("Body parts")]
    public Transform bodyParent;
    public GameObject bodySegmentPrefab, bodyJointPrefab;
    public BodyPart[] bodyParts;
    private List<GameObject[]> bodyPartObjects;

    [Header("Point transformation settings")]
    [Tooltip("Flip the x-axis of the points")]
    public bool flipX = false;
    [Tooltip("Scale the points")]
    public float pointScale = 1f;
    [Tooltip("Use a fixed position for the hip. If false, the hip position is calculated from the image landmarks")]
    public bool fixedPosition = false;
    [Tooltip("Set the hip position to the ground height. Ignored if fixedPosition is true")]
    public bool useGroundHeight = true;

    [Header("Other settings")]
    [Tooltip("The speed of the lerp between the points")]
    public float lerpSpeed = 15f;
    
    public Landmarks Landmakrs
    {
        get
        {
            return lastLandmarks;
        }
    }

    /// Private
    private readonly Transform[] bodyLandmarks = new Transform[Constants.LANDMARKS];

    private CalibrationData calibration = new();

    private Landmarks lastLandmarks = new();

    // The position of the hip calculated from the image landmarks
    private Vector3 worldHipPosition = new();

    private void Start()
    {
        // Get the calibration data from the game controller
        if (GameController.CalibrationData != null)
            calibration = GameController.CalibrationData;

        // Get the body landmarks from the points object 
        for (int i = 0; i < Constants.LANDMARKS; i++)
            bodyLandmarks[i] = points.GetChild(i);

        // Create the body parts
        bodyPartObjects = new List<GameObject[]>();
        foreach (BodyPart part in bodyParts)
        {
            GameObject[] segmentObjects = new GameObject[part.segments.Length * 2];

            // Instantiate the segments and joints
            for (int i = 0; i < part.segments.Length; i++)
            {
                segmentObjects[i] = Instantiate(bodySegmentPrefab, bodyParent);
                segmentObjects[i].name = part.name + " Segment " + i;
                segmentObjects[i].transform.localScale = new Vector3(part.segments[i].width, 1, part.segments[i].width);

                segmentObjects[i + part.segments.Length] = Instantiate(bodyJointPrefab, bodyParent);
                segmentObjects[i + part.segments.Length].name = part.name + " Joint " + i;
                segmentObjects[i + part.segments.Length].transform.localScale = Vector3.one * part.segments[i].jointSize;
            }

            // If end object is set, set it as a child of the last joint 
            if (!string.IsNullOrEmpty(part.moveEndObject))
            {
                Transform endObject = transform.Find(part.moveEndObject);
                if (endObject != null)
                {
                    endObject.SetParent(segmentObjects[part.segments.Length * 2 - 1].transform);
                    endObject.localPosition = Vector3.zero;
                } else {
                    Debug.LogError("End object not found: " + part.moveEndObject);
                }
            }

            bodyPartObjects.Add(segmentObjects);
        }
        
    }

    private void FixedUpdate()
    {
        // Get the landmarks from the JS connector and convert them
        lastLandmarks = GameController.Instance.JsConnector.LatestLandmarks;

        if (lastLandmarks.image == null)
            return;

        // Calculate the hip position from the image landmarks
        CalulateHipPosition();

        // Move all body points using the landmarks and the hip position
        MoveBody();

        // Move the body parts
        MoveBodyParts();
    }

    private void CalulateHipPosition()
    {
        if (fixedPosition)
        {
            worldHipPosition = Vector3.zero;
            return;
        }

        Vector2 leftHip = new (
            lastLandmarks.image[23].x,
            lastLandmarks.image[23].y
        );
        Vector2 rightHip = new (
            lastLandmarks.image[24].x,
            lastLandmarks.image[24].y
        );

        Vector2 imageHipPosition = (leftHip + rightHip) / 2;

        if (useGroundHeight)
            imageHipPosition.y -= calibration.imageGroundHeight;

        worldHipPosition = new (
            imageHipPosition.x * calibration.worldImageRatio.x,
            imageHipPosition.y * calibration.worldImageRatio.y,
            0
        );
    }

    private void MoveBody()
    {
        for (int i = 0; i < Constants.LANDMARKS; i++)
        {
            Vector3 newPos = GetLandmarkPosition(i);
            bodyLandmarks[i].localPosition = newPos;
        }
    }

    private Vector3 GetLandmarkPosition(int index)
    {
        Vector3 lastPos = bodyLandmarks[index].localPosition;
        Vector3 newWorldPos = new Vector3(
            lastLandmarks.world[index].x,
            lastLandmarks.world[index].y,
            lastLandmarks.world[index].z
        );
        Vector3 newPos = (newWorldPos + worldHipPosition) * pointScale;

        return Vector3.Lerp(lastPos, newPos, Time.deltaTime * lerpSpeed);
    }

    private void MoveBodyParts()
    {
        // Body parts
        for (int i = 0; i < bodyParts.Length; i++)
        {
            BodyPart part = bodyParts[i];
            GameObject[] objects = bodyPartObjects[i];

            for (int j = 0; j < part.segments.Length; j++)
            {
                Transform segment = objects[j].transform;

                // Calculate the real start and end positions
                Vector3 start = bodyLandmarks[(int)part.segments[j].start].localPosition;
                Vector3 end = bodyLandmarks[(int)part.segments[j].end].localPosition;
                float distance = Vector3.Distance(start, end);
                Vector3 header = (end - start) / distance;

                // Set the end joint position
                Transform endJoint = objects[j + part.segments.Length].transform;
                endJoint.localPosition = end;

                // Offset by the end joint
                distance -= part.segments[j].jointSize / 2 + part.jointGap;

                // Offset by the previous joint
                if (j != 0)
                {
                    Vector3 offset = header * (part.segments[j - 1].jointSize / 2 + part.jointGap);
                    start += offset;
                    distance -= part.segments[j - 1].jointSize / 2 + part.jointGap;
                }

                // Set the segment position and scale
                segment.localScale = new Vector3(
                    part.segments[j].width,
                    distance / 2,
                    part.segments[j].width
                );

                segment.up = header;
                segment.localPosition = start;
            }
        }
    }

    public void UpdateCalibration(CalibrationData data)
    {
        calibration = data;
    }

    public void ResetActivityPoints()
    {
        // TODO
    }

    public int GetActivityPoints()
    {
        // TODO
        return 100;
    }
}
