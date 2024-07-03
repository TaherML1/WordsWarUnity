using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowPanel : MonoBehaviour
{
    public GameObject shadowPanel;


    public void showShadowPanel()
    {
        shadowPanel.SetActive(true);
    }
    public void closeShadowPanel()
    {
        shadowPanel.SetActive(false);
    }
    
}
