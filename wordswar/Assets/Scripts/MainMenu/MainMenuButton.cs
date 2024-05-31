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

        // Load the main menu scene
        SceneManager.LoadScene("MainMenu");
    }
}
