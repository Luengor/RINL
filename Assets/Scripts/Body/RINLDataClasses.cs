using System;
using UnityEngine;

public static class Constants
{
    public const int LANDMARKS = 33;
}

[System.Serializable]
public enum LandmarkNames
{
    Nose = 0,
    LeftEye = 2,
    RightEye = 5,
    LeftEar = 8,
    RightEar = 7,
    LeftShoulder = 11,
    RightShoulder = 12,
    LeftElbow = 13,
    RightElbow = 14,
    LeftWrist = 15,
    RightWrist = 16,
    LeftHip = 23,
    RightHip = 24,
    LeftKnee = 25,
    RightKnee = 26,
    LeftAnkle = 27,
    RightAnkle = 28
}

[System.Serializable]
public struct Landmark
{
    public float x;
    public float y;
    public float z;

    public readonly Vector2 ToVector2() { return new (x, y); }
    public readonly Vector3 ToVector3() { return new (x, y, z); }

    public readonly float SqrDistance2(Landmark other)
    {
        return Vector2.SqrMagnitude(ToVector2() - other.ToVector2());
    }

    public readonly float SqrDistance3(Landmark other)
    {
        return Vector3.SqrMagnitude(ToVector3() - other.ToVector3());
    }

    public readonly bool InImage()
    {
        return x >= -1 && x <= 1 && y >= 0 && y <= 1; 
    }
}

public struct Landmarks
{
    public Landmark[] world;
    public Landmark[] image;
    
    private int size;

    public Landmarks(int size)
    {
        world = new Landmark[size];
        image = new Landmark[size];

        this.size = size;
    }

    public readonly float SqrDistance2(Landmarks other)
    {
        float diff = 0;
        for (int i = 0; i < size; i++)
            diff += world[i].SqrDistance2(other.world[i]);
        return diff;
    }

    public readonly float SqrDistance3(Landmarks other)
    {
        float diff = 0;
        for (int i = 0; i < size; i++)
            diff += world[i].SqrDistance3(other.world[i]);
        return diff;
    }
}

[System.Serializable]
public struct ImageSize
{
    public int width;
    public int height;
}

[System.Serializable]
public struct Activity
{
    public DateTime date;
    public string minigame;
    public int duration;
    public int activity_points;
    public string extra_data;
}
