using UnityEngine;

public class ProfileDataUI : MonoBehaviour
{
    public static ProfileDataUI instance; // Change UIManager to ProfileDataUI

    public GameObject ProfileDataUi;
    public GameObject mainMenuUi;
    public GameObject SearchingUi;
    public GameObject storeUI;
    public GameObject SpinWheelUI;
   

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void CleanScreen()
    {
        mainMenuUi.SetActive(false);
        ProfileDataUi.SetActive(false);
        SearchingUi.SetActive(false);
        storeUI.SetActive(false);
        SpinWheelUI.SetActive(false);
    }

    public void MainMenuScreen()
    {
        CleanScreen();
        mainMenuUi.SetActive(true);
    }

    public void ProfileScreen()
    {
        CleanScreen();
        ProfileDataUi.SetActive(true);
    }

    public void SearchingScreen()
    {
        CleanScreen();
        SearchingUi.SetActive(true);
    }
    public void StoreScreen()
    {
        CleanScreen();  
        storeUI.SetActive(true);
    }

   public void SpinWheelScreen()
    {
        CleanScreen();
        SpinWheelUI.SetActive(true);
    }
   
}
