using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public ObstacleType type;
    public Transform visible;
    public AudioSource audioSource;
    public float warningTime, duration;
    private bool obstacleActive = false;
    private RINLBody body;
    private Bounds bounds;

    private void Start()
    {
        // Scale the bounds and the visible game object
        bounds = type.GetBounds(GameController.Instance.Body.GetBounds());
        transform.localPosition = new Vector3(bounds.center.x, bounds.center.y, 0.0f);
        transform.localScale = new Vector3(bounds.size.x, bounds.size.y, 1.0f);

        // Set the animator speed to match the warning time
        GetComponent<Animator>().speed = 1.5f / warningTime;
    }

    private void FixedUpdate()
    {
        if (!obstacleActive)
            return;
        
        if (CheckHit())
        {
            Hit();
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

    public void Beep()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    public void ActivateObstacle()
    {
        obstacleActive = true;
        GetComponent<Animator>().speed = 1.0f / Mathf.Min(type.duration, duration);
        body = GameController.Instance.Body; 
    }

    public void Hit()
    {
        DeactivateObstacle(true);
    }

    public void Deactivate()
    {
        DeactivateObstacle(false);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    private void DeactivateObstacle(bool hit)
    {
        ObstacleCreator creator = FindAnyObjectByType<ObstacleCreator>();
        if (creator != null && creator.Gaming)
        {
            if (hit)
                creator.StopGame();
            else
                creator.Dodge();
        }

        obstacleActive = false;
    }
}
