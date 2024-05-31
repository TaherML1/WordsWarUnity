using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class SendWordsAndTopicToDB : MonoBehaviour
{

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                // Firebase is ready to use
                InitializeDatabase();
               
            }
            else
            {
                Debug.LogError("Failed to initialize Firebase: " + task.Result.ToString());
            }
        });
    }
    private void InitializeDatabase()
    {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;

        Dictionary<string, List<string>> topicsAndWords = new Dictionary<string, List<string>>
    {
        {"الحيوانات" , new List<string> {"كلب","قطة","فيل","اسد",}},
        {"الفواكه", new List<string> { "تفاح" , "موز", "برتقال", "عنب" } },
        {"البلدان", new List<string> { "سوريا", "الولايات المتحدة", "ألمانيا", "البرازيل", "تركيا" } },
        {"الالوان" , new List<string> {"اسود","ابيض","اصفر","احمر","اخضر"}},
        { "الرياضة", new List<string> {"كرة القدم", "بيسبول","كرة السلة","سباحة","تنس"}},
        {"اسماء بنات تبدا بحرف الياء", new List<string> { "يمنى", "ياسمين", "يسرى", "يانا" } },
        {"teams", new List<string> {"barca", "real", "city","inter"} },
        {"بلدان عربية", new List<string>{"سوريا","لبنان","عمان","الاردن","فلسطين"}  }
    };

        foreach (var topic in topicsAndWords)
        {
            Dictionary<string, object> wordsWithBoolean = new Dictionary<string, object>();
            foreach (var word in topic.Value)
            {
                wordsWithBoolean[word] = true; // Assign true value to each word
            }
            reference.Child("topics").Child(topic.Key).SetValueAsync(wordsWithBoolean);
        }

        Debug.Log("Topics and words sent to the database.");
    }

}
public class User
{
    public string username;
    public string email;

    public User()
    {
    }

    public User(string username, string email)
    {
        this.username = username;
        this.email = email;
    }
}