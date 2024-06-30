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
    "بلدان قارة اسيا", new Dictionary<string, object>
    {

        {
            "السعودية", new Dictionary<string, object>
            {
                { "primary", "السعودية" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "المملكة العربية السعودية", true },
                        { "سعودية", true }
                    }
                }
            }
        },
        {
            "الامارات", new Dictionary<string, object>
            {
                { "primary", "الامارات" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "الامارات العربية المتحدة", true },
                        { "دولة الامارات", true }
                    }
                }
            }
        },
        {
            "قطر", new Dictionary<string, object>
            {
                { "primary", "قطر" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "دولة قطر", true }
                    }
                }
            }
        },
        {
            "البحرين", new Dictionary<string, object>
            {
                { "primary", "البحرين" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "مملكة البحرين", true }
                    }
                }
            }
        },
        {
            "عمان", new Dictionary<string, object>
            {
                { "primary", "عمان" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "سلطنة عمان", true }
                    }
                }
            }
        },
        {
            "الكويت", new Dictionary<string, object>
            {
                { "primary", "الكويت" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "دولة الكويت", true }
                    }
                }
            }
        },
        {
            "العراق", new Dictionary<string, object>
            {
                { "primary", "العراق" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "جمهورية العراق", true }
                    }
                }
            }
        },
        {
            "اليمن", new Dictionary<string, object>
            {
                { "primary", "اليمن" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "الجمهورية اليمنية", true }
                    }
                }
            }
        },
        {
            "سوريا", new Dictionary<string, object>
            {
                { "primary", "سوريا" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "الجمهورية العربية السورية", true }
                    }
                }
            }
        },
        {
            "لبنان", new Dictionary<string, object>
            {
                { "primary", "لبنان" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "الجمهورية اللبنانية", true }
                    }
                }
            }
        },
        {
            "فلسطين", new Dictionary<string, object>
            {
                { "primary", "فلسطين" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "دولة فلسطين", true }
                    }
                }
            }
        },
        {
            "الاردن", new Dictionary<string, object>
            {
                { "primary", "الاردن" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "المملكة الاردنية الهاشمية", true }
                    }
                }
            }
        },
        {
            "تركيا", new Dictionary<string, object>
            {
                { "primary", "تركيا" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "الجمهورية التركية", true }
                    }
                }
            }
        },
        {
            "ايران", new Dictionary<string, object>
            {
                { "primary", "ايران" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "الجمهورية الاسلامية الايرانية", true }
                    }
                }
            }
        },

        {
            "الهند", new Dictionary<string, object>
            {
                { "primary", "الهند" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "جمهورية الهند", true }
                    }
                }
            }
        },
        {
            "باكستان", new Dictionary<string, object>
            {
                { "primary", "باكستان" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "جمهورية باكستان الاسلامية", true }
                    }
                }
            }
        },


         {
            "اوزباكستان", new Dictionary<string, object>
            {
                { "primary", "اوزباكستان" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "اوزبكستان", true }

                    }
                }
            }
        },
         {
            "طاجيكسان", new Dictionary<string, object>
            {
                { "primary", "طاجيكسان" },
                { "synonyms", new Dictionary<string, bool>
                    {


                    }
                }
            }
        },
         {
            "افغانستان", new Dictionary<string, object>
            {
                { "primary", "افغانستان" },
                { "synonyms", new Dictionary<string, bool>
                    {


                    }
                }
            }
        },

        {
            "بنغلاديش", new Dictionary<string, object>
            {
                { "primary", "بنغلاديش" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "جمهورية بنغلاديش الشعبية", true }
                    }
                }
            }
        },
        {
            "سريلانكا", new Dictionary<string, object>
            {
                { "primary", "سريلانكا" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "جمهورية سريلانكا الديمقراطية الاشتراكية", true },
                      { "سيرلانكا", true }
                    }
                }
            }
        },

        {
            "نيبال", new Dictionary<string, object>
            {
                { "primary", "نيبال" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "جمهورية نيبال الديمقراطية الاتحادية", true }
                    }
                }
            }
        },
        {
            "بوتان", new Dictionary<string, object>
            {
                { "primary", "بوتان" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "مملكة بوتان", true }
                    }
                }
            }
        },
        {
            "مالديف", new Dictionary<string, object>
            {
                { "primary", "مالديف" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "جزر المالديف", true }
                    }
                }
            }
        },
        {
            "سنغافورة", new Dictionary<string, object>
            {
                { "primary", "سنغافورة" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "الجمهورية السنغافورية", true }
                    }
                }
            }
        },

        {
            "اندونيسيا", new Dictionary<string, object>
            {
                { "primary", "اندونيسيا" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "جمهورية اندونيسيا", true }
                    }
                }
            }
        },
        {
            "ماليزيا", new Dictionary<string, object>
            {
                { "primary", "ماليزيا" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "اتحاد ماليزيا", true }
                    }
                }
            }
        },
        {
            "فيتنام", new Dictionary<string, object>
            {
                { "primary", "فيتنام" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "الجمهورية الاشتراكية الفيتنامية", true }
                    }
                }
            }
        },
        {
            "تايلاند", new Dictionary<string, object>
            {
                { "primary", "تايلاند" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "مملكة تايلاند", true },
                        { " تايلاندا", true }
                    }
                }
            }
        },
        
      
        {
            "الفلبين", new Dictionary<string, object>
            {
                { "primary", "الفلبين" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "جمهورية الفلبين", true }
                    }
                }
            }
        },
        {
            "بروناي", new Dictionary<string, object>
            {
                { "primary", "بروناي" },
                { "synonyms", new Dictionary<string, bool>
                    {

                    }
                }
            }
        },
        {
            "لاوس", new Dictionary<string, object>
            {
                { "primary", "لاوس" },
                { "synonyms", new Dictionary<string, bool>
                    {

                    }
                }
            }
        },
        {
            "تيمور الشرقية", new Dictionary<string, object>
            {
                { "primary", "تيمور الشرقية" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "جمهورية تيمور الشرقية", true },
                        { " تيمور ", true }
                    }
                }
            }
        },

        {
            "الصين", new Dictionary<string, object>
            {
                { "primary", "الصين" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "الصين الشعبية", true },
                        { "جمهورية الصين الشعبية", true }

                    }
                }
            }
        },
        {
            "اليابان", new Dictionary<string, object>
            {
                { "primary", "اليابان" },
                { "synonyms", new Dictionary<string, bool>
                    {
                        { "الامبراطورية اليابانية", true },
                        { "يابان", true },
                    }
                }
            }
        },
        {
            "كوريا الجنوبية", new Dictionary<string, object>
            {
                { "primary", "كوريا الجنوبية" },
                { "synonyms", new Dictionary<string, bool>
                    {

                    }
                }
            }
        },
        {
            "كوريا الشمالية", new Dictionary<string, object>
            {
                { "primary", "كوريا الشمالية" },
                { "synonyms", new Dictionary<string, bool>
                    {

                    }
                }
            }
        },
        {
            "منغوليا", new Dictionary<string, object>
            {
                { "primary", "منغوليا" },
                { "synonyms", new Dictionary<string, bool>
                    {

                    }
                }
            }
        },
        {
            "تايوان", new Dictionary<string, object>
            {
                { "primary", "تايوان" },
                { "synonyms", new Dictionary<string, bool>
                    {

                    }
                }
            }
        },
        {
            "هونغ كونغ", new Dictionary<string, object>
            {
                { "primary", "هونغ كونغ" },
                { "synonyms", new Dictionary<string, bool>
                    {

                    }
                }
            }
        },
        {
            "ماكاو", new Dictionary<string, object>
            {
                { "primary", "ماكاو" },
                { "synonyms", new Dictionary<string, bool>
                    {

                    }
                }
            }
        }
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
