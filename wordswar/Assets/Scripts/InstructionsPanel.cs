using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstructionsManager : MonoBehaviour
{
    public List<Sprite> images; // To store image sprites
    private int currentImageIndex = 0; // To track the currently displayed image

    public GameObject imageObject; // Reference to the image game object
    public Button backButton; // Reference to the back button
    public Button nextButton; // Reference to the next button
    void Start()
    {
        // Load images into the list
        foreach (Sprite sprite in Resources.LoadAll<Sprite>("Path/To/Your/Images"))
        {
            images.Add(sprite);
        }

        // Set the initial image
        imageObject.GetComponent<Image>().sprite = images[currentImageIndex];

        // Add button click listeners
        backButton.onClick.AddListener(OnBackButton);
        nextButton.onClick.AddListener(OnNextButton);

        // Disable buttons based on initial state
        backButton.interactable = false; // If at the first image
        nextButton.interactable = images.Count > 1; // If there are more images
    }

    void OnBackButton()
    {
        if (currentImageIndex > 0)
        {
            currentImageIndex--;
            imageObject.GetComponent<Image>().sprite = images[currentImageIndex];
            nextButton.interactable = true; // Enable next button if not at the end
        }
        backButton.interactable = currentImageIndex > 0; // Disable back button if at the first image
    }

    void OnNextButton()
    {
        currentImageIndex++;

        // If the current index exceeds the number of images, loop back to the first image
        if (currentImageIndex >= images.Count)
        {
            currentImageIndex = 0;
        }

        imageObject.GetComponent<Image>().sprite = images[currentImageIndex];

        // Enable back button if not at the first image
        backButton.interactable = currentImageIndex != 0;
    }


}
