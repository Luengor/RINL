using System.Runtime.InteropServices;
using UnityEngine;

public class JSConnector : MonoBehaviour
{
    public RawLandmarks LatestLandmarks {
        get { return latestLandmarks; }
        private set
        {
            latestLandmarks = value;
        }
    } 
    public Shape UserShape { get; private set; } = new Shape { weight = 50f, height = 165f, sex_math = 0.1f };
    public User CurrentUser { get; private set; }

    [DllImport("__Internal")]
    private static extern void SendToReact(string type, string payload);
    [DllImport("__Internal")]
    private static extern void SendAck(string functionName);
    private ImageSize imageSize = new() { width = 640, height = 480 };
    private RawLandmarks latestLandmarks = new(Constants.LANDMARKS); 

    public void SetBodyPosition(string landmarkString)
    {
        int new_i = LatestLandmarks.i + 1;
        LatestLandmarks = JsonUtility.FromJson<RawLandmarks>(landmarkString);
        latestLandmarks.i = new_i;

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
        UserShape = JsonUtility.FromJson<Shape>(shapeString);
        SendAck("SetCurrentShape");
    }

    public void SetCurrentUser(string userString)
    {
        CurrentUser = JsonUtility.FromJson<User>(userString);
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
