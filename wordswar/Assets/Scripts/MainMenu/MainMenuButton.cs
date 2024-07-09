using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
public class MainMenuButton : MonoBehaviour
{
    // Function to handle button click event
    
    public void GoToGamePlay()
    {
        SceneManager.LoadScene("GamePlay");
    }
   public void GoToMainMenu() 
    {

        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadMainMenu();
        }
        else
        {
            Debug.LogError("SceneController instance not found. Cannot load MainMenu.");
        }
    }
}
