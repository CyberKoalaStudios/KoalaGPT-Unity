using System;
using System.Collections;
using System.IO;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections.Specialized;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace KoalaGPT
{
    public class KoalaGPTApi
    {
        /// KoalaGPT API base path for requests.
        private const string BASE_PATH = "https://gpt.cyberkoala.ru/backend-api/v2";

        /// Used for serializing and deserializing PascalCase request object fields into snake_case format for JSON. Ignores null fields when creating JSON strings.
        private readonly JsonSerializerSettings jsonSerializerSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CustomNamingStrategy()
            },
            MissingMemberHandling = MissingMemberHandling.Error,
            Culture = CultureInfo.InvariantCulture
        };

        /// <summary>
        ///     Reads and sets user credentials from %User%/.KoalaGPT/auth.json
        ///     Remember that your API key is a secret! Do not share it with others or expose it in any client-side code (browsers,
        ///     apps).
        ///     Production requests must be routed through your own backend server where your API key can be securely loaded from
        ///     an environment variable or key management service.
        /// </summary>
        private Configuration configuration;

        public KoalaGPTApi(string apiKey = null, string organization = null)
        {
            if (apiKey != null) configuration = new Configuration(apiKey, organization);
        }

        private Configuration Configuration
        {
            get
            {
                if (configuration == null) configuration = new Configuration();

                return configuration;
            }
        }

        /// <summary>
        ///     Dispatches an HTTP request to the specified path with the specified method and optional payload.
        /// </summary>
        /// <param name="path">The path to send the request to.</param>
        /// <param name="method">The HTTP method to use for the request.</param>
        /// <param name="payload">An optional byte array of json payload to include in the request.</param>
        /// <typeparam name="T">Response type of the request.</typeparam>
        /// <returns>A Task containing the response from the request as the specified type.</returns>
        private async Task<T> DispatchRequest<T>(string path, string method, byte[] payload = null) where T : IResponse
        {
            T data;

            using (var request = UnityWebRequest.Put(path, payload))
            {
                request.method = method;
                request.SetHeaders(Configuration, ContentType.ApplicationJson);

                var asyncOperation = request.SendWebRequest();

                while (!asyncOperation.isDone) await Task.Yield();

                data = JsonConvert.DeserializeObject<T>(request.downloadHandler.text, jsonSerializerSettings);
            }

            if (data?.Error == null) return data;
            var error = data.Error;
            Debug.LogError($"Error Message: {error.Message}\nError Type: {error.Type}\n");

            return data;
        }

        /// <summary>
        ///     Dispatches an HTTP request to the specified path with the specified method and optional payload.
        /// </summary>
        /// <param name="path">The path to send the request to.</param>
        /// <param name="method">The HTTP method to use for the request.</param>
        /// <param name="onResponse">A callback function to be called when a response is updated.</param>
        /// <param name="onComplete">A callback function to be called when the request is complete.</param>
        /// <param name="token">A cancellation token to cancel the request.</param>
        /// <param name="payload">An optional byte array of json payload to include in the request.</param>
        private async void DispatchRequest<T>(string path, string method, Action<List<T>> onResponse, Action onComplete,
            CancellationTokenSource token, byte[] payload = null) where T : IResponse
        {
            using (var request = UnityWebRequest.Put(path, payload))
            {
                request.method = method;
                request.SetHeaders(Configuration, ContentType.ApplicationJson);

                var asyncOperation = request.SendWebRequest();

                do
                {
                    var dataList = new List<T>();
                    var lines = request.downloadHandler.text.Split('\n').Where(line => line != "").ToArray();

                    foreach (var line in lines)
                    {
                        var value = line.Replace("data: ", "");

                        if (value.Contains("[DONE]"))
                        {
                            onComplete?.Invoke();
                            break;
                        }

                        var data = JsonConvert.DeserializeObject<T>(value, jsonSerializerSettings);

                        if (data?.Error != null)
                        {
                            var error = data.Error;
                            Debug.LogError($"Error Message: {error.Message}\nError Type: {error.Type}\n");
                        }
                        else
                        {
                            dataList.Add(data);
                        }
                    }

                    onResponse?.Invoke(dataList);

                    await Task.Yield();
                } while (!asyncOperation.isDone && !token.IsCancellationRequested);

                onComplete?.Invoke();
            }
        }

        /// <summary>
        ///     Dispatches an HTTP request to the specified path with a multi-part data form.
        /// </summary>
        /// <param name="path">The path to send the request to.</param>
        /// <param name="form">A multi-part data form to upload with the request.</param>
        /// <typeparam name="T">Response type of the request.</typeparam>
        /// <returns>A Task containing the response from the request as the specified type.</returns>
        private async Task<T> DispatchRequest<T>(string path, List<IMultipartFormSection> form) where T : IResponse
        {
            T data;

            using (var request = new UnityWebRequest(path, "POST"))
            {
                request.SetHeaders(Configuration);
                var boundary = UnityWebRequest.GenerateBoundary();
                var formSections = UnityWebRequest.SerializeFormSections(form, boundary);
                var contentType = $"{ContentType.MultipartFormData}; boundary={Encoding.UTF8.GetString(boundary)}";
                request.uploadHandler = new UploadHandlerRaw(formSections) { contentType = contentType };
                request.downloadHandler = new DownloadHandlerBuffer();
                var asyncOperation = request.SendWebRequest();

                while (!asyncOperation.isDone) await Task.Yield();

                data = JsonConvert.DeserializeObject<T>(request.downloadHandler.text, jsonSerializerSettings);
            }

            if (data != null && data.Error != null)
            {
                var error = data.Error;
                Debug.LogError($"Error Message: {error.Message}\nError Type: {error.Type}\n");
            }

            return data;
        }

        private async Task<string> DispatchSimpleRequest(string path, byte[] payload = null )
        {
            var data = "";

            using var request = UnityWebRequest.Put(path, payload);
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.SetHeaders(Configuration, ContentType.ApplicationJson);
            
            var asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone) await Task.Yield();
                
            data = request.downloadHandler.text;

            return data;
        }
        
        private async Task<AudioClip> DispatchAudioRequest(string path, byte[] payload = null )
        {
            using var request = UnityWebRequest.Put(path, payload);
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.SetHeaders(Configuration, ContentType.ApplicationJson);
            request.downloadHandler = new DownloadHandlerAudioClip(path, AudioType.WAV);
            
            var asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone) await Task.Yield();

            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(request);

            return audioClip;
        }

        
        /// <summary>
        ///     Create byte array payload from the given request object that contains the parameters.
        /// </summary>
        /// <param name="request">The request object that contains the parameters of the payload.</param>
        /// <typeparam name="T">type of the request object.</typeparam>
        /// <returns>Byte array payload.</returns>
        private byte[] CreatePayload<T>(T request)
        {
            var json = JsonConvert.SerializeObject(request, jsonSerializerSettings);
            Debug.Log(json);
            return Encoding.UTF8.GetBytes(json);
        }
        private byte[] CreatePayloadKoala<T>(T request)
        {
            var json = JsonConvert.SerializeObject(request, jsonSerializerSettings);
            return Encoding.UTF8.GetBytes(json);
        }

        /*
         
        /// <summary>
        ///     Lists the currently available models, and provides basic information about each one such as the owner and availability.
        /// </summary>
        public async Task<ListModelsResponse> ListModels()
        {
            var path = $"{BASE_PATH}/models";
            return await DispatchRequest<ListModelsResponse>(path, UnityWebRequest.kHttpVerbGET);
        }
    
        /// <summary>
        ///     Retrieves a model instance, providing basic information about the model such as the owner and permissioning.
        /// </summary>
        /// <param name="id">The ID of the model to use for this request</param>
        /// <returns>See <see cref="Model"/></returns>
        public async Task<KoalaGPTModel> RetrieveModel(string id)
        {
            var path = $"{BASE_PATH}/models/{id}";
            return await DispatchRequest<KoalaGPTModelResponse>(path, UnityWebRequest.kHttpVerbGET);
        }
        
        */

        /// <summary>
        ///     Creates a completion for the provided prompt and parameters.
        /// </summary>
        /// <param name="request">See <see cref="CreateCompletionRequest" /></param>
        /// <returns>See <see cref="CreateCompletionResponse" /></returns>
        public async Task<CreateCompletionResponse> CreateCompletion(CreateCompletionRequest request)
        {
            var path = $"{BASE_PATH}/conversation";
            var payload = CreatePayload(request);
            return await DispatchRequest<CreateCompletionResponse>(path, UnityWebRequest.kHttpVerbPOST, payload);
        }

        /// <summary>
        ///     Creates a chat completion request as in ChatGPT.
        /// </summary>
        /// <param name="request">See <see cref="CreateChatCompletionRequest" /></param>
        /// <param name="onResponse">Callback function that will be called when stream response is updated.</param>
        /// <param name="onComplete">Callback function that will be called when stream response is completed.</param>
        /// <param name="token">Cancellation token to cancel the request.</param>
        public void CreateCompletionAsync(CreateCompletionRequest request,
            Action<List<CreateCompletionResponse>> onResponse, Action onComplete, CancellationTokenSource token)
        {
            request.Stream = true;
            var path = $"{BASE_PATH}/conversation";
            var payload = CreatePayload(request);

            DispatchRequest(path, UnityWebRequest.kHttpVerbPOST, onResponse, onComplete, token, payload);
        }

        /// <summary>
        ///     Creates a chat completion request as in ChatGPT.
        /// </summary>
        /// <param name="request">See <see cref="CreateChatCompletionRequest"/></param>
        /// <returns>See <see cref="CreateChatCompletionResponse"/></returns>
        public async Task<CreateChatCompletionResponse> CreateChatCompletion(CreateChatCompletionRequest request)
        {
            var path = $"{BASE_PATH}/conversation";
            var payload = CreatePayload(request);
            Console.WriteLine(payload);
            return await DispatchRequest<CreateChatCompletionResponse>(path, UnityWebRequest.kHttpVerbPOST, payload);
        }
        
        /// <summary>
        ///     Creates a chat completion request as in ChatGPT.
        /// </summary>
        /// <param name="request">See <see cref="CreateChatCompletionRequest"/></param>
        /// <returns>See <see cref="CreateChatCompletionSimple"/></returns>
        public async Task<string> CreateChatCompletionSimple(CreateChatCompletionRequestSimple request)
        {
            var path = $"{BASE_PATH}/unity";
            var payload = CreatePayloadKoala(request);
            return await DispatchSimpleRequest(path, payload);
        }

        
        /// <summary>
        ///     Creates a chat completion request as in KoalaGPT.
        /// </summary>
        /// <param name="request">See <see cref="CreateChatCompletionRequestPrompt"/></param>
        /// <returns>string</returns>
        public async Task<string> CreateChatCompletionSimplePrompt(CreateChatCompletionRequestPrompt request)
        {
            var path = $"{BASE_PATH}/unity";

            var messages = new List<Meta>();
            
            var message = new Meta();
            message.Content = new Content()
            {
                ContentType = "text",   
                InternetAccess = false,
                Parts = request.Messages,
            };

            messages.Add(message);
            
            var req = new CreateChatCompletionRequestSimple
            {
                Model = request.Model,
                Meta = messages,
                Action = "_ask",
                ConversationId = new Guid().ToString(),
                Jailbreak = "default",
            };
            
            
            var payload = CreatePayloadKoala(req);
            return await DispatchSimpleRequest(path, payload);
        }

        
        /// <summary>
        ///     Creates a voice generation request as in KoalaGPT.
        /// </summary>
        /// <param name="request">See <see cref="CreateChatCompletionRequestPrompt"/></param>
        /// <returns>string</returns>
        public async Task<AudioClip> CreateSpeechPrompt(CreateChatCompletionRequestPrompt request)
        {
            var path = $"{BASE_PATH}/unity/voice";

            var messages = new List<Meta>();
            
            var message = new Meta();
            message.Content = new Content()
            {
                ContentType = "text",   
                InternetAccess = false,
                Voice = request.Voice,
                Parts = request.Messages,
            };

            messages.Add(message);
            
            var req = new CreateChatCompletionRequestSimple
            {
                Model = request.Model,
                Meta = messages,
                Action = "_ask",
                ConversationId = new Guid().ToString(),
                Jailbreak = "default",
            };
            
            var payload = CreatePayloadKoala(req);
            return await DispatchAudioRequest(path, payload);
        }

        
        /// <summary>
        ///     Creates a chat completion request as in ChatGPT.
        /// </summary>
        /// <param name="request">See <see cref="CreateChatCompletionRequest"/></param>
        /// <param name="onResponse">Callback function that will be called when stream response is updated.</param>
        /// <param name="onComplete">Callback function that will be called when stream response is completed.</param>
        /// <param name="token">Cancellation token to cancel the request.</param>
        public void CreateChatCompletionAsync(CreateChatCompletionRequest request,
            Action<List<CreateChatCompletionResponse>> onResponse, Action onComplete, CancellationTokenSource token)
        {
            request.Stream = true;
            var path = $"{BASE_PATH}/conversations";
            var payload = CreatePayload(request);

            DispatchRequest(path, UnityWebRequest.kHttpVerbPOST, onResponse, onComplete, token, payload);
        }
        
        // /// <summary>
        // ///     Transcribes audio into the input language.
        // /// </summary>
        // /// <param name="request">See <see cref="CreateAudioTranscriptionsRequest"/></param>
        // /// <returns>See <see cref="CreateAudioResponse"/></returns>
        // public async Task<CreateAudioResponse> CreateAudioTranscription(CreateAudioTranscriptionsRequest request)
        // {
        //     var path = $"{BASE_PATH}/audio/transcriptions";
        //     
        //     var form = new List<IMultipartFormSection>();
        //     if (string.IsNullOrEmpty(request.File))
        //     {
        //         form.AddData(request.FileData, "file", $"audio/{Path.GetExtension(request.File)}");
        //     }
        //     else
        //     {
        //         form.AddFile(request.File, "file", $"audio/{Path.GetExtension(request.File)}");
        //     }
        //     form.AddValue(request.Model, "model");
        //     form.AddValue(request.Prompt, "prompt");
        //     form.AddValue(request.ResponseFormat, "response_format");
        //     form.AddValue(request.Temperature, "temperature");
        //     form.AddValue(request.Language, "language");
        //
        //     return await DispatchRequest<CreateAudioResponse>(path, form);
        // }
        //

        /// <summary>
        ///     Classifies if text violates KoalaGPT's Content Policy
        /// </summary>
        /// <param name="request">See <see cref="CreateModerationRequest" /></param>
        /// <returns>See <see cref="CreateModerationResponse" /></returns>
        public async Task<CreateModerationResponse> CreateModeration(CreateModerationRequest request)
        {
            var path = $"{BASE_PATH}/moderations";
            var payload = CreatePayload(request);
            return await DispatchRequest<CreateModerationResponse>(path, UnityWebRequest.kHttpVerbPOST, payload);
        }
    }
}
