using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RINLBody : MonoBehaviour
{
    [Header("Transforms")]
    public Transform points;

    [Header("Alert things")]
    public GameObject alertPanel;
    public TextMeshProUGUI alertText;

    [Header("Body parts")]
    public Transform bodyParent;
    public GameObject bodySegmentPrefab, bodyJointPrefab;
    public BodyPart[] bodyParts;
    private List<GameObject[]> bodyPartObjects;

    [Header("Hands")]
    public Transform leftHand;
    public Transform rightHand;
    private GameObject leftHandRenderer, rightHandRenderer;
    public float handScale = 1f;
    public bool showHands = true;

    [Header("Bounds and displacement")]
    [Tooltip("The amount of displacement needed to consider the body displaced")]
    public float displacementThreshold = 0.1f;
    [Tooltip("The time needed to consider the body displaced")]
    public float displacementTime = 3f;
    private float displacementTimer = 0f;

    [Header("Point transformation settings")]
    [Tooltip("Flip the x-axis of the points")]
    public bool flipX = false;
    [Tooltip("Scale the points")]
    public float pointScale = 1f;
    [Tooltip("Use a fixed position for the hip. If false, the hip position is calculated from the image landmarks")]
    public bool fixedPosition = false;
    [Tooltip("Use ground height for the vertical position. If false, the hip will move only in the X axis")]
    public bool useGroundHeight = true;
    [Tooltip("Should the body be in the center of the bounds or in the center of the camera?")]
    public bool centerWithBounds = true;

    [Header("Activity points settings")]
    [Tooltip("If the speed difference is greater than this value, the points will be calculated")]
    public float minAccForPoints = 0.1f;
    [Tooltip("The amount of points per unit of acceleration")]
    public float accToPoints = 100f;

    [Header("Other settings")]
    [Tooltip("Smooth time for the point movement")]
    public float pointSmoothTime = 0.1f;
    [Tooltip("Max speed for the point movement (scaled by pointScale)")]
    public float pointMaxSpeed = 10f;
    public TMPro.TextMeshProUGUI debugText;

    /// Private
    private readonly Transform[] bodyLandmarks = new Transform[Constants.LANDMARKS];
    private readonly Vector3[] bodyLandmarkSpeeds = new Vector3[Constants.LANDMARKS];

    private Landmarks landmarks = new();

    private float activityPoints = 0;
    private int last_ladmarks_i = -1;


    private void OnDrawGizmos()
    {
        if (landmarks.points == null)
            return;

        Gizmos.color = Color.red;
        Bounds bounds = GetBounds();
        Gizmos.DrawWireCube(bounds.center, bounds.size); 
    }

    private void Start()
    {
        // Get the hand renderers
        leftHandRenderer = leftHand.GetChild(0).gameObject;
        rightHandRenderer = rightHand.GetChild(0).gameObject;

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
                }
                else
                {
                    Debug.LogError("End object not found: " + part.moveEndObject);
                }
            }

            bodyPartObjects.Add(segmentObjects);
        }

    }

    private void Update()
    {
        // Get the landmarks from the JS connector and convert them
        RawLandmarks rawLandmarks = GameController.Instance.JsConnector.LatestLandmarks;

        if (rawLandmarks.image == null)
            return;

        if (rawLandmarks.i > last_ladmarks_i)
        {
            landmarks = GameController.CalibrationData.TransformLandmarks(rawLandmarks);
            last_ladmarks_i = rawLandmarks.i;
        }

        // If we are displaced, check if we are ok to resume
        if (displacementTimer > displacementTime && landmarks.displacedAmount < displacementThreshold)
        {
            displacementTimer = 0;
            GameController.Instance.Resume();
            alertPanel.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (landmarks.points == null)
            return;

        // Move all body points using the landmarks and the hip position
        MoveBody();

        // Check if the body is displaced
        UpdateDisplacement();

        // Move the body parts
        MoveBodyParts();

        // Move the hands
        MoveHands();
    }

    private void MoveBody()
    {
        for (int i = 0; i < Constants.LANDMARKS; i++)
        {
            Vector3 newPos = GetLandmarkPosition(i);
            bodyLandmarks[i].localPosition = newPos;
        }

        // Debug
        debugText.text = "Left hip z: " + GameController.Instance.JsConnector.LatestLandmarks.image[(int)LandmarkNames.LeftHip].z + "\n" +
                         "Image ground height: " + GameController.CalibrationData.imageGroundHeight + "\n" +
                         "World ground height: " + landmarks.groundHeight + "\n" +
                         "Displaced amount: " + landmarks.displacedAmount+ "\n"
                         ;
    }

    private Vector3 GetLandmarkPosition(int index)
    {
        // Get the last position and the new landmark position 
        Vector3 lastPos = bodyLandmarks[index].localPosition;
        Vector3 newWorldPos = landmarks.points[index];

        // If the body should be centered with the bounds, add the bounds center to the new position
        if (centerWithBounds)
        {
            Vector3 boundsCenter = GameController.CalibrationData.bounds.center;
            newWorldPos -= Vector3.right * boundsCenter.x; 
        }

        // If the body is not fixed, add the hip position to the new position 
        if (!fixedPosition)
        {
            // If the ground height is used, add the hips y position and subtract the ground height
            if (useGroundHeight)
                // newWorldPos.y += landmarks.hipPosition.y;
                newWorldPos.y += landmarks.hipPosition.y - landmarks.groundHeight;

            // Either way, add the hips x position
            newWorldPos.x += landmarks.hipPosition.x;
        }

        // Flip the x-axis if needed
        if (flipX) newWorldPos.x *= -1;

        // Calculate the new position
        Vector3 newPos = newWorldPos * pointScale;

        // Save the previous speed
        float lastSpeed = bodyLandmarkSpeeds[index].magnitude / pointScale;

        // Calculate the new position 
        Vector3 pos = Vector3.SmoothDamp(lastPos, newPos, ref bodyLandmarkSpeeds[index], pointSmoothTime, pointMaxSpeed * pointScale, Time.fixedDeltaTime);

        // Get the new speed
        float newSpeed = bodyLandmarkSpeeds[index].magnitude / pointScale;

        // Calculate the speed difference for the activity points
        float acc = Math.Abs(newSpeed - lastSpeed) / Time.fixedDeltaTime;
        if (acc > minAccForPoints)
        {
            // Calculate the activity points based on the speed difference
            activityPoints += acc * accToPoints * LandmarkWeights.LandmarkPointWeight[index];
        }

        // Return the new position
        return pos;
    }

    private void UpdateDisplacement()
    {
        // Check if the body is displaced
        if (landmarks.displacedAmount > displacementThreshold && GameController.CalibrationData.calibrated)
            displacementTimer += Time.fixedDeltaTime;
        else
            displacementTimer = 0;

        if (displacementTimer > displacementTime)
        {
            alertPanel.SetActive(true);
            alertText.text = "No te muevas perro";
            GameController.Instance.Pause();
            Debug.Log("Body is displaced");
        }
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

                // Up vector must be set with world coordinates because there is no local up
                segment.up = bodyLandmarks[(int)part.segments[j].end].position - bodyLandmarks[(int)part.segments[j].start].position;
                segment.localPosition = start;
            }
        }
    }

    private void MoveHands()
    {
        // Move
        leftHand.localPosition =
            (bodyLandmarks[(int)LandmarkNames.LeftWrist].localPosition + bodyLandmarks[(int)LandmarkNames.LeftIndex].localPosition) * .5f;
        
        rightHand.localPosition =
            (bodyLandmarks[(int)LandmarkNames.RightWrist].localPosition + bodyLandmarks[(int)LandmarkNames.RightIndex].localPosition) * .5f;
        
        // Scale
        if (!showHands)
        {
            if (leftHandRenderer.activeSelf)
            {
                leftHandRenderer.SetActive(false);
                rightHandRenderer.SetActive(false);
            }
        }
        else
        {
            if (!leftHandRenderer.activeSelf)
            {
                leftHandRenderer.SetActive(true);
                rightHandRenderer.SetActive(true);
            }

            leftHandRenderer.transform.localScale = handScale * pointScale * Vector3.one;
            rightHandRenderer.transform.localScale = handScale * pointScale * Vector3.one;
        }
    }

    public Vector3 GetLandmarkWorldPosition(int index)
    {
        return bodyLandmarks[index].position;
    }

    public Bounds GetBounds()
    {
        Bounds bounds = GameController.CalibrationData.bounds;

        // If the body is centered with the bounds, set the bounds center to the transform position
        if (centerWithBounds)
        {
            bounds.center = new(
                transform.position.x,
                bounds.center.y * pointScale + transform.position.y,
                transform.position.z
            );
        }
        else
        {
            // If the body is not centered with the bounds, also use the bounds x position
            bounds.center = new(
                bounds.center.x * pointScale + transform.position.x,
                bounds.center.y * pointScale + transform.position.y,
                transform.position.z
            );
        }
        
        bounds.size *= pointScale;

        return bounds;
    }

    public void ResetActivityPoints()
    {
        activityPoints = 0f;
    }

    public int GetActivityPoints()
    {
        return Mathf.FloorToInt(activityPoints);
    }
}
