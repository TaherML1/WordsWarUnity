using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using ArabicSupport;
using System;
using System.Collections;






public class GameOverController : MonoBehaviour
{
    public GameObject gameOverPanel;
    public TextMeshProUGUI winnerText;

    public void ShowGameOverWinner()
    {
        winnerText.text ="انت الفائز";
       

        // Activate the game over panel to make it visible
        gameOverPanel.SetActive(true);
    }
    public void ShowGameOverLooser()
    {
        // Update the winner text with the winner's name and score
        winnerText.text = "انت الخاسر";

        // Activate the game over panel to make it visible
        gameOverPanel.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        // Load the main menu scene
        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator DelayedGameOver(bool isLocalPlayerWinner)
    {
        // Wait for 1.5 seconds
        yield return new WaitForSeconds(7.5f);

        // Check if the local player is the winner or not and show the appropriate game over panel
        if (isLocalPlayerWinner)
        {
           ShowGameOverWinner();
        }
        else
        {
          ShowGameOverLooser();
        }
    }
   public void ShowDelayedGameOver(bool isLocalPlayerWinner)
    {
        StartCoroutine(DelayedGameOver(isLocalPlayerWinner));
    }
}
