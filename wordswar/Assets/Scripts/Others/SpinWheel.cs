using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Functions;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SpinWheel : MonoBehaviour
{
    [Header("Radial Bar")]
    [SerializeField] private RadialProgressBar radialProgressBar;

    [Header("Spin Settings")]
    [SerializeField] private Button spinButton; // Reference to the spin button
    [SerializeField] private float minSpinSpeed = 500f; // Minimum speed of the wheel in degrees per second
    [SerializeField] private float maxSpinSpeed = 2000f; // Maximum speed of the wheel in degrees per second
    [SerializeField] private float minSpinDuration = 10f; // Minimum duration for the wheel to spin
    [SerializeField] private float maxSpinDuration = 12f; // Maximum duration for the wheel to spin

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI WonPrizeText; // Text element to show the won prize
    [SerializeField] private Image WonPrizeImage; // Image element to show the won prize image
    [SerializeField] private GameObject PrizePanel;

    [Header("Prize sprites")]
    [SerializeField] private Sprite xpSprite;
    [SerializeField] private Sprite coins10Sprite;
    [SerializeField] private Sprite gems10Sprite;
    [SerializeField] private Sprite badLuckSprite;
    [SerializeField] private Sprite coins100Sprite;
    [SerializeField] private Sprite hintSprite;
    [SerializeField] private Sprite gems100Sprite;
    [SerializeField] private Sprite jokerSprite;

    [Header("Sound")]
    [SerializeField] private AudioSource spinSound; // Reference to the AudioSource for spin sound

    private Dictionary<string, Sprite> prizeSprites;

    private bool isSpinning = false;
    private float currentSpeed;
    private float spinTime;
    private float timeElapsed;

    private FirebaseAuth auth;
    private FirebaseFunctions functions;

    string segmentName;

    void Start()
    {
        // Initialize Firebase
        auth = FirebaseAuth.DefaultInstance;
        functions = FirebaseFunctions.DefaultInstance;

        // Assign the button onClick event
        spinButton.onClick.AddListener(StartSpin);

        // Initialize the prize sprite dictionary
        prizeSprites = new Dictionary<string, Sprite>
        {
            { "100xp", xpSprite },
            { "10 coins", coins10Sprite },
            { "10 gems", gems10Sprite },
            { "bad luck", badLuckSprite },
            { "100 coins", coins100Sprite },
            { "extra hint", hintSprite },
            { "100 gems", gems100Sprite },
            { "joker", jokerSprite }
        };
    }

    void Update()
    {
        if (isSpinning)
        {
            spinButton.interactable = false;
            // Rotate the wheel
            float step = currentSpeed * Time.deltaTime;
            transform.Rotate(0, 0, -step); // Rotate counterclockwise

            // Increase the elapsed time
            timeElapsed += Time.deltaTime;

            // Apply friction or slowing effect
            float t = timeElapsed / spinTime;
            currentSpeed = Mathf.Lerp(currentSpeed, 0, t * t); // Smooth quadratic deceleration

            // Check if the spinning should stop
            if (timeElapsed >= spinTime || currentSpeed < 0.1f)
            {
                isSpinning = false;
                DetermineWinningSegment();
                spinButton.interactable = true;
                spinSound.Stop(); // Stop the spin sound
            }
        }
    }

    public void StartSpin()
    {
        if (auth.CurrentUser != null)
        {
            if (isSpinning) return; // Prevent multiple spins at once

            isSpinning = true;
            timeElapsed = 0;

            // Randomize the spin speed and duration
            currentSpeed = Random.Range(minSpinSpeed, maxSpinSpeed);
            spinTime = Random.Range(minSpinDuration, maxSpinDuration);

            spinSound.Play(); // Play the spin sound
        }
        else
        {
            Debug.Log("You have to be authenticated");
        }
    }

    void DetermineWinningSegment()
    {
        // Determine the angle where the wheel stopped
        float stoppedAngle = transform.eulerAngles.z;
        float adjustedAngle = (360 - stoppedAngle + 90) % 360; // Adjust for arrow position at 0 degrees

        // Calculate the segment based on the adjusted angle
        int numberOfSegments = 8; // Number of segments
        float segmentAngle = 360f / numberOfSegments;

        int selectedSegment = Mathf.FloorToInt(adjustedAngle / segmentAngle);

        // Translate segment index to readable format
        segmentName = GetSegmentName(selectedSegment);

        // Log the result locally
        Debug.Log("Wheel stopped at: " + segmentName);

        // Update the UI to show the prize

        // Send the result to the server for validation and reward assignment
        StartCoroutine(SendSpinResultToServer(segmentName));
    }

    string GetSegmentName(int segmentIndex)
    {
        switch (segmentIndex)
        {
            case 0: return "100xp";
            case 1: return "10 coins";
            case 2: return "10 gems";
            case 3: return "bad luck";
            case 4: return "100 coins";
            case 5: return "extra hint";
            case 6: return "100 gems";
            case 7: return "joker";
            default: return "Unknown";
        }
    }

    void DisplayWonPrize(string prizeName)
    {
        PrizePanel.SetActive(true);
        // Update the text to show the won prize
        WonPrizeText.text = $"Congratulations! You won {prizeName}";

        // Update the image to show the won prize sprite
        if (prizeSprites.ContainsKey(prizeName))
        {
            WonPrizeImage.sprite = prizeSprites[prizeName];
        }
        else
        {
            Debug.LogWarning($"Prize image for '{prizeName}' not found!");
        }
    }

    IEnumerator SendSpinResultToServer(string reward)
    {
        radialProgressBar.StartSpinning();
        var function = functions.GetHttpsCallable("processSpinResult");
        var data = new Dictionary<string, object>
        {
            { "reward", reward }
        };

        var task = function.CallAsync(data);

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("Error sending spin result to server: " + task.Exception);
            radialProgressBar.StopSpinning();
        }
        else
        {
            radialProgressBar.StopSpinning();
            Debug.Log("Successfully sent spin result to server. Response: " + task.Result.Data);
            // Optionally, you can handle the server's response here
            // For example, update the player's UI or show a confirmation message
            Debug.Log("the segment name is " + segmentName);
            DisplayWonPrize(segmentName);
        }
    }
}
