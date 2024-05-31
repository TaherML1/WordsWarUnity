using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Functions;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SetUser : MonoBehaviour
{
    private FirebaseFunctions functions;
    private bool firebaseInitialized = false; // Flag to track Firebase initialization
    public TMP_InputField usernameInputField;
    public TextMeshProUGUI responseText;
    public GameObject setUserPanel;

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
        if (!firebaseInitialized)
        {
            Debug.LogError("Firebase not yet initialized. Please wait.");
            return;
        }

        if (functions == null)
        {
            Debug.LogError("Firebase Functions not initialized.");
            return;
        }

        // Get the username from the input field
        string username = usernameInputField.text;

        // Prepare data to send to the Cloud Function
        var data = new Dictionary<string, object>
        {
            { "username", username }, // Use the username from the input field
            // Add other user data fields here if needed
        };

        try
        {
            // Call the Cloud Function
            var result = await functions.GetHttpsCallable("setUser").CallAsync(data);

            // Handle the result
            Debug.Log(result.Data);
            Debug.Log("Profile saved successfully");
            responseText.text = "Profile saved successfully";

            // Call CheckUserProfileCompletion in FetchUserProfile after setting the username
            UserManager.Instance.ListenForUserDataChanges();
            setUserPanel.SetActive(false);
            
        }
        catch (System.Exception e)
        {
            // Handle any errors
            Debug.LogError($"Error calling function: {e.Message}");
        }
    }
}
