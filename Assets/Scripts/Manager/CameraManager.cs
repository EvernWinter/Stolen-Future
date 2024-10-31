using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private GameObject confinerGameObject; // Assign in inspector
    private CinemachineConfiner2D confiner; // Store the CinemachineConfiner2D component
    public Collider2D bound;

    private void Awake()
    {
        if (confinerGameObject != null)
        {
            var virtualCamera = confinerGameObject.GetComponent<CinemachineVirtualCamera>();

            if (virtualCamera != null)
            {
                confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();

                if (confiner == null)
                {
                    //Debug.LogError("Cinemachine Confiner 2D component not found on the assigned GameObject!");
                }
                else
                {
                    //Debug.Log("Cinemachine Confiner 2D found.");
                }
            }
            else
            {
                //Debug.LogError("CinemachineVirtualCamera component not found on the assigned GameObject!");
            }
        }
        else
        {
            //Debug.LogError("Confiner GameObject is not assigned in the inspector.");
        }
    }

    private void Start()
    {
        if (confiner != null)
        {
            StartCoroutine(DelayedConfinerSetup());
        }
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (confiner != null && bound != null)
        {
            confiner.InvalidateCache(); // Refresh the confiner path
            //Debug.Log("Confiner bounds updated on scene load.");
        }
    }

    private IEnumerator DelayedConfinerSetup()
    {
        yield return new WaitForSeconds(0.1f);

        if (confiner != null && bound != null)
        {
            confiner.m_BoundingShape2D = bound; // Set the bounding shape
            //Debug.Log("Confiner bounds setup after initial delay.");

            for (int i = 0; i < 3; i++)
            {
                // Set the max window size to -1
                confiner.m_MaxWindowSize = -1f;
                //Debug.Log("MaxWindowSize set to -1f.");

                // Wait for 0.5 seconds
                yield return new WaitForSeconds(0.5f);

                // Change max window size to 10f
                confiner.m_MaxWindowSize = 1f;
                //Debug.Log("MaxWindowSize changed to 1f.");

                // Wait for another 0.5 seconds
                yield return new WaitForSeconds(0.5f);
                
                confiner.m_MaxWindowSize = -1f;
                //Debug.Log("MaxWindowSize changed to -1f.");
            }
        }
        else
        {
            //Debug.LogWarning("Cinemachine Confiner 2D or bounding shape is not assigned or found.");
        }
    }
}
