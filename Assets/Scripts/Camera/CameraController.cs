using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // Assign your virtual camera here
    public float startZoom = 2f; // Starting zoom value
    public float endZoom = 5f; // End zoom value
    public float duration = 2f; // Duration of the zoom
    
    
    private float timer = 0f;

    void Start()
    {
        // Set initial zoom
        virtualCamera.m_Lens.OrthographicSize = startZoom;
        
    }

    void Update()
    {
        // Increase the timer based on the time passed
        if (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration; // Normalize the time between 0 and 1
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(startZoom, endZoom, t);
        }
        
    }
}
