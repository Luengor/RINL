using UnityEngine;

public class JSConnector : MonoBehaviour
{
    public RINLBody body;

    public void SetBodyPosition(string landmarkString)
    {
        body.SetBodyPosition(landmarkString);
    }

    public void SetImageSize(string sizeString)
    {
        body.SetImageSize(sizeString);
    }
}
