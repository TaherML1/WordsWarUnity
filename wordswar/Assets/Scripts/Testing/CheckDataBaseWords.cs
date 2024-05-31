using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions; // for ContinueWithOnMainThread
using TMPro;
using Firebase.Functions;
using System;
using System.Threading.Tasks;

public class CheckDataBaseWords : MonoBehaviour
{
    // Start is called before the first frame update
    private DatabaseReference reference;
    private FirebaseFunctions functions;

    public TextMeshProUGUI nameText;
   // string topicName = "teams";
    //string word = "city";
    void Start()
    {
        // Get the root reference location of the database.
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        functions = FirebaseFunctions.DefaultInstance;
     
       // GetTopicWordsFromCloudFunction(topicName);
      // GetArabicFemaleNames();
        /*ReadCountries();
        Task.Run(async () =>
        {
            bool exists = await CheckTopicAndWordExistence(topicName, word);
            if (exists)
            {
                Debug.Log($"Word '{word}' exists under the topic '{topicName}'.");
            }
            else
            {
                Debug.LogWarning($"Word '{word}' does not exist under the topic '{topicName}'.");
            }
        });*/

    }

    public void ReadTeamsFromTopics()
    {
        // Reference the "topics" node
        DatabaseReference topicsRef = reference.Child("topics");

        // Attach a listener to the "topics" node to capture its children (including "teams")
        topicsRef.ValueChanged += (object sender, ValueChangedEventArgs e) =>
        {
            if (e.Snapshot.HasChildren)
            {
                IDictionary topicsData = e.Snapshot.Value as IDictionary;

                // Assuming topicsData is likely a Dictionary<string, object>
                if (topicsData is Dictionary<string, object>)
                {
                    Dictionary<string, object> typedTopicsData = (Dictionary<string, object>)topicsData;

                    // Check if the "teams" node exists using TryGetValue
                    object teamData;
                    if (typedTopicsData.TryGetValue("teams", out teamData))
                    {
                        // Access the "teams" node data
                        if (teamData is DataSnapshot)
                        {
                            DataSnapshot teamsSnapshot = (DataSnapshot)teamData;

                            // Iterate through the children of the "teams" node (numeric keys)
                            foreach (DataSnapshot teamSnapshot in teamsSnapshot.Children)
                            {
                                string teamName = teamSnapshot.Value as string; // Access team name (value)

                                // Process team name
                                Debug.Log("Team: " + teamName);
                            }
                        }
                        else
                        {
                            Debug.LogError("The value associated with 'teams' is not a DataSnapshot.");
                        }
                    }
                    else
                    {
                        Debug.LogError("The 'topics' node does not contain a 'teams' node.");
                    }
                }
                else
                {
                    Debug.Log("topicsData is not a Dictionary<string, object>.");
                }
            }
            else
            {
                Debug.Log("The 'topics' node has no data.");
            }
        };
    }

    public async void GetArabicFemaleNames()
    {
        DatabaseReference namesRef = reference.Child("topics").Child("اسماء بنات تبدا بحرف الياء");
        DataSnapshot snapshot = await namesRef.GetValueAsync();

        if (snapshot.HasChildren)
        {
            IList<string> names = new List<string>();
            string allwords = "";
            foreach (DataSnapshot nameSnapshot in snapshot.Children)
            {
                // Check if the value is a boolean
                if (nameSnapshot.Value is bool)
                {
                    bool value = (bool)nameSnapshot.Value;
                    // Assuming you want to skip boolean values
                    continue;
                }

                string name = nameSnapshot.Value as string;
                if (name != null)
                {
                    allwords += name + "\n";
                    names.Add(name); // Add name to the list
                }
            }

            nameText.text = allwords;
            Debug.Log("Arabic Female Names (Yaa):");
            foreach (string name in names)
            {
                Debug.Log(name);
            }
        }
        else
        {
            Debug.LogError("The 'اسماء بنات تبدا بحرف الياء' node has no data.");
        }
    }

   /* public async Task<bool> CheckTopicAndWordExistence(string topicName, string word)
    {
        // Check if the topic exists
        DataSnapshot topicSnapshot = await reference.Child("topics").Child(topicName).GetValueAsync();
        if (!topicSnapshot.Exists)
        {
            Debug.LogWarning($"Topic '{topicName}' does not exist.");
            return false;
        }

        // Check if the word exists under the topic
        DataSnapshot wordSnapshot = topicSnapshot.Child(word);
        if (!wordSnapshot.Exists)
        {
            Debug.LogWarning($"Word '{word}' does not exist under the topic '{topicName}'.");
            return false;
        }

        return true; // Both topic and word exist
    }*/
    public async void ReadCountries()
    {
        DatabaseReference countryRef = reference.Child("topics").Child("البلدان");
        DataSnapshot dataSnapshot = await countryRef.GetValueAsync();

        if (dataSnapshot.HasChildren)
        {
            IList<string> countryNames = new List<string>();
            foreach (DataSnapshot nameSnapShot in dataSnapshot.Children)
            {
                // Check if the value is a boolean
                if (nameSnapShot.Value is bool)
                {
                    bool value = (bool)nameSnapShot.Value;
                    // Assuming you want to skip boolean values
                    continue;
                }

                string name = nameSnapShot.Value as string;
                countryNames.Add(name); // Add name to the list
            }

            foreach (string countryName in countryNames)
            {
                Debug.Log(countryName);
            }
        }
    }


   
   /* public void CallCloudFunction(string userId)
    {
        Debug.Log("function is working");
        // Create a dictionary to pass parameters to the Cloud Function
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "userId", userId }
        };

        // Call the Cloud Function with parameters
        functions.GetHttpsCallable("retrieveUserData")
            .CallAsync(data).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"Cloud Function call failed: {task.Exception}");
                    return;
                }

                // Handle the response
                var result = (Dictionary<string, object>)task.Result.Data;
                string username = (string)result["username"];
                string email = (string)result["email"];

                // Output the username and email
                Debug.Log($"Username: {username}, Email: {email}");
            });
    }*/
}

[Serializable]
public class WordsDataWrapper
{
    public WordsData Data;
}

[Serializable]
public class WordsData
{
    public List<string> Words { get; set; }
}