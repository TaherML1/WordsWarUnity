using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class CustomKeyboard : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;
   
 
    [SerializeField]AudioSource audioSource;

    private void Start()
    {
        inputField.shouldHideMobileInput = true;
      
        
    }

    private void PlayKeypressSound()
    {
        audioSource.Play();
    }

    public void ClearLastCharacter()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
            PlayKeypressSound();
        }
    }

    public void Space()
    {
        inputField.text += " ";
        PlayKeypressSound();
    }

    public void ClearInput()
    {
        inputField.text = string.Empty;
        PlayKeypressSound();
    }

    public void SubmitWord()
    {
        string word = inputField.text;
        Debug.Log("Submitted word: " + word);
        // Add logic to submit the word to your game
        PlayKeypressSound();
    }

    private void AddCharacter(char character)
    {
        inputField.text += character.ToString();
        PlayKeypressSound();
    }

    public void AddAlif() => AddCharacter('\u0627');
    public void AddBaa() => AddCharacter('\u0628');
    public void AddTaa() => AddCharacter('\u062A');
    public void AddThaa() => AddCharacter('\u062B');
    public void AddJeem() => AddCharacter('\u062C');
    public void AddHaa() => AddCharacter('\u062D');
    public void AddKhaa() => AddCharacter('\u062E');
    public void AddDal() => AddCharacter('\u062F');
    public void AddThal() => AddCharacter('\u0630');
    public void AddRa() => AddCharacter('\u0631');
    public void AddZa() => AddCharacter('\u0632');
    public void AddSeen() => AddCharacter('\u0633');
    public void AddSheen() => AddCharacter('\u0634');
    public void AddSaad() => AddCharacter('\u0635');
    public void AddDaad() => AddCharacter('\u0636');
    public void AddTaa2() => AddCharacter('\u0637');
    public void AddZaa() => AddCharacter('\u0638');
    public void AddAin() => AddCharacter('\u0639');
    public void AddGhain() => AddCharacter('\u063A');
    public void AddFa() => AddCharacter('\u0641');
    public void AddQaf() => AddCharacter('\u0642');
    public void AddKaf() => AddCharacter('\u0643');
    public void AddLam() => AddCharacter('\u0644');
    public void AddMeem() => AddCharacter('\u0645');
    public void AddNoon() => AddCharacter('\u0646');
    public void AddHaa2() => AddCharacter('\u0647');
    public void AddWaw() => AddCharacter('\u0648');
    public void AddYaa() => AddCharacter('\u064A');
    public void AddMaksurahamza() => AddCharacter('\u0626');
    public void tamarbota() => AddCharacter('\u0629');
    public void hamza() => AddCharacter('\u0621');
    public void addMaksura() => AddCharacter('\u0649');
    public void addwawhamza() => AddCharacter('\u0624');
}
