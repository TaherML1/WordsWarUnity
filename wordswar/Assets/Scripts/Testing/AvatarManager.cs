using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Storage;
using Firebase.Extensions;
public class AvatarManager : MonoBehaviour
{
    FirebaseStorage storage;
    StorageReference storageRef;
    List<string> avatarUrls = new List<string>();
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            storage = FirebaseStorage.DefaultInstance;
            storageRef = storage.GetReferenceFromUrl("gs://your-app.appspot.com/avatars/");
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
