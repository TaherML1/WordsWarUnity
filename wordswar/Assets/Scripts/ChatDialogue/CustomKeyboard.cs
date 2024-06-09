using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class CustomKeyboard : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI winnerText;
    public Button submitButton;

    private void Start()
    {
        inputField.shouldHideMobileInput = true;

    }

    public void ClearLastCharacter()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
        }
    }
    public void Space()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            inputField.text = AddSpaceBetweenWords(inputField.text);
        }
    }

    private string AddSpaceBetweenWords(string input)
    {
        // Check if the input string is null or empty
        if (string.IsNullOrEmpty(input))
        {
            Debug.LogWarning("Input string is null or empty.");
            return input;
        }

        // Split the input string by spaces
        string[] words = input.Split(' ');

        // Join the words with a space between them
        string result = string.Join(" ", words);

        return result;
    }





    // Function to clear the whole input field
    public void ClearInput()
    {
        inputField.text = string.Empty;
    }

    // Function to submit the word
    public void SubmitWord()
    {
        string word = inputField.text;
        Debug.Log("Submitted word: " + word);
        // Add logic to submit the word to your game
    }


    // Function to add the Arabic letter "ا" to the input field
    public void AddAlif()
    {
        char alif = '\u0627'; // Unicode value for "ا"
        inputField.text += alif.ToString();
    }

    // Function to add the Arabic letter "ب" to the input field
    public void AddBaa()
    {
        char baa = '\u0628'; // Unicode value for "ب"
        inputField.text += baa.ToString();
    }

    // Function to add the Arabic letter "ت" to the input field
    public void AddTaa()
    {
        char taa = '\u062A'; // Unicode value for "ت"
        inputField.text += taa.ToString();
    }

    // Function to add the Arabic letter "ث" to the input field
    public void AddThaa()
    {
        char thaa = '\u062B'; // Unicode value for "ث"
        inputField.text += thaa.ToString();
    }

    // Function to add the Arabic letter "ج" to the input field
    public void AddJeem()
    {
        char jeem = '\u062C'; // Unicode value for "ج"
        inputField.text += jeem.ToString();
    }

    // Function to add the Arabic letter "ح" to the input field
    public void AddHaa()
    {
        char haa = '\u062D'; // Unicode value for "ح"
        inputField.text += haa.ToString();
    }

    // Function to add the Arabic letter "خ" to the input field
    public void AddKhaa()
    {
        char khaa = '\u062E'; // Unicode value for "خ"
        inputField.text += khaa.ToString();
    }

    // Function to add the Arabic letter "د" to the input field
    public void AddDal()
    {
        char dal = '\u062F'; // Unicode value for "د"
        inputField.text += dal.ToString();
    }

    // Function to add the Arabic letter "ذ" to the input field
    public void AddThal()
    {
        char thal = '\u0630'; // Unicode value for "ذ"
        inputField.text += thal.ToString();
    }

    // Function to add the Arabic letter "ر" to the input field
    public void AddRa()
    {
        char ra = '\u0631'; // Unicode value for "ر"
        inputField.text += ra.ToString();
    }

    // Function to add the Arabic letter "ز" to the input field
    public void AddZa()
    {
        char za = '\u0632'; // Unicode value for "ز"
        inputField.text += za.ToString();
    }

    // Function to add the Arabic letter "س" to the input field
    public void AddSeen()
    {
        char seen = '\u0633'; // Unicode value for "س"
        inputField.text += seen.ToString();
    }

    // Function to add the Arabic letter "ش" to the input field
    public void AddSheen()
    {
        char sheen = '\u0634'; // Unicode value for "ش"
        inputField.text += sheen.ToString();
    }

    // Function to add the Arabic letter "ص" to the input field
    public void AddSaad()
    {
        char saad = '\u0635'; // Unicode value for "ص"
        inputField.text += saad.ToString();
    }

    // Function to add the Arabic letter "ض" to the input field
    public void AddDaad()
    {
        char daad = '\u0636'; // Unicode value for "ض"
        inputField.text += daad.ToString();
    }

    // Function to add the Arabic letter "ط" to the input field
    public void AddTaa2()
    {
        char taa2 = '\u0637'; // Unicode value for "ط"
        inputField.text += taa2.ToString();
    }

    // Function to add the Arabic letter "ظ" to the input field
    public void AddZaa()
    {
        char zaa = '\u0638'; // Unicode value for "ظ"
        inputField.text += zaa.ToString();
    }

    // Function to add the Arabic letter "ع" to the input field
    public void AddAin()
    {
        char ain = '\u0639'; // Unicode value for "ع"
        inputField.text += ain.ToString();
    }

    // Function to add the Arabic letter "غ" to the input field
    public void AddGhain()
    {
        char ghain = '\u063A'; // Unicode value for "غ"
        inputField.text += ghain.ToString();
    }

    // Function to add the Arabic letter "ف" to the input field
    public void AddFa()
    {
        char fa = '\u0641'; // Unicode value for "ف"
        inputField.text += fa.ToString();
    }

    // Function to add the Arabic letter "ق" to the input field
    public void AddQaf()
    {
        char qaf = '\u0642'; // Unicode value for "ق"
        inputField.text += qaf.ToString();
    }

    // Function to add the Arabic letter "ك" to the input field
    public void AddKaf()
    {
        char kaf = '\u0643'; // Unicode value for "ك"
        inputField.text += kaf.ToString();
    }

    // Function to add the Arabic letter "ل" to the input field
    public void AddLam()
    {
        char lam = '\u0644'; // Unicode value for "ل"
        inputField.text += lam.ToString();
    }

    // Function to add the Arabic letter "م" to the input field
    public void AddMeem()
    {
        char meem = '\u0645'; // Unicode value for "م"
        inputField.text += meem.ToString();
    }

    // Function to add the Arabic letter "ن" to the input field
    public void AddNoon()
    {
        char noon = '\u0646'; // Unicode value for "ن"
        inputField.text += noon.ToString();
    }

    // Function to add the Arabic letter "ه" to the input field
    public void AddHaa2()
    {
        char haa2 = '\u0647'; // Unicode value for "ه"
        inputField.text += haa2.ToString();
    }

    // Function to add the Arabic letter "و" to the input field
    public void AddWaw()
    {
        char waw = '\u0648'; // Unicode value for "و"
        inputField.text += waw.ToString();
    }

    // Function to add the Arabic letter "ي" to the input field
    public void AddYaa()
    {
        char yaa = '\u064A'; // Unicode value for "ي"
        inputField.text += yaa.ToString();
    }
    public void AddMaksurahamza() // ئ
    {
        char maksura = '\u0626';
        inputField.text += maksura.ToString();
    }

    public void tamarbota()
    {
        char marbota = '\u0629';
        inputField.text += marbota.ToString();
    }
    public void hamza()
    {
        char hamza = '\u0621';
        inputField.text += hamza.ToString();
    }
    public void addMaksura()
    {
        char maksura = '\u0649';
        inputField.text += maksura.ToString();
    }
    public void addwawhamza()
    {
        char waw = '\u0624';
        inputField.text += waw.ToString();
    }
}