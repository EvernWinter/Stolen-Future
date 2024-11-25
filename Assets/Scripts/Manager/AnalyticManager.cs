using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

public class AnalyticManager : MonoBehaviour
{
    public static AnalyticManager instance {get; private set;}
    private bool _isInitialized = false;
    // Start is called before the first frame update

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        AnalyticsService.Instance.StartDataCollection();
        _isInitialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Score(int score)
    {
        if (!_isInitialized)
        {
            return;
        }

        CustomEvent myEvent = new CustomEvent("Score")
        {
            {"Score", score }
        };
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
        Debug.Log("Score Updated");
    }
    
    public void Survive(int time)
    {
        if (!_isInitialized)
        {
            return;
        }

        CustomEvent myEvent = new CustomEvent("Time")
        {
            {"Time", time }
        };
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
        Debug.Log("Time Updated");
    }
    
    public void Dead(int die)
    {
        if (!_isInitialized)
        {
            return;
        }

        CustomEvent myEvent = new CustomEvent("Die")
        {
            {"Die", die }
        };
        AnalyticsService.Instance.RecordEvent(myEvent);
        AnalyticsService.Instance.Flush();
        Debug.Log("Dead Updated");
    }
}
