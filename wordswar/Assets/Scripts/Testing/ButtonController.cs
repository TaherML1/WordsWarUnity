using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    // Reference to the Animator component
    private Animator animator;

    void Start()
    {
        // Get the Animator component attached to this GameObject
        animator = GetComponent<Animator>();

        // Check if the Animator component is attached
        if (animator == null)
        {
            Debug.LogError("No Animator component found on this GameObject");
        }
    }

    // Function to set the MoveUp trigger
    public void MoveUp()
    {
        if (animator != null)
        {
            animator.SetTrigger("MoveUp");
        }
    }

    // Function to set the MoveDown trigger
    public void MoveDown()
    {
        if (animator != null)
        {
            animator.SetTrigger("MoveDown");
        }
    }
}
