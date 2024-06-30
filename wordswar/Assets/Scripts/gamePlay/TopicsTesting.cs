using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ArabicSupport;
using TMPro;
using Firebase;
using Firebase.Database;
using System.Collections.Generic;
using Firebase.Auth;
using System;
using Firebase.Functions;
using System.Threading.Tasks;
using Firebase.Firestore;
using Firebase.Extensions;


public class TopicsTesting : MonoBehaviour
{
    private FirebaseDatabase database;
    private DatabaseReference databaseReference;
    
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private FirebaseFunctions functions;


    public TMP_InputField playerInput;
    public Button submitButton;
    void Start()
    {
        Task<DependencyStatus> firebaseTask = FirebaseApp.CheckAndFixDependenciesAsync();
        

        if (firebaseTask.Result == DependencyStatus.Available)
        {
            // Set up Firebase references and components
           


           
        }
        else
        {
            Debug.LogError("Failed to initialize Firebase");
        }
    }

    
}
