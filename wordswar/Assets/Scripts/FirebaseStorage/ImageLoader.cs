using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Firebase;
using Firebase.Extensions;
using Firebase.Storage;
using Firebase.Firestore;
using Firebase.Auth;

public class ImageLoader : MonoBehaviour
{
    public GameObject imageContainer; // Container for RawImage components
    public GameObject imagePrefab; // Prefab for RawImage
    public List<string> imageNames; // List of image names to load from Firebase Storage
    public RawImage profileImage; // Profile image to be updated

    FirebaseStorage storage;
    StorageReference storageReference;
    FirebaseFirestore db;

    string currentUserID = "userID"; // Replace with actual user ID or dynamically get it

    void Start()
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
        // Initialize Firebase components
        storage = FirebaseStorage.DefaultInstance;
        storageReference = storage.GetReferenceFromUrl("gs://words-war-8d86e.appspot.com/avatars");
        db = FirebaseFirestore.DefaultInstance;

        // Get the current user ID dynamically
        currentUserID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        // Fetch the profile image
        FetchProfileImage();
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

    public void LoadAvatarContainer()
    {
        // Load and display each image
        foreach (string imageName in imageNames)
        {
            LoadAndDisplayImage(imageName);
        }

        // Adjust the container size to fit all images
        AdjustContainerSize();
    }

    void LoadAndDisplayImage(string imageName)
    {
        // Get a reference to the image in Firebase Storage
        StorageReference imageRef = storageReference.Child(imageName);

        // Fetch the download URL of the image
        imageRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Failed to get download URL for {imageName}: {task.Exception}");
                return;
            }

            Uri downloadUri = task.Result;
            Debug.Log($"Download URL for {imageName}: {downloadUri}");

            // Create a new RawImage UI element
            GameObject newImage = Instantiate(imagePrefab, imageContainer.transform);
            RawImage rawImage = newImage.GetComponent<RawImage>();

            // Add a button component if not already there
            if (newImage.GetComponent<Button>() == null)
            {
                newImage.AddComponent<Button>();
            }

            // Assign the image URL to the button's click event
            Button button = newImage.GetComponent<Button>();
            button.onClick.AddListener(() => OnImageClicked(downloadUri.ToString()));

            // Start coroutine to load the image from the URL
            StartCoroutine(LoadImage(downloadUri.ToString(), rawImage));
        });
    }

    IEnumerator LoadImage(string imageUrl, RawImage rawImage)
    {
        Debug.Log($"Loading image from URL: {imageUrl}");

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error loading image from URL: {imageUrl}. Error: {request.error}");
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            rawImage.texture = texture;
        }
    }

    void OnImageClicked(string imageUrl)
    {
        Debug.Log($"Image clicked: {imageUrl}");

        // Start a coroutine to load the image and set it as the profile image
        StartCoroutine(LoadImage(imageUrl, profileImage));

        // Save selected image URL to Firestore
        SaveProfileImageURL(imageUrl);
    }

    void SaveProfileImageURL(string imageUrl)
    {
        // Reference to the current user's document in Firestore
        DocumentReference userRef = db.Collection("users").Document(currentUserID);

        // Update the profileImageURL field in the document
        userRef.UpdateAsync("profileImageURL", imageUrl).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Failed to update profile image URL: {task.Exception}");
            }
            else
            {
                Debug.Log("Profile image URL updated successfully");
            }
        });
    }

    void FetchProfileImage()
    {
        // Retrieve the profile image URL from Firestore
        DocumentReference userRef = db.Collection("users").Document(currentUserID);
        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Failed to fetch user document: {task.Exception}");
                return;
            }

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                string profileImageURL = snapshot.GetValue<string>("profileImageURL");
                if (!string.IsNullOrEmpty(profileImageURL))
                {
                    // Load the profile image using the URL
                    StartCoroutine(LoadImage(profileImageURL, profileImage));
                }
                else
                {
                    Debug.LogWarning("Profile image URL is null or empty");
                }
            }
            else
            {
                Debug.LogWarning("User document does not exist");
            }
        });
    }

    void AdjustContainerSize()
    {
        // Get the RectTransform of the container
        RectTransform containerRect = imageContainer.GetComponent<RectTransform>();

        // Get the RectTransform of the prefab to determine its size
        RectTransform prefabRect = imagePrefab.GetComponent<RectTransform>();

        // Calculate the total width based on the number of prefabs
        int numberOfImages = imageNames.Count;

        // Assuming horizontal layout and uniform spacing
        float spacing = 10f; // Adjust this based on your layout settings
        float totalWidth = (prefabRect.rect.width + spacing) * numberOfImages - spacing; // Total width including spacing between prefabs

        // Update the container's size
        containerRect.sizeDelta = new Vector2(totalWidth, containerRect.sizeDelta.y);

        // Optionally, you can set the position to start at the left-most point
        containerRect.anchoredPosition = new Vector2(0, containerRect.anchoredPosition.y);
    }
}
