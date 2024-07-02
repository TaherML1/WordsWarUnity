using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumericInput : MonoBehaviour
{
    public TMP_InputField inputField;

    void Start()
    {
        inputField.onValueChanged.AddListener(ValidateInput);
    }

    void ValidateInput(string input)
    {
        // Allow only numeric characters
        string newText = string.Empty;
        foreach (char c in input)
        {
            if (char.IsDigit(c))
            {
                newText += c;
            }
        }

        // Update the input field's text without triggering the listener again
        inputField.text = newText;
    }
}
