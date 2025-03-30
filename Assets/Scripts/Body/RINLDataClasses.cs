using System;
using UnityEngine;

public static class Constants
{
    public const int LANDMARKS = 33;
}

[Serializable]
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


// The Landmarks as received from the JS side

[Serializable]
public struct RawLandmark
{
    public float x;
    public float y;
    public float z;

    public readonly Vector2 ToVector2() { return new(x, y); }
    public readonly Vector3 ToVector3() { return new(x, y, z); }

    public readonly float SqrDistance2(RawLandmark other)
    {
        return Vector2.SqrMagnitude(ToVector2() - other.ToVector2());
    }

    public readonly float SqrDistance3(RawLandmark other)
    {
        return Vector3.SqrMagnitude(ToVector3() - other.ToVector3());
    }

    public readonly bool InImage()
    {
        return x >= -1 && x <= 1 && y >= 0 && y <= 1;
    }
}

public struct RawLandmarks
{
    public RawLandmark[] world;
    public RawLandmark[] image;

    private readonly int size;

    public RawLandmarks(int size)
    {
        world = new RawLandmark[size];
        image = new RawLandmark[size];

        this.size = size;
    }

    public readonly float SqrDistance2(RawLandmarks other)
    {
        float diff = 0;
        for (int i = 0; i < size; i++)
            diff += world[i].SqrDistance2(other.world[i]);
        return diff;
    }

    public readonly float SqrDistance3(RawLandmarks other)
    {
        float diff = 0;
        for (int i = 0; i < size; i++)
            diff += world[i].SqrDistance3(other.world[i]);
        return diff;
    }
}

// The Landmarks as used by the body

public struct Landmarks
{
    // The points in world coordinates WITH THE HIP POSITION BEING THE ORIGIN
    public Vector3[] points;

    // The height of the ground in world coordinates (notice that in the points the hip position is the origin)
    public float groundHeight;

    // The hip position in world coordinates. This is the origin of the points
    public Vector3 hipPosition;
}

[Serializable]
public struct ImageSize
{
    public int width;
    public int height;
}

[Serializable]
public struct Activity
{
    public DateTime date;
    public string minigame;
    public int duration;
    public int score;
    public int activity_points;
    public string extra_data;
}

[Serializable]
public struct Shape
{
    public float weight;
    public float height;
    public float sex_math;
}

[Serializable]
public struct User
{
    public string fullname;
}
