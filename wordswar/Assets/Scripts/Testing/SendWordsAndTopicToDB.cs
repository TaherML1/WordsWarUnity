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

    public void InitializeDatabase()
    {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        Dictionary<string, Dictionary<string, object>> topicsAndWords = new Dictionary<string, Dictionary<string, object>>
{
    {
        "اسماء أفراد العائلة", new Dictionary<string, object>
        {
            {
                "الأب", new Dictionary<string, object>
                {
                    { "primary", "الأب" },
                    { "synonyms", new Dictionary<string, bool>
                        {
                            { "أب", true },
                            { "الاب", true },
                            { "بابا", true },
                        }
                    }
                }
            },
            {
                "الأم", new Dictionary<string, object>
                {
                    { "primary", "الأم" },
                    { "synonyms", new Dictionary<string, bool>
                        {
                            { "ام", true },
                            { "الام", true },
                            { "ماما", true },
                        }
                    }
                }
            },
            {
                "الأخ", new Dictionary<string, object>
                {
                    { "primary", "الأخ" },
                    { "synonyms", new Dictionary<string, bool>
                        {
                            { "الاخ", true },
                            { "اخ", true },
                        }
                    }
                }
            },
            {
                "الأخت", new Dictionary<string, object>
                {
                    { "primary", "الأخت" },
                    { "synonyms", new Dictionary<string, bool>
                        {
                            { "الاخت", true },
                            { "اخت", true },
                            { "أخت", true },
                        }
                    }
                }
            },
            {
                "الجد", new Dictionary<string, object>
                {
                    { "primary", "الجد" },
                    { "synonyms", new Dictionary<string, bool>
                        {
                            { "جد", true },
                            { "الجد", true },

                        }
                    }
                }
            },
            {
                "الجدة", new Dictionary<string, object>
                {
                    { "primary", "الجدة" },
                    { "synonyms", new Dictionary<string, bool>
                        {
                            { "جدتي", true },
                            { "الجدة", true },
                              { "جدة", true },
                        }
                    }
                }
            },
            {
                "العم", new Dictionary<string, object>
                {
                    { "primary", "العم" },
                    { "synonyms", new Dictionary<string, bool>
                        {
                            { "عمي", true },
                            { "العم", true },
                        }
                    }
                }
            },
            {
                "العمة", new Dictionary<string, object>
                {
                    { "primary", "العمة" },
                    { "synonyms", new Dictionary<string, bool>
                        {
                            { "عمتي", true },
                            { "العمة", true },
                        }
                    }
                }
            },
            {
                "الابن", new Dictionary<string, object>
                {
                    { "primary", "الابن" },
                    { "synonyms", new Dictionary<string, bool>
                        {
                            { "ابني", true },
                            { "الابن", true },

                        }
                    }
                }
            },
            {
                "الابنة", new Dictionary<string, object>
                {
                    { "primary", "الابنة" },
                    { "synonyms", new Dictionary<string, bool>
                        {
                            { "ابنة", true },
                            { "الابنة", true },

                        }
                    }
                }
            },
            {
                "الخال", new Dictionary<string, object>
                {
                    { "primary", "الخال" },
                    { "synonyms", new Dictionary<string, bool>
                        {
                            { "خال", true },


                        }
                    }
                }
            },
            {
                "حفيد", new Dictionary<string, object>
                {
                    { "primary", "حفيد" },
                    { "synonyms", new Dictionary<string, bool>
                        {
                       
                            { "الحفيد", true },

                        }
                    }
                }
           },
            {
                "حفيدة", new Dictionary<string, object>
                {
                    { "primary", "حفيدة" },
                    { "synonyms", new Dictionary<string, bool>
                        {
                          
                            { "الحفيدة", true },

                        }
                    }
                }
           },
            {
                "الصهر", new Dictionary<string, object>
                {
                    { "primary", "الصهر" },
                    { "synonyms", new Dictionary<string, bool>
                        {

                            { "صهر", true },

                        }
                    }
                }
           },
            {
                "السلف", new Dictionary<string, object>
                {
                    { "primary", "السلف" },
                    { "synonyms", new Dictionary<string, bool>
                        {

                            { "سلف", true },

                        }
                    }
                }
           },
            {
                "الحم", new Dictionary<string, object>
                {
                    { "primary", "الحم" },
                    { "synonyms", new Dictionary<string, bool>
                        {

                            { "الحم", true },

                        }
                    }
                }
           },
            {
                "الكنة", new Dictionary<string, object>
                {
                    { "primary", "الكنة" },
                    { "synonyms", new Dictionary<string, bool>
                        {

                            { "كنة", true },

                        }
                    }
                }
           },
            {
                "الحماة", new Dictionary<string, object>
                {
                    { "primary", "الحماة" },
                    { "synonyms", new Dictionary<string, bool>
                        {

                            { "حماة", true },

                        }
                    }
                }
           },
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
