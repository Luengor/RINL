using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class MouseThing : MonoBehaviour
{
    public Transform target;
    public RectTransform circle;
    public float stillClickTime = 2f;
    public float stillClickDistance = 0.1f;
    public float maxCircleSize = 30;
    public AnimationCurve circleSizeCurve;

    private Camera mainCamera;
    private Vector2 lastPos = Vector2.zero;
    private float stillTime;

    void OnDrawGizmos()
    {
        if (target == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(target.position, stillClickDistance);
    }

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Get on screen position of the target object 
        Vector2 screenPos = mainCamera.WorldToScreenPoint(target.position);

        // Move the circle to the target position
        circle.position = screenPos;

        // Scale the circle based on the still time
        float circleSize = circleSizeCurve.Evaluate(stillTime / stillClickTime) * maxCircleSize;
        circle.sizeDelta = new Vector2(circleSize, circleSize);

        // Queue a mouse event at the position of this object
        InputSystem.QueueStateEvent(Mouse.current, new MouseState
        {
            position = screenPos
        });

        // Check if the mouse is still
        Vector2 normalizedScreenPos = screenPos / Screen.height;
        if (Vector2.SqrMagnitude(lastPos - normalizedScreenPos) < stillClickDistance * stillClickDistance)
        {
            stillTime += Time.deltaTime;

            if (stillTime >= stillClickTime)
            {
                InputSystem.QueueStateEvent(Mouse.current, new MouseState
                {
                    position = screenPos,
                    buttons = 1 << (int)MouseButton.Left
                });
                Debug.Log("Click!");

                stillTime = 0;
            }
        }
        else
        {
            stillTime = 0;
            lastPos = normalizedScreenPos;
        }

        // Send the queued events
        InputSystem.Update();
    }
}
