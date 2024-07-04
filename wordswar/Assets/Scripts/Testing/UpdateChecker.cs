using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

public class UpdateChecker : MonoBehaviour
{
    FirebaseFirestore db;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                Debug.Log("Firebase initialized successfully.");
                CheckForUpdate();  // Make sure this is called
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {task.Result}");
            }
        });
    }

    void CheckForUpdate()
    {
        Debug.Log("Checking for updates...");  // Confirming method is called

        DocumentReference docRef = db.Collection("appConfig").Document("versionInfo");
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Exception != null)
                {
                    Debug.LogError("Error retrieving version information: " + task.Exception);
                    return;
                }

                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    string latestVersion = snapshot.GetValue<string>("latestVersion");
                    string currentVersion = Application.version;

                    Debug.Log($"Current Version: {currentVersion}, Latest Version: {latestVersion}");

                    if (IsNewVersionAvailable(currentVersion, latestVersion))
                    {
                        ShowUpdatePrompt();
                    }
                    else
                    {
                        Debug.Log("The game is up to date.");
                    }
                }
                else
                {
                    Debug.Log("No version information found in Firestore.");
                }
            }
            else
            {
                Debug.LogError("Error completing task: " + task.Exception);
            }
        });
    }

    bool IsNewVersionAvailable(string currentVersion, string latestVersion)
    {
        Debug.Log($"Comparing versions: Current ({currentVersion}) vs Latest ({latestVersion})");
        return string.Compare(currentVersion, latestVersion) < 0;
    }

    void ShowUpdatePrompt()
    {
        // Implement your update prompt logic here
        Debug.Log("Update required! Please download the latest version.");
        // You could show a UI dialog here or redirect to the app store.
    }
}
