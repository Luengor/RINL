using UnityEngine;

[System.Serializable]
public struct Segment
{
    public LandmarkNames start;
    public LandmarkNames end;

    public float width;
    public float jointSize;
}

[CreateAssetMenu(fileName = "BodyPart", menuName = "Scriptable Objects/BodyPart")]
public class BodyPart : ScriptableObject
{
    public string partName;
    public float jointGap = 0.05f;
    public Segment[] segments;
    public string moveEndObject;
}
