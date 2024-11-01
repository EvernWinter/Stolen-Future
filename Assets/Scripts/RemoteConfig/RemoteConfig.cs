using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using System.Threading.Tasks;
using Unity.Services.RemoteConfig;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class RemoteConfig : MonoBehaviour
{
    public static RemoteConfig Instance {get; private set;}

    
    public bool isEvent = false;
    public Event onEvent;
    public struct userAttributes
    {
        public int score;
    }
    public struct appAttributes {}

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    async Task InitializeRemoteConfigAsync()
    {
            // initialize handlers for unity game services
            await UnityServices.InitializeAsync();

            // remote config requires authentication for managing environment information
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
    }

    async Task Start()
    {
        // initialize Unity's authentication and core services, however check for internet connection
        // in order to fail gracefully without throwing exception if connection does not exist
        if (Utilities.CheckForInternetConnection())
        {
            await InitializeRemoteConfigAsync();
        }

        userAttributes uaStruct = new userAttributes
        {
            score = 10
        };
       

        RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;
        RemoteConfigService.Instance.FetchConfigs(uaStruct, new appAttributes());
    }

    void ApplyRemoteSettings(ConfigResponse configResponse)
    {
        switch (configResponse.requestOrigin)
        {
            case ConfigOrigin.Default:
                Debug.Log($"No Setting Loaded in this session; using Default values");
                break;
            case ConfigOrigin.Cached:
                Debug.Log($"No Setting Loaded in this session; using Cached values");
                break;
            case ConfigOrigin.Remote:
                Debug.Log($"New Setting Loaded in this session; update value accordingly");
                isEvent = RemoteConfigService.Instance.appConfig.GetBool("isEvent");
                onEvent.SetEvent(isEvent);
                break;
        }
        Debug.Log("RemoteConfigService.Instance.appConfig fetched: " + RemoteConfigService.Instance.appConfig.config.ToString());
    }
}
