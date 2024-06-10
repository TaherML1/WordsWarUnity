using UnityEngine;
using UnityEngine.UI;

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

    void Start()
    {
        // Assign the button onClick event
        spinButton.onClick.AddListener(StartSpin);
    }

    void Update()
    {
        if (isSpinning)
        {
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
        string segmentName = "Unknown";
        switch (selectedSegment)
        {
            case 0:
                segmentName = "100xp";
                break;
            case 1:
                segmentName = "10 coins";
                break;
            case 2:
                segmentName = "10 gems";
                break;
            case 3:
                segmentName = "bad luck";
                break;
            case 4:
                segmentName = "100 coins";
                break;
            case 5:
                segmentName = "extra time";
                break;
            case 6:
                segmentName = "100 gems";
                break;
            case 7:
                segmentName = "joker";
                break;
        }

        Debug.Log("Wheel stopped at: " + segmentName);
    }
}
