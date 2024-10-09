using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private BoxCollider2D boundBox; // Assign the bounding box in the Inspector
    private Camera cam;
    private float camHalfHeight;
    private float camHalfWidth;

    private void Start()
    {
        cam = Camera.main;
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;
    }

    private void LateUpdate()
    {
        // Get the camera's current position
        Vector3 newPosition = transform.position;

        // Calculate the bounds of the bounding box
        Bounds bounds = boundBox.bounds;

        // Clamp the camera position so it doesn't exceed the bounds
        float minX = bounds.min.x + camHalfWidth;
        float maxX = bounds.max.x - camHalfWidth;
        float minY = bounds.min.y + camHalfHeight;
        float maxY = bounds.max.y - camHalfHeight;

        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        // Apply the clamped position to the camera
        transform.position = newPosition;
    }
}
