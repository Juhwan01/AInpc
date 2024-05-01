using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var authPath = $"{userPath}/.openai/auth.json";
        Debug.Log(authPath);
        if (File.Exists(authPath))
        {
            var json = File.ReadAllText(authPath);
            
        }
        else
        {
            Debug.LogError("API Key is null and auth.json does not exist. Please check https://github.com/srcnalt/OpenAI-Unity#saving-your-credentials");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
