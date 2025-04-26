using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public ObstacleType type;
    private bool obstacleActive = false;
    private RINLBody body;
    private Bounds bounds;

    private void Start()
    {
        // Scale the bounds 
        bounds = type.GetBounds(GameController.Instance.Body.GetBounds());
    }

    private void FixedUpdate()
    {
        if (!obstacleActive)
            return;
        
        if (CheckHit())
        {
            DeactivateObstacle(true);
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            if (type == null)
                return;

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(Vector2.zero, Vector2.one);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(type.position - Vector2.one * .5f, type.size);
        }
        else
        {
            var playerBounds = GameController.Instance.Body.GetBounds();

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(playerBounds.center, playerBounds.size);

            Gizmos.color = Color.green;
            if (obstacleActive)
                Gizmos.DrawCube(bounds.center, bounds.size);
            else
                Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }

    private bool CheckHit()
    {
        for (ushort i = 0; i < Constants.LANDMARKS; i++)
        {
            Vector3 point = body.GetLandmarkWorldPosition(i);
            if (bounds.Contains(point))
                return true;
        }

        return false;
    }

    public void ActivateObstacle()
    {
        obstacleActive = true;
        Invoke("DeactivateObstacle", type.duration);
        body = GameController.Instance.Body; 
    }

    public void DeactivateObstacle(bool hit = false)
    {
        ObstacleCreator creator = FindAnyObjectByType<ObstacleCreator>();
        if (creator == null)
        {
            Debug.LogError("ObstacleCreator not found in the scene.");
            return;
        }

        if (hit)
        {
            creator.StopGame();
        }
        else
        {
            creator.Dodge();
        }

        obstacleActive = false;
        Destroy(gameObject);
    }
}
