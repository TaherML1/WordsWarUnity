using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("GamePlay"); // Load the gameplay scene
    }

    public void ViewInstructions()
    {
        SceneManager.LoadScene("instructions"); // Load the instructions scene
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit(); // Quit the application
    }
    public void store()
    {
        SceneManager.LoadScene("store");
    }
    public void mainMenu()
    {
        SceneManager.LoadScene("mainmenu");
    }

    public void keyboard()
    {
        SceneManager.LoadScene("keyboard");
    }

}
