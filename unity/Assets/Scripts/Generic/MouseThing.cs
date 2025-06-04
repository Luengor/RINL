using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class MouseThing : MonoBehaviour
{
    public Transform target;
    public Color circleColor = Color.white;
    public RectTransform circle;
    public float stillClickTime = 2f;
    public float stillClickDistance = 0.1f;
    public float maxCircleSize = 30;
    public AnimationCurve circleSizeCurve;

    private Camera mainCamera;
    private Vector2 lastPos = Vector2.zero;
    private float stillTime;
    private Mouse mouse;

    void Awake()
    {
        mouse = InputSystem.AddDevice<Mouse>();
    }

    void Start()
    {
        mainCamera = Camera.main;
        circle.GetComponent<Image>().color = circleColor;
    }

    void Update()
    {
        // Get on screen position of the target object 
        Vector2 screenPos = mainCamera.WorldToScreenPoint(target.position);

        // Check if there is a button in the position
        var eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPos
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        bool hasButton = false;
        for (int i = 0; i < results.Count; i++)
            if (results[i].gameObject.GetComponent<Button>() != null)
            {
                hasButton = true;
                break;
            }
        
        // If there is no button, exit early
        if (!hasButton)
        {
            if (stillTime > 0)
            {
                // Queue a mouse event to move the mouse out
                InputSystem.QueueStateEvent(mouse, new MouseState
                {
                    position = screenPos
                });
                stillTime = 0;
                circle.sizeDelta = Vector2.zero; // Hide the circle
            }

            return;
        }

        // If there is button
        // Move the circle to the target position
        circle.position = screenPos;

        // Scale the circle based on the still time
        float circleSize = circleSizeCurve.Evaluate(stillTime / stillClickTime) * maxCircleSize;
        circle.sizeDelta = new Vector2(circleSize, circleSize);

        // Queue a mouse event at the position of this object
        InputSystem.QueueStateEvent(mouse, new MouseState
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
                InputSystem.QueueStateEvent(mouse, new MouseState
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
