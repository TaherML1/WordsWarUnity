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

        Dictionary<string, Dictionary<string, object>> topicsAndWords = new Dictionary<string, Dictionary<string, object>>
        {
            {
                "fruits", new Dictionary<string, object>
                {
                    {
                        "apple", new Dictionary<string, object>
                        {
                            { "primary", "تفاح" },
                            { "synonyms", new Dictionary<string, bool>
                                {
                                    { "تفاحة", true },
                                    { "تفاحي", true },
                                    { "تفاحات", true },
                                    { "تفاحه", true }
                                }
                            }
                        }
                    },
                    {
                        "banana", new Dictionary<string, object>
                        {
                            { "primary", "موز" },
                            { "synonyms", new Dictionary<string, bool>
                                {
                                    { "موزة", true },
                                    { "موزات", true }
                                }
                            }
                        }
                    }
                    // Add more fruits or topics as needed
                }
            }
        };

        foreach (var topic in topicsAndWords)
        {
            foreach (var wordData in topic.Value)
            {
                string wordKey = wordData.Key;
                Dictionary<string, object> wordInfo = (Dictionary<string, object>)wordData.Value;

                // Set the value in Firebase under "topics/{topic.Key}/{wordKey}"
                reference.Child("topics").Child(topic.Key).Child(wordKey).SetValueAsync(wordInfo)
                    .ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCompleted && !task.IsFaulted)
                        {
                            Debug.Log($"Word '{wordKey}' under topic '{topic.Key}' sent to the database.");
                        }
                        else
                        {
                            Debug.LogError($"Failed to send word '{wordKey}' under topic '{topic.Key}' to the database: {task.Exception}");
                        }
                    });
            }
        }
    }
}
