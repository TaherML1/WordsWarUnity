using UnityEngine;
using UnityEngine.UI;

public class OverlayManager : MonoBehaviour
{
    private void Start()
    {
        // Get the Button component attached to the overlay panel
        Button overlayButton = GetComponent<Button>();

        // Add a listener to the button's onClick event to hide the panel when clicked
        overlayButton.onClick.AddListener(HideOverlay);
    }

    // Method to hide the overlay
    public void HideOverlay()
    {
        gameObject.SetActive(false);
    }

    // Method to show the overlay
    public void ShowOverlay()
    {
        gameObject.SetActive(true);
    }
}
