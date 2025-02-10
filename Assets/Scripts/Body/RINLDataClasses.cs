[System.Serializable]
public struct Landmark
{
    public float x;
    public float y;
    public float z;
}

public struct Landmarks
{
    public Landmark[] world;
    public Landmark[] image;
}

[System.Serializable]
public struct ImageSize
{
    public int width;
    public int height;
}

