using UnityEngine;
using TMPro;

[System.Serializable]
public struct ObjText
{
    public string obj;
    public string text;
}

public class SetText : MonoBehaviour
{
    public void SetObjectText(string jsonData)
    {
        ObjText data = JsonUtility.FromJson<ObjText>(jsonData);
        GameObject obj = SceneScript.Instance.GetObject(data.obj);

        Activity activity = GameController.Instance.GetActivityData();
        string formatedText = data.text
            .Replace("//score", activity.score.ToString())
            .Replace("//activity_points", activity.activity_points.ToString())
            .Replace("//duration", activity.duration.ToString())
            .Replace("//extra_data", activity.extra_data);

        obj.GetComponent<TextMeshProUGUI>().text = formatedText;
    }
}
