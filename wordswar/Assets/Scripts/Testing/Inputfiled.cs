using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class Inputfiled : MonoBehaviour
{
    public TMP_InputField playerInput;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    public void OnSubmitButtonClick()
    {
        string userInput = playerInput.text;
        Debug.Log("Button clicked, input is: " + userInput);

        // Do something with the user input here, e.g., save it, process it, etc.
    }
}
