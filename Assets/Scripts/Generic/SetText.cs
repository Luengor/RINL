using Unity.VisualScripting;
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
        GameObject obj = GameObject.Find(data.obj);
        obj.GetComponent<TextMeshProUGUI>().text = data.text;
    }
}
