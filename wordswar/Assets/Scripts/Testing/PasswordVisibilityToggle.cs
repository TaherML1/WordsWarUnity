using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PasswordVisibilityToggle : MonoBehaviour
{
    public TMP_InputField passwordInputField;
    public Button toggleVisibilityButton;
    public TextMeshProUGUI visibilityText;

    private bool isPasswordVisible = false;

    void Start()
    {
        toggleVisibilityButton.onClick.AddListener(TogglePasswordVisibility);
        UpdateVisibilityText();
    }

    void TogglePasswordVisibility()
    {
        isPasswordVisible = !isPasswordVisible;
        passwordInputField.contentType = isPasswordVisible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        passwordInputField.ForceLabelUpdate();
        UpdateVisibilityText();
    }

    void UpdateVisibilityText()
    {
        visibilityText.text = isPasswordVisible ? "Hide" : "Show";
    }
}
