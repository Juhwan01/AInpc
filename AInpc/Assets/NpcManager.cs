using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI;

public class NpcManager : MonoBehaviour
{
    private static NpcManager instance = null;
    private OpenAIApi openAI = new OpenAIApi();
    private List<ChatMessage> messages = new List<ChatMessage>();


    public static NpcManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new NpcManager();
            }
            return instance;
        }
    }

    public async void AskChatGPT(string newText)
    {
        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = newText;
        newMessage.Role = "user";

        messages.Add(newMessage);

        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        request.Model = "gpt-3.5-turbo";

        var response = await openAI.CreateChatCompletion(request);

        if (response.Choices != null && response.Choices.Count > 0)
        {
            var chatResponse = response.Choices[0].Message;
            messages.Add(chatResponse);
            Debug.Log(chatResponse.Content);
            GoogleTTS.Instance.RunTTS(chatResponse.Content, SystemLanguage.Korean);
            //GoogleCloudTTS.Instance.RunTTS(chatResponse.Content);
        }
    }

    void Start()
    {
        // 초기화 코드 작성
    }

    void Update()
    {
        // 업데이트 코드 작성
    }
}
