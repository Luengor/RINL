using System.Runtime.InteropServices;
using UnityEngine;

public class JSConnector : MonoBehaviour
{
    public RawLandmarks LatestLandmarks { get; private set; }

    [DllImport("__Internal")]
    private static extern void SendToReact(string type, string payload);
    [DllImport("__Internal")]
    private static extern void SendAck(string functionName);
    private ImageSize imageSize = new() { width = 640, height = 480 };

    public void SetBodyPosition(string landmarkString)
    {
        LatestLandmarks = JsonUtility.FromJson<RawLandmarks>(landmarkString);

        // Convert the landmarks
        for (int i = 0; i < LatestLandmarks.world.Length; i++)
        {
            // Flip the 3D landmarks
            LatestLandmarks.world[i].y *= -1;
            LatestLandmarks.world[i].z *= -1;

            // Calculate aspect ratio
            float aspect = (float)imageSize.width / imageSize.height;

            // Flip and change the range of the image landmarks
            LatestLandmarks.image[i].x = LatestLandmarks.image[i].x * aspect * 2 - aspect;
            LatestLandmarks.image[i].y = 1 - LatestLandmarks.image[i].y;
        }
    }

    public void SetVideoSize(string sizeString)
    {
        imageSize = JsonUtility.FromJson<ImageSize>(sizeString);
        SendAck("SetVideoSize");
    }

    public void SetCurrentShape(string shapeString)
    {
        Shape shape = JsonUtility.FromJson<Shape>(shapeString);
        SendAck("SetCurrentShape");
    }

    public void SetCurrentUser(string userString)
    {
        User user = JsonUtility.FromJson<User>(userString);
        SendAck("SetCurrentUser");
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
        Debug.Log("activity: " + json);
        SendToReact("activity", json);
    }
}
