using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[System.Serializable]
struct Landmark
{
    public float x;
    public float y;
    public float z;
    public float visibility;
}

struct Landmarks
{
    public Landmark[] landmarks;
}


public class RINLBody : MonoBehaviour
{
    public GameObject bodyLandmarkPrefab;
    public GameObject edgePrefab;
    public float positionScale = 1.0f;

    private readonly bool[] landmarkHasTrail = new bool[33] {
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, true,
        true, false, false, false, false, false, false, false, false, false,
        false, true, true
    };

    private readonly int[][] edges = new int[26][] {
        new int[] {11, 12},
        new int[] {11, 23},
        new int[] {11, 13},
        new int[] {12, 14},
        new int[] {12, 24},
        new int[] {13, 15},
        new int[] {14, 16},
        new int[] {15, 17},
        new int[] {15, 19},
        new int[] {15, 21},
        new int[] {16, 18},
        new int[] {16, 20},
        new int[] {16, 22},
        new int[] {17, 19},
        new int[] {18, 20},
        new int[] {23, 24},
        new int[] {23, 25},
        new int[] {24, 26},
        new int[] {25, 27},
        new int[] {26, 28},
        new int[] {27, 29},
        new int[] {27, 31},
        new int[] {28, 30},
        new int[] {28, 32},
        new int[] {29, 31},
        new int[] {30, 32}
    };

    private readonly GameObject[] bodyLandmarks = new GameObject[33];
    private readonly LineRenderer[] edgesRenderer = new LineRenderer[26];

    private void Start()
    {
        // Instantiate 33 spheres for the body landmarks
        for (int i = 0; i < 33; i++)
        {
            bodyLandmarks[i] = Instantiate(bodyLandmarkPrefab, transform);
            bodyLandmarks[i].name = "BodyLandmark" + i;

            // Enable trail renderer for some landmarks
            if (landmarkHasTrail[i])
                bodyLandmarks[i].transform.GetChild(0).GetComponent<TrailRenderer>().enabled = true;
        }

        // Instantiate the edges between the landmarks
        for (int i = 0; i < edges.Length; i++)
        {
            edgesRenderer[i] = Instantiate(edgePrefab, transform)
                .GetComponent<LineRenderer>();
            edgesRenderer[i].name = "Edge" + i;
        }
    }

    private void SetEdgePositions()
    {
        for (int i = 0; i < edgesRenderer.Length; i++)
        {
            edgesRenderer[i].SetPosition(0, bodyLandmarks[edges[i][0]].transform.position);
            edgesRenderer[i].SetPosition(1, bodyLandmarks[edges[i][1]].transform.position);
        }
    }

    public void SetBodyPosition(string landmarks)
    {
        Debug.Log(landmarks);
        Landmark[] landmarkArray =
            JsonUtility.FromJson<Landmarks>(landmarks).landmarks;

        for (int i = 0; i < Math.Min(33, landmarks.Length); i++)
        {
            bodyLandmarks[i].transform.position = new Vector3(
                landmarkArray[i].x * -1,
                landmarkArray[i].y * -1,
                landmarkArray[i].z * +1
            ) * positionScale;
        }

        SetEdgePositions();
    }
}
