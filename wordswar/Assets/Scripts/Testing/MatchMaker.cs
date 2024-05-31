using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MatchMaker : MonoBehaviour
{
    // Replace this with your Cloud Function endpoint URL
    private string cloudFunctionURL = "YOUR_CLOUD_FUNCTION_ENDPOINT_URL";

    // Function to trigger the Cloud Function for matchmaking
    public void JoinMatchmakingQueue()
    {
        StartCoroutine(TriggerMatchmakerCloudFunction());
    }

    // Coroutine to send HTTP request to trigger the Cloud Function
    private IEnumerator TriggerMatchmakerCloudFunction()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(cloudFunctionURL))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Matchmaking request sent successfully!");
                // Handle the response from the Cloud Function if needed
            }
            else
            {
                Debug.LogError("Matchmaking request failed: " + webRequest.error);
            }
        }
    }
}
