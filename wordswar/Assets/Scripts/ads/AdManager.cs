using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;

public class AdManager : MonoBehaviour
{
    private BannerView _bannerView;
    private InterstitialAd _interstitialAd;
    private RewardedAd _rewardedAdTicket;
    private RewardedAd _rewardedAdSpin;
    private RewardedAd _rewardedAdCoins;

    public string adUnitIdBanner;
    public string adUnitIdInterstitial;
    public string adUnitIdRewardedTicket;
    public string adUnitIdRewardedCoins;
    public string adUnitIdRewardedSpin;
    public TimerManager timerManager;
    public SpinWheel spinWheel;

    void Start()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            // This callback is called once the MobileAds SDK is initialized.
            Debug.Log("Google Mobile Ads SDK initialized.");
            // Request ads
            RequestInterstitial();
            RequestRewardedAds();
            RequestRewardedAdCoins();
        });

        
    }

    // Interstitial Ads
    private void RequestInterstitial()
    {
#if UNITY_ANDROID
        adUnitIdInterstitial = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
        adUnitIdInterstitial = "";
#else
        adUnitIdInterstitial = "unexpected_platform";
#endif

        Debug.Log("Creating interstitial view");

        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        Debug.Log("Loading the interstitial ad.");

        var adRequest = new AdRequest();
        InterstitialAd.Load(adUnitIdInterstitial, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Interstitial ad failed to load an ad with error: " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response: " + ad.GetResponseInfo());
                _interstitialAd = ad;
            });
    }

    public void ShowInterstitialAd()
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            _interstitialAd.Show();
        }
        else
        {
            Debug.LogError("Interstitial ad is not ready yet.");
        }
    }

    // Rewarded Ads
    private void RequestRewardedAds()
    {
#if UNITY_ANDROID
        adUnitIdRewardedTicket = "ca-app-pub-8174773242665808/5141384415"; // Ticket reward ad unit
        adUnitIdRewardedSpin = "ca-app-pub-8174773242665808/6885231999"; // Spin reward ad unit 
        adUnitIdRewardedCoins = "ca-app-pub-8174773242665808/5684145598"; // coind reward ad unit
#elif UNITY_IPHONE
        adUnitIdRewardedTicket = "";
        adUnitIdRewardedSpin = "";
#else
        adUnitIdRewardedTicket = "unexpected_platform";
        adUnitIdRewardedSpin = "unexpected_platform";
#endif

        RequestRewardedAdTicket();
        RequestRewardedAdSpin();
    }

    private void RequestRewardedAdTicket()
    {
        if (_rewardedAdTicket != null)
        {
            _rewardedAdTicket.Destroy();
            _rewardedAdTicket = null;
        }

        Debug.Log("Loading the rewarded ad for ticket.");

        var adRequest = new AdRequest();
        RewardedAd.Load(adUnitIdRewardedTicket, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad for ticket failed to load an ad with error: " + error);
                    return;
                }

                Debug.Log("Rewarded ad for ticket loaded with response: " + ad.GetResponseInfo());
                _rewardedAdTicket = ad;
            });
    }

    private void RequestRewardedAdSpin()
    {
        if (_rewardedAdSpin != null)
        {
            _rewardedAdSpin.Destroy();
            _rewardedAdSpin = null;
        }

        Debug.Log("Loading the rewarded ad for spin.");

        var adRequest = new AdRequest();
        RewardedAd.Load(adUnitIdRewardedSpin, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad for spin failed to load an ad with error: " + error);
                    return;
                }

                Debug.Log("Rewarded ad for spin loaded with response: " + ad.GetResponseInfo());
                _rewardedAdSpin = ad;
            });
    }

    private void RequestRewardedAdCoins()
    {
        if (_rewardedAdCoins != null)
        {
            _rewardedAdCoins.Destroy();
            _rewardedAdCoins = null;
        }

        Debug.Log("Loading the rewarded ad for coins.");

        var adRequest = new AdRequest();
        RewardedAd.Load(adUnitIdRewardedCoins, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad for coins failed to load an ad with error: " + error);
                    return;
                }

                Debug.Log("Rewarded ad for coins loaded with response: " + ad.GetResponseInfo());
                _rewardedAdCoins = ad;
            });
    }

    public void ShowRewardedAdTicket()
    {
        if (_rewardedAdTicket != null && _rewardedAdTicket.CanShowAd())
        {
            _rewardedAdTicket.Show(reward =>
            {
                Debug.Log($"Rewarded ad for ticket rewarded the user. Type: {reward.Type}, amount: {reward.Amount}");
                timerManager.IncreaseTicketsLocally();
                // Reload the ad after it is shown
                RequestRewardedAdTicket();
            });
        }
        else
        {
            Debug.LogError("Rewarded ad for ticket is not ready yet.");
        }
    }

    public void ShowRewardedAdSpin()
    {
        if (_rewardedAdSpin != null && _rewardedAdSpin.CanShowAd())
        {
            _rewardedAdSpin.Show(reward =>
            {
                Debug.Log($"Rewarded ad for spin rewarded the user. Type: {reward.Type}, amount: {reward.Amount}");
                spinWheel.IncreaseSpinTicket();
                // Reload the ad after it is shown
                RequestRewardedAdSpin();
            });
        }
        else
        {
            Debug.LogError("Rewarded ad for spin is not ready yet.");
        }
    }
    public void ShowRewardedAd(System.Action onAdCompleted)
    {
        if (_rewardedAdCoins != null && _rewardedAdCoins.CanShowAd())
        {
            _rewardedAdCoins.Show(reward =>
            {
                Debug.Log("Rewarded ad completed.");
                onAdCompleted?.Invoke();
                RequestRewardedAdCoins(); // Reload the ad after it is shown
            });
        }
        else
        {
            Debug.LogError("Rewarded ad is not ready yet.");
        }
    }
}
