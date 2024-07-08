using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Auth;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }
    public string DefaultNextSceneName = "MainMenu"; // Default next scene if no specific scene is specified

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
        if (auth.CurrentUser != null)
        {
            // User is authenticated, load the MainMenu scene
            Debug.Log("You are authenticated");
            LoadNextScene("MainMenu");
        }
        else
        {
            // User is not authenticated, load the UIManager scene
            Debug.Log("You are not authenticated");
            LoadNextScene("UIManager");
        }
    }

    // Method to load a specific scene, ensuring Firebase is initialized
    public void LoadNextScene(string sceneName)
    {
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
        LoadNextScene("MainMenu");
    }

    // Example method to load Gameplay scene
    public void LoadGameplay()
    {
        LoadNextScene("GamePlay");
    }

    // Example method to load UIManager scene
    public void LoadUiManager()
    {
        LoadNextScene("UIManager");
    }
}
