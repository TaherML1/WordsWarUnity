using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SpinWheel : MonoBehaviour
{
    public GameObject wheel;             // Reference to the wheel GameObject
    public float spinDuration = 3f;      // Duration of the spin
    public Button spinButton;            // Reference to the spin button

    private bool isSpinning = false;
    private Dictionary<string, float> rewards = new Dictionary<string, float>();
    private List<string> rewardList = new List<string>();
    private List<float> cumulativeProbabilities = new List<float>();
    float endAngle = 0;

    void Start()
    {
        // Initialize rewards with probabilities
        rewards.Add("10 Gold", 0.5f);   // Red
        rewards.Add("50 Gold", 0.3f);   // Blue
        rewards.Add("100 Gold", 0.15f); // Green
        rewards.Add("Gems", 0.05f);     // Magenta

        // Prepare reward list and cumulative probabilities for random selection
        CalculateCumulativeProbabilities();

        // Add listener to the button
        spinButton.onClick.AddListener(Spin);
    }

    void CalculateCumulativeProbabilities()
    {
        float cumulative = 0f;
        foreach (var reward in rewards)
        {
            cumulative += reward.Value;
            cumulativeProbabilities.Add(cumulative);
            rewardList.Add(reward.Key);
        }

        // Debug logs for cumulative probabilities
        Debug.Log("Cumulative Probabilities: ");
        for (int i = 0; i < cumulativeProbabilities.Count; i++)
        {
            Debug.Log($"Reward: {rewardList[i]}, Cumulative Probability: {cumulativeProbabilities[i]}");
        }
    }

    public void Spin()
    {
        if (!isSpinning)
        {
            StartCoroutine(SpinTheWheel());
        }
    }

    private IEnumerator SpinTheWheel()
    {
        isSpinning = true;
        float elapsedTime = 0f;
        float startAngle = wheel.transform.eulerAngles.z;
         endAngle = startAngle + Random.Range(720f, 1440f); // Randomly choosing spin amount

        while (elapsedTime < spinDuration)
        {
            float currentAngle = Mathf.Lerp(startAngle, endAngle, elapsedTime / spinDuration);
            wheel.transform.eulerAngles = new Vector3(0, 0, currentAngle);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensuring the wheel ends at the final angle
        wheel.transform.eulerAngles = new Vector3(0, 0, endAngle);

        // Determine reward
        string reward = DetermineReward();
        Debug.Log("You won: " + reward);
        // Implement reward granting logic here

        isSpinning = false;
    }

    private string DetermineReward()
    {
        // Generate a random number between 0 and 1
        float randomValue = Random.Range(0f, 1f);
        Debug.Log("Random Value: " + randomValue);

        // Determine which reward is won based on cumulative probabilities
        for (int i = 0; i < cumulativeProbabilities.Count; i++)
        {
            if (randomValue <= cumulativeProbabilities[i])
            {
                Debug.Log($"Random Value {randomValue} is less than or equal to Cumulative Probability {cumulativeProbabilities[i]}");
                Debug.Log("Selected Reward: " + rewardList[i]);
                ShowSelectedSegment(endAngle);
                return rewardList[i];
            }
            else
            {
                Debug.Log($"Random Value {randomValue} is greater than Cumulative Probability {cumulativeProbabilities[i]}");
            }
        }

        // Fallback, should never reach here if probabilities are set correctly
        Debug.Log("Fallback to default reward");
        ShowSelectedSegment(endAngle);
        return rewardList[0];
    }

    private void ShowSelectedSegment(float endAngle)
    {
        // Normalize the angle to a [0, 360) range and convert to clockwise
        float normalizedAngle = endAngle % 360f;
        normalizedAngle = 360f - normalizedAngle;
        Debug.Log("Normalized Angle (Clockwise): " + normalizedAngle);

        // Determine the segment based on the normalized angle
        if (normalizedAngle >= 0 && normalizedAngle < 90)
        {
            Debug.Log("Pointer lands on Magenta (0 to 90 degrees): Gems");
        }
        else if (normalizedAngle >= 90 && normalizedAngle < 180)
        {
            Debug.Log("Pointer lands on Green (90 to 180 degrees): 100 Gold");
        }
        else if (normalizedAngle >= 180 && normalizedAngle < 270)
        {
            Debug.Log("Pointer lands on Blue (180 to 270 degrees): 50 Gold");
        }
        else if (normalizedAngle >= 270 && normalizedAngle < 360)
        {
            Debug.Log("Pointer lands on Red (270 to 360 degrees): 10 Gold");
        }
    }
}
