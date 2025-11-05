using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public static class AsyncHelper
{
    public static async void Forget(this Task task, string errorMessage = "An error occurred")
    {
        try
        {
            await task;
        }
        catch (Exception e)
        {
            Debug.LogError($"{errorMessage}: {e}");
        }
    }
}

public class FirebaseInit_MRE : MonoBehaviour
{
    private FirebaseApp app = null;
    private FirebaseAuth auth = null;
    private FirebaseFirestore firestore = null;
    [SerializeField]
    private Button deleteFirebaseHeartbeatFolderButton, deleteFirestoreLogsAndConfigsButton, deleteFirestoreFolderButton;

    [SerializeField]
    private Button initButton, clearButton, loginButton, logoutButton, getDocumentButton;

    private void Awake()
    {
        deleteFirebaseHeartbeatFolderButton.onClick.AddListener(DeleteFirebaseHearbeatFolder);
        deleteFirestoreFolderButton.onClick.AddListener(DeleteFirestoreFolder);
        deleteFirestoreLogsAndConfigsButton.onClick.AddListener(DeleteFirebaseLogsAndConfigs);

        initButton.onClick.AddListener(() => InitFirebaseAsync().Forget("Could not resolve all Firebase dependencies"));
        clearButton.onClick.AddListener(() => ClearAsync().Forget("Error during sign out"));
        loginButton.onClick.AddListener(() => LoginAsync().Forget("Error during sign in"));
        logoutButton.onClick.AddListener(() => LogoutAsync().Forget("Error during sign out"));
        getDocumentButton.onClick.AddListener(() => GetDocumentAsync().Forget("Error getting document"));


    }
    void Start()
    {
        string appVersion = Application.version;
        Debug.Log("App version: " + appVersion);
    }

    private async Task InitFirebaseAsync()
    {
        await FirebaseApp.CheckAndFixDependenciesAsync();
        app = FirebaseApp.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;
        firestore.Settings.PersistenceEnabled = true;
        Debug.Log("Firebase Initialization successful");
        var user = auth.CurrentUser;
        Debug.Log("Current user: " + (user != null ? user.Email : "None"));
    }

    private async Task ClearAsync()
    {
        await firestore.TerminateAsync();
        await firestore.ClearPersistenceAsync();
        Debug.Log("Firestore persistence cleared.");
    }

    class Credentials
    {
        public string email;
        public string password;
    }
    private async Task LoginAsync()
    {
        string credentialsPath = System.IO.Path.Combine(Application.streamingAssetsPath, "credentials.json");
        string credentialsJson = await System.IO.File.ReadAllTextAsync(credentialsPath);
        var credentials = JsonUtility.FromJson<Credentials>(credentialsJson);
        var userCredential = await auth.SignInWithEmailAndPasswordAsync(credentials.email, credentials.password);
        var user = userCredential.User;
        Debug.Log("User signed in successfully: " + user.Email);
    }

    private async Task LogoutAsync()
    {
        auth.SignOut();
        var user = auth.CurrentUser;
        Debug.Log("Current user: " + (user != null ? user.Email : "None"));
    }

    private async Task GetDocumentAsync()
    {
        var user = auth.CurrentUser;
        Debug.Log("Current user: " + (user != null ? user.Email : "None"));
        Debug.Log("User ID: " + user.UserId);
        await Task.Delay(500);
        var collection = firestore.Collection("User");
        Debug.Log("Collection reference obtained.");
        await Task.Delay(500);
        var document = collection.Document(user.UserId);
        Debug.Log("Document reference obtained.");
        await Task.Delay(500);
        var snapshot = await document.GetSnapshotAsync();
        Debug.Log("Document retrieved successfully.");
    }
    private string GetLocalAppdataPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }
    private void DeleteFirebaseHearbeatFolder()
    {
        string path = Path.Combine(GetLocalAppdataPath(), "firebase-heartbeat");
        if (Directory.Exists(path))
        {

            Directory.Delete(path, true);
            Debug.Log($"Directory deleted: {path}");


        }
        else
        {
            Debug.Log($"Directory not found: {path}");
        }
    }
    private void DeleteFirebaseLogsAndConfigs()
    {
        string path = Path.Combine(GetLocalAppdataPath(), "firestore", "__FIRAPP_DEFAULT", "cynteract-a52e4", "main");
        var files = Directory.GetFiles(path);
        if (files.Length == 0)
        {
            print("No files found");
        }
        foreach (var filePath in files)
        {
            var extension = Path.GetExtension(filePath);
            if (extension == ".ldb")
            {
                print($"Skipping database file: {filePath}");
                continue;
            }
            else
            {
                print($"Deleted file: {filePath}");

            }
        }

    }
    private void DeleteFirestoreFolder()
    {
        string path = Path.Combine(GetLocalAppdataPath(), "firestore");
        if (Directory.Exists(path))
        {

            Directory.Delete(path, true);
            Debug.Log($"Directory deleted: {path}");


        }
        else
        {
            Debug.Log($"Directory not found: {path}");
        }
    }
}