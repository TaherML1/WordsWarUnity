using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Firebase;
using Firebase.Auth;
using Firebase.Functions;
using System.Collections;
using System.Collections.Generic;

public class SpinWheel : MonoBehaviour
{
    public Button spinButton; // Reference to the spin button
    public float minSpinSpeed = 500f; // Minimum speed of the wheel in degrees per second
    public float maxSpinSpeed = 2000f; // Maximum speed of the wheel in degrees per second
    public float minSpinDuration = 10f; // Minimum duration for the wheel to spin
    public float maxSpinDuration = 12f; // Maximum duration for the wheel to spin
    private bool isSpinning = false;
    private float currentSpeed;
    private float spinTime;
    private float timeElapsed;

    private FirebaseAuth auth;
    private FirebaseFunctions functions;

    void Start()
    {
        // Initialize Firebase
        auth = FirebaseAuth.DefaultInstance;
        functions = FirebaseFunctions.DefaultInstance;

        // Assign the button onClick event
        spinButton.onClick.AddListener(StartSpin);
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
            }
        }
    }

    public void StartSpin()
    {
        if (isSpinning) return; // Prevent multiple spins at once

        isSpinning = true;
        timeElapsed = 0;

        // Randomize the spin speed and duration
        currentSpeed = Random.Range(minSpinSpeed, maxSpinSpeed);
        spinTime = Random.Range(minSpinDuration, maxSpinDuration);
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
        string segmentName = GetSegmentName(selectedSegment);

        // Log the result locally
        Debug.Log("Wheel stopped at: " + segmentName);

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

    IEnumerator SendSpinResultToServer(string reward)
    {
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
        }
        else
        {
            Debug.Log("Successfully sent spin result to server. Response: " + task.Result.Data);
            // Optionally, you can handle the server's response here
            // For example, update the player's UI or show a confirmation message
        }
    }
}
