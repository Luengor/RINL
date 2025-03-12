using UnityEngine;

public class RINLBody : MonoBehaviour
{
    [Header("Transforms")]
    public Transform head;
    public Transform rightHand, leftHand;
    public Transform hips;
    public Transform points;

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

    private bool hasData = false;
    private Landmarks lastLandmarks = new();
    private ImageSize imageSize = new() { width = 640, height = 480 };

    // The position of the hip calculated from the image landmarks
    private Vector3 worldHipPosition = new();

    private void Start()
    {
        // Get the body landmarks from the points object 
        for (int i = 0; i < Constants.LANDMARKS; i++)
            bodyLandmarks[i] = points.GetChild(i);
    }

    private void FixedUpdate()
    {
        if (hasData)
        {
            // Calculate the hip position from the image landmarks
            CalulateHipPosition();

            // Move all body points using the landmarks and the hip position
            MoveBody();

            // Move the body parts
            MoveBodyParts();
        }
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
        // Head
        head.localPosition = (bodyLandmarks[7].localPosition + bodyLandmarks[8].localPosition) / 2; 
        Vector3 headUp = Vector3.Cross(bodyLandmarks[8].localPosition - bodyLandmarks[7].localPosition, bodyLandmarks[0].localPosition - head.localPosition);
        head.LookAt(bodyLandmarks[0], headUp * -1);

        // Hips
        hips.localPosition = (bodyLandmarks[23].localPosition + bodyLandmarks[24].localPosition) / 2;
        Vector3 shoulderAvg = (bodyLandmarks[11].localPosition + bodyLandmarks[12].localPosition) / 2;
        Vector3 hipUp = shoulderAvg - hips.localPosition;
        Vector3 hipForward = Vector3.Cross(bodyLandmarks[24].localPosition - bodyLandmarks[23].localPosition, shoulderAvg - hips.localPosition);
        hips.LookAt(hipForward + hips.localPosition, hipUp);

        // Hands
        leftHand.localPosition = (bodyLandmarks[15].localPosition + bodyLandmarks[17].localPosition + bodyLandmarks[19].localPosition) / 3;
        rightHand.localPosition = (bodyLandmarks[16].localPosition + bodyLandmarks[18].localPosition + bodyLandmarks[20].localPosition) / 3;

        leftHand.LookAt((bodyLandmarks[17].localPosition + bodyLandmarks[19].localPosition) / 2);
        rightHand.LookAt((bodyLandmarks[18].localPosition + bodyLandmarks[20].localPosition) / 2);
    }

    public void UpdateCalibration(CalibrationData data)
    {
        calibration = data;
    }

    public void UpdateBodyLandmarks(string landmarkString)
    {
        hasData = true;
        lastLandmarks = JsonUtility.FromJson<Landmarks>(landmarkString);

        // Convert the landmarks
        for (int i = 0; i < lastLandmarks.world.Length; i++)
        {
            // Flip the 3D landmarks
            lastLandmarks.world[i].x *= flipX ? -1 : 1;
            lastLandmarks.world[i].y *= -1;
            lastLandmarks.world[i].z *= -1;

            // Calculate aspect ratio
            float aspect = (float)imageSize.width / imageSize.height;

            // Flip and change the range of the image landmarks
            lastLandmarks.image[i].x = lastLandmarks.image[i].x * aspect * 2 - aspect;
            if (flipX)
                lastLandmarks.image[i].x *= -1;
            lastLandmarks.image[i].y = 1 - lastLandmarks.image[i].y;

            // lastLandmarks.image[i].z *= -aspect;
        }
    }

    public void SetVideoSize(string sizeString)
    {
        imageSize = JsonUtility.FromJson<ImageSize>(sizeString);
    }
}
