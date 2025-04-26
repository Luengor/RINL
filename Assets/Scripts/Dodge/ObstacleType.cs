using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleType", menuName = "Scriptable Objects/ObstacleType")]
public class ObstacleType : ScriptableObject
{
    public Vector2 size;
    public Vector2 position;

    public float duration = 1f;

    public Bounds GetBounds(Bounds bodyBounds)
    {
        Vector2 bodyExtents = bodyBounds.size;

        Vector2 actualSize = new(
            size.x * bodyExtents.x,
            size.y * bodyExtents.y
        );

        Vector2 actualPosition = new(
            position.x * bodyExtents.x + bodyBounds.min.x, 
            position.y * bodyExtents.y + bodyBounds.min.y 
        );

        return new Bounds(
            new Vector3(actualPosition.x, actualPosition.y, 0),
            new Vector3(actualSize.x, actualSize.y, 2)
        );
    }
}
