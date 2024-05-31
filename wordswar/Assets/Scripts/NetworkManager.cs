using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }
    public TMP_Text statusText;
    public GameObject internetConnectionPanel;
    public GameObject shadowPanel;
    public GameObject loginManager;
    public float checkInterval = 5f; // Check interval in seconds
    private float timeSinceLastCheck = 0f;
    private bool previousConnectionStatus;
    private bool currentConnectionStatus = false;

   

    void Start()
    {
        CheckInternetConnection();
    }

   void Update()
    {
        timeSinceLastCheck += Time.deltaTime;
        if (timeSinceLastCheck >= checkInterval)
        {
            CheckInternetConnection();
            timeSinceLastCheck = 0f;
        }
    }

    void CheckInternetConnection()
    {
        bool isConnected = Application.internetReachability != NetworkReachability.NotReachable;
        if (isConnected)
        {
            if (!previousConnectionStatus)
            {
                currentConnectionStatus = true;
                // Internet connection restored

                statusText.text = "Internet Connection Restored!";
            
                loginManager.SetActive(true);

                // Optionally, you can resume any pending tasks or notify the user here
            }
            previousConnectionStatus = true;
          
        }
        else
        {
            // No internet connection
            currentConnectionStatus |= false;
            statusText.text = "No Internet Connection!";
            previousConnectionStatus = false;
            internetConnectionPanel.SetActive(true);
            shadowPanel.SetActive(true);
            loginManager.SetActive(false);

            // Optionally, you can prevent further progress in your app here
        }
    }
    public void AttemptReconnect()
    {     
        // Call the CheckInternetConnection method to attempt reconnection
        if (currentConnectionStatus == true)
        {
            internetConnectionPanel.SetActive(false) ;
            shadowPanel.SetActive(false);
        }
  
    }
}
