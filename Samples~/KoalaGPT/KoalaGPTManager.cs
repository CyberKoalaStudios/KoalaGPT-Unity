using System.Collections;
using System.Collections.Generic;
using KoalaGPT;
using UnityEngine;
using UnityEngine.Events;

public class KoalaGPTManager : MonoBehaviour
{
    public OnResponseEvent OnResponse;

    [System.Serializable]
    public class OnResponseEvent : UnityEvent<string>
    {
    }

    private KoalaGPTApi _koalaGptApi = new KoalaGPTApi();
    private List<Part> _messages = new List<Part>();


    public async void AskKoalaGPT(string prompt)
    {
        var message = new Part();
        message.Role = "user";
        message.Content = prompt;

        _messages.Add(message);

        var request = new CreateChatCompletionRequestPrompt();
        request.Messages = _messages;
        request.Model = "gpt4";

        var response = await _koalaGptApi.CreateChatCompletionSimplePrompt(request);

        if (response != null)
        {
            var tokenResponse = response;
            // _messages.Add(tokenResponse);

            Debug.Log(tokenResponse);
            
            OnResponse.Invoke(tokenResponse);
        }
    }
}