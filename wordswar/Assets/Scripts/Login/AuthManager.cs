using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Firebase.Analytics;
using Firebase.Firestore;
using Firebase.Functions;
using System;

public class AuthManager : MonoBehaviour
{
    public FeedbackManager feedbackManager;
    public RadialProgressBar radialProgressBar;

    FirebaseUser fuser;
    FirebaseFunctions functions;
    FirebaseFirestore db;

    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;
    FirebaseApp app;

    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    [Header("Reset Password")]
    public TMP_InputField emailResetPasswordField;
    public TMP_Text warningResetPasswordText;
    public TMP_Text confirmResetPasswordText;

    void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        app = FirebaseApp.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        fuser = auth.CurrentUser;
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        functions = FirebaseFunctions.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }

    public void RegisterButton()
    {
        RegisterAsync(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text).Forget();
    }

    public void LoginButton()
    {
        LoginAsync(emailLoginField.text, passwordLoginField.text).Forget();
    }

    public void ResetPasswordButton()
    {
        ResetPasswordAsync(emailResetPasswordField.text).Forget();
    }

    private async Task LoginAsync(string _email, string _password)
    {
        radialProgressBar.StartSpinning();

        try
        {
            var loginTask = await auth.SignInWithEmailAndPasswordAsync(_email, _password);
            User = loginTask.User;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);

            feedbackManager.ShowFeedback("Logged In");
            radialProgressBar.StopSpinning();

            // Log success event
            FirebaseAnalytics.LogEvent("login_success",
                new Parameter("user_id", User.UserId),
                new Parameter("email", _email),
                new Parameter("timestamp", DateTime.UtcNow.ToString("o"))); // ISO 8601 format

            SceneManager.LoadScene(SceneNames.MainMenu);
            FirebaseAnalytics.LogEvent("scene_change", new Parameter("scene_name", SceneNames.MainMenu));
        }
        catch (FirebaseException ex)
        {
            Debug.LogWarning($"Failed to login: {ex.Message}");
            AuthError errorCode = (AuthError)ex.ErrorCode;
            string message = GetErrorMessage(errorCode);
            feedbackManager.ShowFeedback(message);
            radialProgressBar.StopSpinning();

            // Log failure event

        }
    }

    private async Task RegisterAsync(string _email, string _password, string _username)
    {
        if (string.IsNullOrEmpty(_username))
        {
            feedbackManager.ShowFeedback("Missing Username");
            return;
        }

        if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            feedbackManager.ShowFeedback("Password Does Not Match!");
            return;
        }

        radialProgressBar.StartSpinning();
        try
        {
            var userCredential = await auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            User = userCredential.User;

            UserProfile profile = new UserProfile { DisplayName = _username };
            await User.UpdateUserProfileAsync(profile);

            // Log success event
            FirebaseAnalytics.LogEvent("register_success",
                new Parameter("user_id", User.UserId),
                new Parameter("email", _email),
                new Parameter("username", _username),
                new Parameter("timestamp", DateTime.UtcNow.ToString("o")));

            radialProgressBar.StopSpinning();
            UIManager.instance.LoginScreen();
        }
        catch (FirebaseException ex)
        {
            AuthError errorCode = (AuthError)ex.ErrorCode;
            string message = GetErrorMessage(errorCode);
            feedbackManager.ShowFeedback(message);
            radialProgressBar.StopSpinning();

            // Log failure event

        }
    }

    private async Task ResetPasswordAsync(string email)
    {
        try
        {
            await auth.SendPasswordResetEmailAsync(email);
            Debug.Log("Password reset email sent to: " + email);
            feedbackManager.ShowFeedback("Password reset email sent to: " + email);

            // Log success event
            FirebaseAnalytics.LogEvent("password_reset_success",
                new Parameter("email", email),
                new Parameter("timestamp", DateTime.UtcNow.ToString("o")));
        }
        catch (FirebaseException ex)
        {
            Debug.LogWarning("Failed to send password reset email: " + ex.Message);
            AuthError errorCode = (AuthError)ex.ErrorCode;
            string message = GetErrorMessage(errorCode);
            feedbackManager.ShowFeedback(message);

            // Log failure event

        }
    }

    public void registerScreen()
    {
        UIManager.instance.RegisterScreen();
        ClearFields(new TMP_InputField[] { emailLoginField, passwordLoginField });
    }

    public void mainScreen()
    {
        UIManager.instance.MainScreen();
        ClearFields(new TMP_InputField[] { emailLoginField, passwordLoginField });
    }

    public void loginScreen()
    {
        UIManager.instance.LoginScreen();
        ClearFields(new TMP_InputField[] { usernameRegisterField, emailRegisterField, passwordRegisterField, passwordRegisterVerifyField });
    }

    public void Logout()
    {
        LogoutAsync().Forget();
    }

    private async Task LogoutAsync()
    {
        try
        {
            if (auth == null)
            {
                Debug.LogError("FirebaseAuth instance is null.");
                return;
            }

            if (feedbackManager == null)
            {
                Debug.LogError("FeedbackManager is not assigned.");
                return;
            }

            // Sign out from Firebase Authentication
            auth.SignOut();
            User = null; // Clear the current user
            Debug.Log("User logged out successfully.");

            // Provide feedback to the user
            feedbackManager.ShowFeedback("Logged Out");

            // Check if the SceneNames.MainMenu is defined and valid
            if (string.IsNullOrEmpty(SceneNames.UserProfile))
            {
                Debug.LogError("Scene name for MainMenu is not set.");
                return;
            }

            // Redirect to login or main screen
            SceneManager.LoadScene(SceneNames.UserProfile);

            // Additional cleanup if necessary
            ClearUserSpecificData();

            // Log logout event
            FirebaseAnalytics.LogEvent("logout_success", new Parameter("timestamp", DateTime.UtcNow.ToString("o")));
        }
        catch (Exception ex)
        {
            Debug.LogError("Error while logging out: " + ex.Message);
            feedbackManager.ShowFeedback("Logout failed. Please try again.");

            // Log logout failure event


        }
    }

    private void ClearUserSpecificData()
    {
        // Example: Clear user-related data or reset UI fields
        emailLoginField.text = "";
        passwordLoginField.text = "";
    }

    private void ClearFields(TMP_InputField[] fields)
    {
        foreach (var field in fields)
        {
            field.text = "";
        }
    }

    private string GetErrorMessage(AuthError errorCode)
    {
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                return "Missing Email";
            case AuthError.MissingPassword:
                return "Missing Password";
            case AuthError.WrongPassword:
                return "The email or password is wrong";
            case AuthError.InvalidEmail:
                return "Invalid Email";
            case AuthError.UserNotFound:
                return "Account does not exist";
            case AuthError.WeakPassword:
                return "Weak Password";
            case AuthError.EmailAlreadyInUse:
                return "Email Already In Use";
            default:
                return "Login Failed! Please try again.";
        }
    }
}

public static class SceneNames
{
    public const string MainMenu = "MainMenu";
    public const string UserProfile = "UserProfile"; // Adjusted to be consistent
}

public static class TaskExtensions
{
    public static void Forget(this Task task)
    {
        task.ContinueWith(t =>
        {
            Debug.LogException(t.Exception);
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
}
