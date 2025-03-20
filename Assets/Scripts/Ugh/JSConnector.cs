using System.Runtime.InteropServices;
using UnityEngine;

public class JSConnector : MonoBehaviour
{
    public Landmarks LatestLandmarks { get; private set; }

    [DllImport("__Internal")]
    private static extern void SendToReact(string message);
    private ImageSize imageSize = new() { width = 640, height = 480 };

    public void SetBodyPosition(string landmarkString)
    {
        LatestLandmarks = JsonUtility.FromJson<Landmarks>(landmarkString);

        // Convert the landmarks
        for (int i = 0; i < LatestLandmarks.world.Length; i++)
        {
            // // Flip the 3D landmarks
            // LatestLandmarks.world[i].x *= flipX ? -1 : 1;    Flipping is not done here 
            LatestLandmarks.world[i].y *= -1;
            LatestLandmarks.world[i].z *= -1;

            // Calculate aspect ratio
            float aspect = (float)imageSize.width / imageSize.height;

            // Flip and change the range of the image landmarks
            LatestLandmarks.image[i].x = LatestLandmarks.image[i].x * aspect * 2 - aspect;

            /*
            if (flipX)
                LatestLandmarks.image[i].x *= -1;
            */

            LatestLandmarks.image[i].y = 1 - LatestLandmarks.image[i].y;
        }
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
