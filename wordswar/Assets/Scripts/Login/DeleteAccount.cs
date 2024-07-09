using Firebase;
using Firebase.Auth;
using Firebase.Functions;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Analytics;
using System.Collections;
using UnityEngine.SceneManagement;

public class DeleteAccount : MonoBehaviour
{
    [SerializeField] Button deleteAccountButton;

    private FirebaseAuth auth;
    private FirebaseFunctions functions;

    private void Start()
    {
        // Check if Firebase is initialized
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsFirebaseInitialized)
        {
            InitializeFirebaseComponents();
        }
        else
        {
            // Wait until Firebase is initialized
            StartCoroutine(WaitForFirebaseInitialization());
        }
    }

    private void InitializeFirebaseComponents()
    {
        auth = FirebaseAuth.DefaultInstance;
        functions = FirebaseFunctions.DefaultInstance;
        deleteAccountButton.onClick.AddListener(OnDeleteAccountButtonClick);

        if (auth.CurrentUser != null)
        {
            // Additional initialization logic if needed
        }
        else
        {
            Debug.LogError("No user is currently logged in.");
        }
    }

    private IEnumerator WaitForFirebaseInitialization()
    {
        // Wait until Firebase is initialized
        while (!FirebaseManager.Instance.IsFirebaseInitialized)
        {
            yield return null;
        }

        // Firebase is now initialized, initialize Firebase components
        InitializeFirebaseComponents();
    }
    public void ClearFirebaseAuthState()
    {
        FirebaseAuth.DefaultInstance.SignOut();
    }


    private void OnDeleteAccountButtonClick()
    {
        // Prompt user to confirm account deletion
        // Re-authenticate the user if necessary

        functions.GetHttpsCallable("deleteUserAccount").CallAsync()
            .ContinueWith(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("Account deletion failed: " + task.Exception);
                }
                else
                {
                    Debug.Log("Account deleted successfully.");
                    ClearFirebaseAuthState();
                    SceneController.Instance.LoadUiManager();

                }
            });
    }
}
