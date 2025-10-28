using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;

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
    void Start()
    {
        string appVersion = Application.version;
        Debug.Log("App version: " + appVersion);

        // Get all Button components in the active scene (including inactive)
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var rootObjects = scene.GetRootGameObjects();
        var buttons = new System.Collections.Generic.List<Button>();
        foreach (var root in rootObjects)
        {
            buttons.AddRange(root.GetComponentsInChildren<Button>(true));
        }
        // Ensure exactly 5 buttons were found
        UnityEngine.Assertions.Assert.AreEqual(5, buttons.Count, $"Expected 5 buttons in scene but found {buttons.Count}.");

        buttons[0].onClick.AddListener(() => InitFirebaseAsync().Forget("Could not resolve all Firebase dependencies"));
        buttons[1].onClick.AddListener(() => ClearAsync().Forget("Error during sign out"));
        buttons[2].onClick.AddListener(() => LoginAsync().Forget("Error during sign in"));
        buttons[3].onClick.AddListener(() => LogoutAsync().Forget("Error during sign out"));
        buttons[4].onClick.AddListener(() => GetDocumentAsync().Forget("Error getting document"));
    }

    private async Task InitFirebaseAsync()
    {
        await FirebaseApp.CheckAndFixDependenciesAsync();
        FirebaseFirestore.DefaultInstance.Settings.PersistenceEnabled = true;
        Debug.Log("Firebase Initialization successful");
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        Debug.Log("Current user: " + (user != null ? user.Email : "None"));
    }

    private async Task ClearAsync()
    {
        await FirebaseFirestore.DefaultInstance.TerminateAsync();
        await FirebaseFirestore.DefaultInstance.ClearPersistenceAsync();
        Debug.Log("Firestore persistence cleared.");
    }

    class Credentials
    {
        public string email;
        public string password;
    }
    private async Task LoginAsync()
    {
        var auth = FirebaseAuth.DefaultInstance;
        // Read credentials from StreamingAssets/credentials.json

        string credentialsPath = System.IO.Path.Combine(Application.streamingAssetsPath, "credentials.json");
        string credentialsJson = await System.IO.File.ReadAllTextAsync(credentialsPath);
        var credentials = JsonUtility.FromJson<Credentials>(credentialsJson);
        var userCredential = await auth.SignInWithEmailAndPasswordAsync(credentials.email, credentials.password);
        var user = userCredential.User;
        Debug.Log("User signed in successfully: " + user.Email);
    }

    private async Task LogoutAsync()
    {
        FirebaseAuth.DefaultInstance.SignOut();
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        Debug.Log("Current user: " + (user != null ? user.Email : "None"));
    }

    private async Task GetDocumentAsync()
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        Debug.Log("Current user: " + (user != null ? user.Email : "None"));
        Debug.Log("User ID: " + user.UserId);
        await FirebaseFirestore.DefaultInstance.Collection("User").Document(user.UserId).GetSnapshotAsync();
        Debug.Log("Document retrieved successfully.");
    }
}