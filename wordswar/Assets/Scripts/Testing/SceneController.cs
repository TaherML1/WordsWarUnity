using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using System.Collections;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }
    public string DefaultNextSceneName = "MainMenu"; // Default next scene if no specific scene is specified
    public float sceneLoadDelay = 2f; // Delay in seconds before loading scenes

    private void Awake()
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

    private void Start()
    {
        // Check if Firebase is initialized
        if (FirebaseManager.Instance != null)
        {
            if (FirebaseManager.Instance.IsFirebaseInitialized)
            {
                HandleFirebaseInitialized(); // Firebase is already initialized, handle the next step
            }
            else
            {
                // Subscribe to the event when Firebase is initialized
                FirebaseManager.Instance.OnFirebaseInitialized += HandleFirebaseInitialized;
            }
        }
        else
        {
            Debug.LogError("FirebaseManager instance is not found.");
        }
    }

    // Event handler for Firebase initialization
    private void HandleFirebaseInitialized()
    {
        // Check if the user is authenticated
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;

        // Fetch the current user
        FirebaseUser user = auth.CurrentUser;

        if (user != null)
        {
            // User is authenticated, load the MainMenu scene after delay
            Debug.Log("User is authenticated: " + user.UserId);
            StartCoroutine(LoadSceneAfterDelay("MainMenu"));
        }
        else
        {
            // User is not authenticated, load the UIManager scene after delay
            Debug.Log("User is not authenticated.");
            StartCoroutine(LoadSceneAfterDelay("UIManager"));
        }
    }

    // Coroutine to load a specific scene after a delay
    private IEnumerator LoadSceneAfterDelay(string sceneName)
    {
        yield return new WaitForSeconds(sceneLoadDelay);

        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsFirebaseInitialized)
        {
            // Load the specified scene if Firebase is initialized
            Debug.Log($"Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Cannot proceed to the next scene because Firebase is not initialized.");
        }
    }

    // Example method to load MainMenu scene
    public void LoadMainMenu()
    {
        StartCoroutine(LoadSceneAfterDelay("MainMenu"));
    }

    // Example method to load Gameplay scene
    public void LoadGameplay()
    {
        StartCoroutine(LoadSceneAfterDelay("GamePlay"));
    }

    // Example method to load UIManager scene
    public void LoadUiManager()
    {
        StartCoroutine(LoadSceneAfterDelay("UIManager"));
    }
}
