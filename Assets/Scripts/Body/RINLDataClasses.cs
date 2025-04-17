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
    LeftEyeInner = 1,
    LeftEye = 2,
    LeftEyeOuter = 3,
    RightEyeInner = 4,
    RightEye = 5,
    RightEyeOuter = 6,
    LeftEar = 7,
    RightEar = 8,
    MouthLeft = 9,
    MouthRight = 10,
    LeftShoulder = 11,
    RightShoulder = 12,
    LeftElbow = 13,
    RightElbow = 14,
    LeftWrist = 15,
    RightWrist = 16,
    LeftPinky = 17,
    RightPinky = 18,
    LeftIndex = 19,
    RightIndex = 20,
    LeftThumb = 21,
    RightThumb = 22,
    LeftHip = 23,
    RightHip = 24,
    LeftKnee = 25,
    RightKnee = 26,
    LeftAnkle = 27,
    RightAnkle = 28,
    LeftHeel = 29,
    RightHeel = 30,
    LeftFootIndex = 31,
    RightFootIndex = 32,
}

public static class LandmarkWeights
{
    public static float[] LandmarkPointWeight = new float[Constants.LANDMARKS]
    {
        0.7f, // Nose
        0.0f, // LeftEyeInner
        0.0f, // LeftEye
        0.0f, // LeftEyeOuter
        0.0f, // RightEyeInner
        0.0f, // RightEye
        0.0f, // RightEyeOuter
        0.0f, // LeftEar
        0.0f, // RightEar
        0.0f, // MouthLeft
        0.0f, // MouthRight
        1.5f, // LeftShoulder
        1.5f, // RightShoulder
        0.0f, // LeftElbow
        0.0f, // RightElbow
        0.4f, // LeftWrist
        0.4f, // RightWrist
        0.0f, // LeftPinky
        0.0f, // RightPinky
        0.0f, // LeftIndex
        0.0f, // RightIndex
        0.0f, // LeftThumb
        0.0f, // RightThumb
        2.0f, // LeftHip
        2.0f, // RightHip
        0.0f, // LeftKnee
        0.0f, // RightKnee
        0.0f, // LeftAnkle
        1.0f, // RightAnkle
        1.0f, // LeftHeel
        0.0f, // RightHeel
        0.0f, // LeftFootIndex
        0.0f, // RightFootIndex
    };
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

    public int i;
    private readonly int size;

    public RawLandmarks(int size)
    {
        world = new RawLandmark[size];
        image = new RawLandmark[size];

        this.size = size;
        this.i = 0;
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

    // An array of bools indicating if the point is inside the bounds of the body
    public bool[] inBounds;

    // The height of the ground in world coordinates (notice that in the points the hip position is the origin)
    public float groundHeight;

    // The hip position in world coordinates. This is the origin of the points
    public Vector3 hipPosition;

    // The distance between the lowest feet and the ground (in image coordinates)
    public float displacedAmount;
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
