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

    [Header("Hands")]
    public Transform leftHand;
    public Transform rightHand;
    private GameObject leftHandRenderer, rightHandRenderer;
    public float handScale = 1f;
    public bool showHands = true;

    [Header("Point transformation settings")]
    [Tooltip("Flip the x-axis of the points")]
    public bool flipX = false;
    [Tooltip("Scale the points")]
    public float pointScale = 1f;
    [Tooltip("Use a fixed position for the hip. If false, the hip position is calculated from the image landmarks")]
    public bool fixedPosition = false;
    [Tooltip("Should the body be in the center of the bounds or in the center of the camera?")]
    public bool centerWithBounds = true;

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


    private void OnDrawGizmos()
    {
        if (landmarks.points == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GameController.CalibrationData.bounds.center * pointScale + transform.position, GameController.CalibrationData.bounds.size * pointScale);
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

    private void FixedUpdate()
    {
        // Get the landmarks from the JS connector and convert them
        RawLandmarks rawLandmarks = GameController.Instance.JsConnector.LatestLandmarks;

        if (rawLandmarks.image == null)
            return;

        landmarks = GameController.CalibrationData.TransformLandmarks(rawLandmarks);

        // Move all body points using the landmarks and the hip position
        MoveBody();

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
        float shoulderTilt = bodyLandmarks[(int)LandmarkNames.RightShoulder].localPosition.y - bodyLandmarks[(int)LandmarkNames.LeftShoulder].localPosition.y;
        float hipTilt = bodyLandmarks[(int)LandmarkNames.RightHip].localPosition.y - bodyLandmarks[(int)LandmarkNames.LeftHip].localPosition.y;

        debugText.text = "Left shoulder: " + bodyLandmarks[(int)LandmarkNames.LeftShoulder].localPosition + "\n" +
                         "Right shoulder: " + bodyLandmarks[(int)LandmarkNames.RightShoulder].localPosition + "\n" +
                         "Left hip: " + bodyLandmarks[(int)LandmarkNames.LeftHip].localPosition + "\n" +
                         "Right hip: " + bodyLandmarks[(int)LandmarkNames.RightHip].localPosition + "\n\n" +
                         "Shoulder tilt: " + shoulderTilt + "\n" +
                         "Hip tilt: " + hipTilt + "\n";
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
        // (this allows jumping and moving around)
        if (!fixedPosition)
            newWorldPos += landmarks.hipPosition;

        // Flip the x-axis if needed
        if (flipX) newWorldPos.x *= -1;

        // Calculate the new position
        Vector3 newPos = newWorldPos * pointScale;
        return Vector3.SmoothDamp(lastPos, newPos, ref bodyLandmarkSpeeds[index], pointSmoothTime, pointMaxSpeed * pointScale, Time.fixedDeltaTime);
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

    public void MoveHands()
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

    public Bounds GetBounds()
    {
        Bounds bounds = GameController.CalibrationData.bounds;

        bounds.center = bounds.center * pointScale + transform.position;
        bounds.size *= pointScale;

        return bounds;
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
