using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Functions;
using UnityEngine;

public class SetTimer : MonoBehaviour
{
    private FirebaseFunctions functions;
    private FirebaseAuth auth;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                functions = FirebaseFunctions.DefaultInstance;

                // Assuming the user is already authenticated
                FirebaseUser user = auth.CurrentUser;

                if (user != null)
                {
                    CallSetTimerFunction();
                }
                else
                {
                    Debug.LogError("User is not authenticated.");
                }
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });
    }

    void CallSetTimerFunction()
    {
        var function = functions.GetHttpsCallable("setTimer");
        function.CallAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Cloud Function 'setTimer' called successfully.");
            }
            else
            {
                Debug.LogError("Error calling Cloud Function 'setTimer': " + task.Exception);
            }
        });
    }
}
