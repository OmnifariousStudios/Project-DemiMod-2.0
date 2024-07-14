using System;
using System.Threading.Tasks;
using ModIO;
using UnityEditor;
using UnityEngine;

public class ModIODemiMod : EditorWindow
{
    public bool isLoggedIn = false;

    public string currentEmail;
    public string currentSecurityCode;
    
    // Creates a window that will let the user type in their username and password and log in to mod.io.
    //[MenuItem("Project Demigod/Mod.io Login")]
    public static void ShowModIOLogin()
    {
        EditorWindow.GetWindow(typeof(ModIODemiMod));
    }

    private void OnGUI()
    {
        GUILayout.Label("Mod.io Login", EditorStyles.boldLabel);
        
        GUILayout.Label("Enter your username and password to log in to mod.io.");
        
        GUILayout.Label("Username:");
        currentEmail = EditorGUILayout.TextField("", GUILayout.Width(200));
        
        GUILayout.Label("Password:");
        currentSecurityCode = EditorGUILayout.PasswordField("", GUILayout.Width(200));
        
        
        if (GUILayout.Button("Log In"))
        {
            // Log in to mod.io.
            SendAuthenticationEmail(currentEmail);
        }
        
        
        if (GUILayout.Button("Submit Security Code"))
        {
            SubmitSecurityCode(currentSecurityCode);
            isLoggedIn = true;
        }
    }
    
    
    
    async void SendAuthenticationEmail(string emailAddress)
    {
        Result result = await ModIOUnityAsync.RequestAuthenticationEmail(emailAddress);
 
        if (result.Succeeded())
        {
            Debug.Log("Succeeded to send security code");
        }
        else
        {
            Debug.Log("Failed to send security code to that email address");
            
            Debug.Log(result.message);
            
        }
    }
    
    async void SubmitSecurityCode(string currentSecurityCode)
    {
        Result result = await ModIOUnityAsync.SubmitEmailSecurityCode(currentSecurityCode);
 
        if (result.Succeeded())
        {
            Debug.Log("Succeeded to log in");
        }
        else
        {
            Debug.Log("Failed to log in");
        }
    }
}
