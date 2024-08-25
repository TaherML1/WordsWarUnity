using System.Collections;
using UnityEngine;
using GoogleMobileAds.Api;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    private RewardedAd _rewardedAdTicket;
    private RewardedAd _rewardedAdSpin;
    private RewardedAd _rewardedAdCoins;

    public string adUnitIdRewardedTicket;
    public string adUnitIdRewardedSpin;
    public string adUnitIdRewardedCoins;

    private float adReloadInterval = 1800f; // 30 minutes
    private int maxRetryAttempts = 3;
    private float retryDelay = 5f; // 5 seconds

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            Debug.Log("Google Mobile Ads SDK initialized.");
            RequestRewardedAds();
            StartCoroutine(ReloadAdsCoroutine());
        });
    }

    private void RequestRewardedAds()
    {
        RequestRewardedAdTicket();
        RequestRewardedAdSpin();
        RequestRewardedAdCoins();
    }

    private void RequestRewardedAdTicket()
    {
        StartCoroutine(LoadRewardedAdWithRetry(adUnitIdRewardedTicket, (ad) => _rewardedAdTicket = ad));
    }

    private void RequestRewardedAdSpin()
    {
        StartCoroutine(LoadRewardedAdWithRetry(adUnitIdRewardedSpin, (ad) => _rewardedAdSpin = ad));
    }

    private void RequestRewardedAdCoins()
    {
        StartCoroutine(LoadRewardedAdWithRetry(adUnitIdRewardedCoins, (ad) => _rewardedAdCoins = ad));
    }

    private IEnumerator LoadRewardedAdWithRetry(string adUnitId, System.Action<RewardedAd> onAdLoaded)
    {
        int retryAttempts = 0;
        while (retryAttempts < maxRetryAttempts)
        {
            var adRequest = new AdRequest();
            RewardedAd.Load(adUnitId, adRequest,
                (RewardedAd ad, LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        Debug.LogError("Rewarded ad failed to load with error: " + error);
                        retryAttempts++;
                        if (retryAttempts < maxRetryAttempts)
                        {
                            StartCoroutine(RetryLoadAd(retryDelay));
                        }
                    }
                    else
                    {
                        Debug.Log("Rewarded ad loaded with response: " + ad.GetResponseInfo());
                        onAdLoaded(ad);
                    }
                });
            yield return null;
        }
    }

    private IEnumerator RetryLoadAd(float delay)
    {
        yield return new WaitForSeconds(delay);
        RequestRewardedAds();
    }

    private IEnumerator ReloadAdsCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(adReloadInterval);
            RequestRewardedAds();
        }
    }

    public void ShowRewardedAdTicket()
    {
        if (_rewardedAdTicket != null && _rewardedAdTicket.CanShowAd())
        {
            _rewardedAdTicket.Show(reward =>
            {
                Debug.Log($"Rewarded ad for ticket rewarded the user. Type: {reward.Type}, amount: {reward.Amount}");
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
