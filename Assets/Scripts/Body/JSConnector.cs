using UnityEngine;

public class JSConnector : MonoBehaviour
{
    public RINLBody body;

    public void SetBodyPosition(string landmarkString)
    {
        body.UpdateBodyLandmarks(landmarkString);
    }

    public void SetVideoSize(string sizeString)
    {
        body.SetVideoSize(sizeString);
    }
}
