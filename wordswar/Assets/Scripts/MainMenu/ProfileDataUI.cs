using UnityEngine;
using System.Collections;

public class ProfileDataUI : MonoBehaviour
{
    public static ProfileDataUI instance;

    public GameObject ProfileDataUi;
    public GameObject mainMenuUi;
    public GameObject SearchingUi;
    public GameObject storeUI;
    public GameObject SpinWheelUI;
    public GameObject PlayModeUI;

    public float transitionDuration = 0.2f; // Duration for the slide transitions

    private GameObject currentActiveScreen; // Track the current active screen

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            currentActiveScreen = mainMenuUi; // Set initial screen
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void CleanScreen()
    {
        // Slide out the current active screen to the left if there is any
        if (currentActiveScreen != null) StartCoroutine(SlideOut(currentActiveScreen, "left"));
    }

    public void MainMenuScreen()
    {
        // Check if the current screen is already the main menu
        if (currentActiveScreen == mainMenuUi) return;

        CleanScreen();
        StartCoroutine(SlideIn(mainMenuUi, "right"));
        currentActiveScreen = mainMenuUi; // Update the current active screen
    }

    public void ProfileScreen()
    {
        // Check if the current screen is already the profile screen
        if (currentActiveScreen == ProfileDataUi) return;

        CleanScreen();
        StartCoroutine(SlideIn(ProfileDataUi, "right"));
        currentActiveScreen = ProfileDataUi; // Update the current active screen
    }

   

    public void StoreScreen()
    {
        // Check if the current screen is already the store screen
        if (currentActiveScreen == storeUI) return;

        CleanScreen();
        StartCoroutine(SlideIn(storeUI, "right"));
        currentActiveScreen = storeUI; // Update the current active screen
    }

   

   

    private IEnumerator SlideIn(GameObject uiElement, string direction)
    {
        RectTransform rectTransform = uiElement.GetComponent<RectTransform>();
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = Vector2.zero; // Center of the screen

        // Determine the starting position based on the direction
        switch (direction)
        {
            case "left":
                startPos = new Vector2(-Screen.width, rectTransform.anchoredPosition.y);
                break;
            case "right":
                startPos = new Vector2(Screen.width, rectTransform.anchoredPosition.y);
                break;
            case "up":
                startPos = new Vector2(rectTransform.anchoredPosition.x, Screen.height);
                break;
            case "down":
                startPos = new Vector2(rectTransform.anchoredPosition.x, -Screen.height);
                break;
        }

        rectTransform.anchoredPosition = startPos;
        uiElement.SetActive(true);

        float elapsedTime = 0;
        while (elapsedTime < transitionDuration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsedTime / transitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        rectTransform.anchoredPosition = endPos;
    }

    private IEnumerator SlideOut(GameObject uiElement, string direction)
    {
        RectTransform rectTransform = uiElement.GetComponent<RectTransform>();
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = startPos;

        // Determine the ending position based on the direction
        switch (direction)
        {
            case "left":
                endPos = new Vector2(-Screen.width, startPos.y);
                break;
            case "right":
                endPos = new Vector2(Screen.width, startPos.y);
                break;
            case "up":
                endPos = new Vector2(startPos.x, Screen.height);
                break;
            case "down":
                endPos = new Vector2(startPos.x, -Screen.height);
                break;
        }

        float elapsedTime = 0;
        while (elapsedTime < transitionDuration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsedTime / transitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        rectTransform.anchoredPosition = endPos;
        uiElement.SetActive(false);
    }


   
}
