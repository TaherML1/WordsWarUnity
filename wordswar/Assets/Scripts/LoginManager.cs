using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Auth;

public class LoginManager : MonoBehaviour
{
    FirebaseAuth auth;

    void Start()
    {
        // Initialize Firebase Auth
        auth = FirebaseAuth.DefaultInstance;

        // Check if a user is already signed in
        if (auth.CurrentUser != null)
        {
            // If a user is signed in, load the main menu directly
            LoadMainMenu();
        }
        else
        {
            SceneManager.LoadScene("UIManager");
        }
    }

    // Function to load the main menu scene
    void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Replace "MainMenu" with your actual scene name
    }

    // Function to handle login button click
    public void OnLoginButtonClicked()
    {
        // Implement your login logic here (e.g., using email/password authentication)

        // After successful login, load the main menu
        LoadMainMenu();
    }

    // Function to handle register button click
    public void OnRegisterButtonClicked()
    {
        // Implement your registration logic here

        // After successful registration, load the main menu
        LoadMainMenu();
    }

    // Function to handle logout button click
    public void OnLogoutButtonClicked()
    {
        // Sign out the user
        FirebaseAuth.DefaultInstance.SignOut();

        // Load the login scene
        SceneManager.LoadScene("UIManager"); // Replace "LoginScene" with the name of your login scene

    }
}