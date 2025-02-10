using UnityEngine;

public class RINLBody : MonoBehaviour
{
    [Header("Transforms")]
    public Transform head;
    public Transform rightHand, leftHand;
    public Transform hips;
    public Transform points;

    [Header("Point transformation settings")]
    public bool flipX = false;
    public float pointScale = 1f;
    public bool fixedPosition = false;

    [Header("Other settings")]
    public LayerMask groundLayer;
    public float lerpSpeed = 15f;
    
    /// Private
    private readonly Transform[] bodyLandmarks = new Transform[33];

    private readonly BodyCalibration calibration = new();

    private bool hasData = false;
    private Landmarks lastLandmarks = new();
    private ImageSize imageSize = new() { width = 640, height = 480 };

    // The position of the hip calculated from the image landmarks
    private Vector3 worldHipPosition = new();

    private void Start()
    {
        // Get the 33 body landmarks from the points object 
        for (int i = 0; i < 33; i++)
            bodyLandmarks[i] = points.GetChild(i);
    }

    private void FixedUpdate()
    {
        if (Input.GetMouseButtonDown(0))
            calibration.InitialT(lastLandmarks);

        if (hasData)
        {
            // Calculate the hip position from the image landmarks
            CalulateHipPosition();

            // Move all body points using the landmarks and the hip position
            MoveBody();
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
        imageHipPosition.y -= calibration.data.imageGroundHeight;

        worldHipPosition = new (
            imageHipPosition.x * calibration.data.worldImageRatio.x,
            imageHipPosition.y * calibration.data.worldImageRatio.y,
            0
        );
    }

    private void MoveBody()
    {
        for (int i = 0; i < 33; i++)
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

    private Bounds GetWorldBounds()
    {
        Bounds bounds = new();
        for (int i = 0; i < 33; i++)
            bounds.Encapsulate(new Vector3(lastLandmarks.world[i].x, lastLandmarks.world[i].y, lastLandmarks.world[i].z));
        return bounds;
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
                lastLandmarks.image[i].x = 1 - lastLandmarks.image[i].x;
            lastLandmarks.image[i].y = 1 - lastLandmarks.image[i].y;

            // lastLandmarks.image[i].z *= -aspect;
        }
    }

    public void SetImageSize(string sizeString)
    {
        imageSize = JsonUtility.FromJson<ImageSize>(sizeString);
    }
}
