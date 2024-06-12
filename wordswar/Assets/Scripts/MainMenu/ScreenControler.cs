using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenControler : MonoBehaviour
{
    public static ScreenControler instance;
    public GameObject SpinWheelUI;
    public GameObject PlayModeUI;
    public GameObject SearchingUi;
    public GameObject mainmenuUI;

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
        SearchingUi.SetActive(false);
        SpinWheelUI.SetActive(false);
        PlayModeUI.SetActive(false);
        mainmenuUI.SetActive(false);
    }


    public void SearchingScreen()
    {
        CleanScreen();
        SearchingUi.SetActive(true);
    }
    public void SpinWheelScreen()
    {
        CleanScreen();
        SpinWheelUI.SetActive(true);
    }
    public void PlayModeScreen()
    {
        CleanScreen();
        PlayModeUI.SetActive(true);
    }
    public void mainmenuScreen()
    {
        CleanScreen();
        mainmenuUI.SetActive(true);
    }
}
