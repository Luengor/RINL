using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public Bounds bounds;
    private bool obstacleActive = false;
    private RINLBody body;

    private void Start()
    {
        ActivateObstacle();
    }

    private void FixedUpdate()
    {
        bounds.center = transform.position;
        
        if (CheckHit())
        {
            DeactivateObstacle();
            Debug.Log("Obstacle hit!");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 center = Application.isPlaying ? bounds.center : transform.position;
        if (!obstacleActive)
            Gizmos.DrawWireCube(center, bounds.size);
        else
            Gizmos.DrawCube(center, bounds.size);
    }

    private bool CheckHit()
    {
        if (!obstacleActive)
            return false;

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
        body = GameController.Instance.Body; 
    }

    public void DeactivateObstacle()
    {
        obstacleActive = false;
    }
}
