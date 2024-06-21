using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    public Animator[] buttonAnimators; // Assign the Animator components of the buttons in the inspector

    // Initialize the buttons' states
    private void Start()
    {
        // Assuming buttons are initially down, set IsUp to false for all
        foreach (Animator animator in buttonAnimators)
        {
            animator.SetBool("IsUp", false); // Initially, all buttons are down
        }
    }

    // Method called when a button is clicked
    public void OnButtonClick(int clickedButtonIndex)
    {
        for (int i = 0; i < buttonAnimators.Length; i++)
        {
            if (i == clickedButtonIndex)
            {
                buttonAnimators[i].SetBool("IsUp", true);     // Set the clicked button's IsUp to true
                buttonAnimators[i].SetTrigger("MoveUp");   // Trigger the up animation
            }
            else
            {
                buttonAnimators[i].SetBool("IsUp", false);    // Set the other buttons' IsUp to false
                buttonAnimators[i].SetTrigger("MoveDown"); // Trigger the down animation for other buttons
            }
        }
    }
}
