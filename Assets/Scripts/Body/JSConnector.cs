using System.Runtime.InteropServices;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;

public class JSConnector : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void SendToReact(string message);

    public RINLBody body;

    public void SetBodyPosition(string landmarkString)
    {
        body.UpdateBodyLandmarks(landmarkString);
    }

    public void SetVideoSize(string sizeString)
    {
        body.SetVideoSize(sizeString);
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
