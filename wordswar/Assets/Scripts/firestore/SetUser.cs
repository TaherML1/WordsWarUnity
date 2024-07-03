using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Functions;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions; // Needed for regex validation

public class SetUser : MonoBehaviour
{
    private FirebaseFunctions functions;

    public RadialProgressBar radialProgressBar;
    
    private bool firebaseInitialized = false; // Flag to track Firebase initialization
    public TMP_InputField usernameInputField;
    public TextMeshProUGUI responseText;
    public GameObject setUserPanel;
    public GameObject Shadow;

    void Start()
    {
        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to initialize Firebase with {task.Exception}");
                return;
            }

            functions = FirebaseFunctions.DefaultInstance;
            firebaseInitialized = true;
        });
    }

    public async void CallSetUserFunction()
    {
        Shadow.SetActive(true);
        radialProgressBar.StartSpinning();
        if (!firebaseInitialized)
        {
            Debug.LogError("Firebase not yet initialized. Please wait.");
            responseText.text = "Firebase initialization in progress. Please wait.";
            Shadow.SetActive(false);
            radialProgressBar.StopSpinning();
            return;
        }

        if (functions == null)
        {
            Debug.LogError("Firebase Functions not initialized.");
            responseText.text = "Firebase Functions not initialized.";
            Shadow.SetActive(false);
            radialProgressBar.StopSpinning();
            return;
        }

        // Get the username from the input field
        string username = usernameInputField.text;

        // Sanitize and validate the username
        if (!IsValidUsername(username))
        {
            Debug.LogError("Invalid username. Only letters and numbers are allowed.");
            responseText.text = "Invalid username. Only letters and numbers are allowed.";
            Shadow.SetActive(false);
            radialProgressBar.StopSpinning();
            return;
        }

        // Prepare data to send to the Cloud Function
        var data = new Dictionary<string, object>
        {
            { "username", username }
        };

        try
        {
            // Call the Cloud Function
            var result = await functions.GetHttpsCallable("setUser2").CallAsync(data);

            // Handle the result
            Debug.Log(result.Data);
            Debug.Log("Profile saved successfully");
            responseText.text = "Profile saved successfully";

            // Call CheckUserProfileCompletion in FetchUserProfile after setting the username
            UserManager.Instance.ListenForUserDataChanges();
            setUserPanel.SetActive(false);
            Shadow.SetActive(false);
             radialProgressBar.StopSpinning();

        }
        catch (System.Exception e)
        {
            // Handle any errors
            Debug.LogError($"Error calling function: {e.Message}");
            responseText.text = "Error saving profile. Please try again.";
            Shadow.SetActive(false);
            radialProgressBar.StopSpinning();
        }
    }

    // Method to validate the username
    private bool IsValidUsername(string username)
    {
        // Regular expression to match letters (both English and Arabic) and numbers
        // Including more Arabic Unicode ranges if needed
        string pattern = @"^[a-zA-Z0-9\u0600-\u06FF]+$";
        return Regex.IsMatch(username, pattern);
    }
}
