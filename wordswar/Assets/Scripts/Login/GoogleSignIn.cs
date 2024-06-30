// Add using directives for better organization
using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Extensions;
using Google;
using UnityEngine.SceneManagement;
using Firebase.Database;
using System.Threading.Tasks;
using Firebase;
public class GoogleSignInManager : MonoBehaviour
{
    public FeedbackManager feedbackManager; // Renamed for consistency
    private FirebaseAuth auth;
    private DatabaseReference databaseReference;

    public TMP_Text errorMessageText;
    private string webClientId = "593667123111-fsuc686571d6b5or4d6f2ms7kdl5supm.apps.googleusercontent.com"; 

    private void Start()
    {
        // Initialize Firebase components
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to initialize Firebase: {task.Exception}");
                return;
            }

            auth = FirebaseAuth.DefaultInstance;
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        });
    }

    public void SignIn()
    {
        Debug.Log("Sign-In button clicked, initializing Google Sign-In.");
        feedbackManager.ShowFeedback("Sign-In button clicked");
        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestIdToken = true,
            RequestEmail = true
        };

        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(FinishSignIn);
    }

    private void FinishSignIn(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted || task.IsCanceled)
        {
            feedbackManager.ShowFeedback("An error occurred while signing in with Google");
            Debug.LogError("An error occurred while signing in with Google: " + task.Exception?.Message);
            return;
        }

        Debug.Log("Google Sign-In successful, signing in with Firebase.");

        Credential credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);
        _ = auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(signInTask =>
        {
            if (signInTask.IsCanceled || signInTask.IsFaulted)
            {
                feedbackManager.ShowFeedback("An error occurred while signing in with Firebase");
                Debug.LogError("An error occurred while signing in with Firebase: " + signInTask.Exception?.Message);
                return;
            }

            FirebaseUser firebaseUser = signInTask.Result;
            feedbackManager.ShowFeedback("Signed in successfully");
            Debug.Log("Signed in successfully as: " + firebaseUser.DisplayName);

            SceneManager.LoadScene("MainMenu");
        });
    }

}
