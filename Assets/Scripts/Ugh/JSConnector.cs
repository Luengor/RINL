using System.Runtime.InteropServices;
using UnityEngine;

public class JSConnector : MonoBehaviour
{
    public Landmarks Landmarks { get; private set; }
    public RawLandmarks RawLandmarks { get; private set; } = new();
    public Shape CurrentShape { get; private set; } = new () { height = 1.63f, weight = 50 };

    [DllImport("__Internal")]
    private static extern void SendToReact(string message);
    private ImageSize imageSize = new() { width = 640, height = 480 };

    public void SetBodyPosition(string landmarkString)
    {
        RawLandmarks = JsonUtility.FromJson<RawLandmarks>(landmarkString);

        // Convert the landmarks
        for (int i = 0; i < RawLandmarks.world.Length; i++)
        {
            // // Flip the 3D landmarks
            // LatestLandmarks.world[i].x *= flipX ? -1 : 1;    Flipping is not done here 
            RawLandmarks.world[i].y *= -1;
            RawLandmarks.world[i].z *= -1;

            // Calculate aspect ratio
            float aspect = (float)imageSize.width / imageSize.height;

            // Flip and change the range of the image landmarks
            RawLandmarks.image[i].x = RawLandmarks.image[i].x * aspect * 2 - aspect;

            /*
            if (flipX)
                LatestLandmarks.image[i].x *= -1;
            */

            RawLandmarks.image[i].y = 1 - RawLandmarks.image[i].y;
        }

        // Transform the landmarks
        Landmarks = GameController.CalibrationData.TransformLandmarks(RawLandmarks);
    }

    public void SetVideoSize(string sizeString)
    {
        imageSize = JsonUtility.FromJson<ImageSize>(sizeString);
    }

    public void CreateActivity(string minigame, int duration, int activity_points, string extra_data)
    {
        Activity activity = new()
        {
            date = System.DateTime.Now,
            minigame = minigame,
            duration = duration,
            activity_points = activity_points,
            extra_data = extra_data
        };

        string json = JsonUtility.ToJson(activity);
        SendToReact(json);
    }
}
