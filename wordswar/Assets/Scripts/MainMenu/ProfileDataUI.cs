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
    public float swipeThreshold = 50f; // Minimum swipe distance to recognize as a swipe

    private GameObject currentActiveScreen; // Track the current active screen
    private Vector2 touchStartPos; // To store the start position of the touch
    private Vector2 touchEndPos; // To store the end position of the touch

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

    private void Update()
    {
        DetectSwipe();
    }

    public void CleanScreen()
    {
        // Slide out the current active screen to the left if there is any
        if (currentActiveScreen != null) StartCoroutine(SlideOut(currentActiveScreen, "left"));
    }

    private void DeleteScreen()
    {
        SpinWheelUI.SetActive(false);
        PlayModeUI.SetActive(false);
    }

    public void MainMenuScreen()
    {
        // Check if the current screen is already the main menu
        if (currentActiveScreen == mainMenuUi) return;
        DeleteScreen();
        CleanScreen();
        StartCoroutine(SlideIn(mainMenuUi, "right"));
        currentActiveScreen = mainMenuUi; // Update the current active screen
    }

    public void ProfileScreen()
    {
        // Check if the current screen is already the profile screen
        if (currentActiveScreen == ProfileDataUi) return;
        DeleteScreen();
        CleanScreen();
        StartCoroutine(SlideIn(ProfileDataUi, "right"));
        currentActiveScreen = ProfileDataUi; // Update the current active screen
    }

    public void StoreScreen()
    {
        // Check if the current screen is already the store screen
        if (currentActiveScreen == storeUI) return;
        DeleteScreen();
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

    private void DetectSwipe()
    {
        // Handle touch input for mobile
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = touch.position;
                    break;

                case TouchPhase.Ended:
                    touchEndPos = touch.position;
                    HandleSwipe();
                    break;
            }
        }
    }

    private void HandleSwipe()
    {
        float horizontalSwipeDistance = touchEndPos.x - touchStartPos.x;
        float verticalSwipeDistance = touchEndPos.y - touchStartPos.y;

        // Check if the swipe distance meets the threshold
        if (Mathf.Abs(horizontalSwipeDistance) > swipeThreshold && Mathf.Abs(horizontalSwipeDistance) > Mathf.Abs(verticalSwipeDistance))
        {
            if (horizontalSwipeDistance > 0)
            {
                // Right swipe
                OnSwipeRight();
            }
            else
            {
                // Left swipe
                OnSwipeLeft();
            }
        }
    }

    private void OnSwipeRight()
    {
        // Transition to the next screen on right swipe
        if (currentActiveScreen == mainMenuUi)
        {
            ProfileScreen(); // Main menu to Profile screen
        }
        else if (currentActiveScreen == ProfileDataUi)
        {
            StoreScreen(); // Profile screen to Store screen
        }
        // Add more cases if you have more screens to navigate through
    }

    private void OnSwipeLeft()
    {
        // Transition to the previous screen on left swipe
        if (currentActiveScreen == storeUI)
        {
            ProfileScreen(); // Store screen to Profile screen
        }
        else if (currentActiveScreen == ProfileDataUi)
        {
            MainMenuScreen(); // Profile screen to Main menu
        }
        // Add more cases if you have more screens to navigate through
    }
}
