using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using TMPro;
using UnityEngine.Purchasing.Extension;

public class IAPStore : MonoBehaviour
{
    [Header("Comsumable")]
    [SerializeField] public TextMeshProUGUI coinText;
    
    [Header("Non-comsumable")]
    [SerializeField] public GameObject adsPurchasedWindow;
    [SerializeField] public GameObject adsBanner;

    [Header("Subscription")] 
    [SerializeField] public GameObject subActivateWindow;
    [SerializeField] public GameObject premiumLogo;
    // Start is called before the first frame update
    void Start()
    {
        //coinText.text = PlayerPrefs.GetInt("totalCoins").ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription reason)
    {
        Debug.Log(product.definition.id);
        Debug.Log(reason.reason);
    }

    public void OnPurchase100CoinComplete(Product product)
    {
        Debug.Log(product.definition.id);
        AddCoins(100);
    }

    public void AddCoins(int num)
    {
        int coins = PlayerPrefs.GetInt("total_coins");
        coins += num;
        PlayerPrefs.SetInt("totalCoins", coins);
        coinText.text = "Current Coin: "+ coins;
    }

    void DisplayAds(bool active)
    {
        if (!active)
        {
            adsPurchasedWindow.SetActive(true);
            adsBanner.SetActive(false);
        }
        else
        {
            adsPurchasedWindow.SetActive(false);
            adsBanner.SetActive(true);
        }
    }

    void RemoveAds()
    {
        DisplayAds(false);
    }

    void ShowAds()
    {
        DisplayAds(true);
    }

    public void OnPurchaseRemoveAdsComplete(Product product)
    {
        Debug.Log(product.definition.id);
        RemoveAds();
    }

    public void CheckNonConsumable(Product product)
    {
        if (product != null)
        {
            if (product.hasReceipt)
            {
                RemoveAds();   
            }
            else
            {
                ShowAds();
            }
        }
    }

    void SetUpVIP(bool active)
    {
        if (active)
        {
            subActivateWindow.SetActive(true);
            premiumLogo.SetActive(true);
        }
        else
        {
            subActivateWindow.SetActive(false);
            premiumLogo.SetActive(false);
        }
    }

    void ActivateVIP()
    {
        SetUpVIP(true);
    }
    
    void DeactivateVIP()
    {
        SetUpVIP(false);
    }
    
    public void OnPurchaseActiveVIPComplete(Product product)
    {
        Debug.Log(product.definition.id);
        ActivateVIP();
        
    }

    public void CheckSubscription(Product subProduct)
    {
        try
        {
            if (subProduct.hasReceipt)
            {
                var subManager = new SubscriptionManager(subProduct, null);
                var info = subManager.getSubscriptionInfo();

                if (info.isSubscribed() == Result.True)
                {
                    Debug.Log("User subscribed");
                    ActivateVIP();
                }
                else
                {
                    Debug.Log("User not subscribed");
                    DeactivateVIP();
                }
            }
            else
            {
                Debug.Log("Not found");
            }
        }
        catch (Exception)
        {
            Debug.Log("Its only work for google store, appstore");
        }
    }
}
