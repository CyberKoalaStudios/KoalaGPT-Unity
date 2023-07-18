![YouTube Channel Views](https://img.shields.io/youtube/channel/views/UCUpVfgd42h7pwZwCTcwjp8g)
![Discord](https://img.shields.io/discord/1016305251936129094)



---

## KoalaGPT Unity Package
An official Unity package that allows you to use the KoalaGPT API directly in the Unity game engine.

![Imgur](https://i.imgur.com/BYOPVby.png)

## How To Use
See [[Video](https://www.youtube.com/watch?v=htAeWQ5OaZE)](https://youtu.be/htAeWQ5OaZE)

[![](http://img.youtube.com/vi/htAeWQ5OaZE/0.jpg)](http://www.youtube.com/watch?v=htAeWQ5OaZE "KoalaGPT API Quick Start Tutorial")

### Importing the Package
To import the package, follow these steps:
- Open Unity 2019 or later
- Go to `Window > Package Manager`
- Click the `+` button and select `Add package from git URL`
- Paste the repository URL https://github.com/CyberKoalaStudios/KoalaGPT-Unity.git and click `Add`

### Setting Up Your CyberKoala Account
To use the KoalaGPT API, you need to have an CyberKoala account. Follow these steps to create an account and generate an API key:

- Go to https://beta.cyberkoala.ru/ and sign up for an account
- Once you have created an account, go to https://beta.cyberkoala.ru/main/dashboard
- Buy a new secret key and save it

### Saving Your Credentials
To make requests to the KoalaGPT API, you need to use your API key and organization name (if applicable). To avoid exposing your API key in your Unity project, you can save it in your device's local storage.

To do this, follow these steps:

- Create a folder called .cyberkoala in your home directory (e.g. `C:Users\UserName\` for Windows or `~\` for Linux or Mac)
- Create a file called `auth.json` in the `.cyberkoala` folder
- Add an api_key field and a organization field (if applicable) to the auth.json file and save it
- Here is an example of what your auth.json file should look like:

```json
{
    "api_key": "ko-...er",
    "organization": "org-...MOW"
}
```

**IMPORTANT:** Your API key is a secret.
Do not share it with others or expose it in any client-side code (e.g. browsers, apps).
If you are using KoalaGPT for production, make sure to run it on the server side, where your API key can be securely loaded from an environment variable or key management service.

### Making Requests to KoalaGPT
You can use the `KoalaGPTApi` class to make async requests to the KoalaGPT API.

All methods are asynchronous and can be accessed directly from an instance of the `KoalaGPTApi` class.

Here is an example of how to make a request:

```csharp
private async void SendRequest()
{
    var _koalaGptApi = new KoalaGPTApi();
    
    var _messages = new List<Part>();
    var message = new Part();
    message.Role = "user";
    message.Content = prompt;
    
    _messages.Add(message);
        
    var request = new CreateChatCompletionRequestPrompt{
        Model="gpt4",
        Prompt="Hello!",
    };
    var response = await _koalaGptApi.CreateChatCompletionSimplePrompt(request);
}
```

### Making Voice Request.
Voices available: `jane, filipp, omazh, madirus`
```csharp
private async void SpeakKoalaGPT()
        {
            var message = new Part();
            message.Role = "user";
            message.Content = "Hi! Help me to pick right wand";

            _messages.Add(message);

            var request = new CreateChatCompletionRequestPrompt();
            request.Messages = _messages;
            request.Model = "gpt4";
            request.Voice = "filipp";
            
            var response = await _koalaGptApi.CreateSpeechPrompt(request);

            if (response != null)
            {
                var audioClip = response;
                Debug.Log(audioClip.length);
     
                _audioSource.clip = audioClip;
                _audioSource.Play();
            }
        }
```

### Sample Projects
This package includes two sample scenes that you can import via the Package Manager:

- **KoalaGPT sample:** A simple KoalaGPT like chat example.

### Supported Unity Versions for WebGL Builds
The following table shows the supported Unity versions for WebGL builds:

| Unity Version | Supported |
| --- | --- |
| 2022.2.8f1 | ✅ |
| 2021.3.5f1 | ⛔ |
| 2020.3.0f1 | ✅ |

