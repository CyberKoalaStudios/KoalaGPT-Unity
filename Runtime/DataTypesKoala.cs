#nullable enable
using System.Collections.Generic;
using Newtonsoft.Json;

namespace KoalaGPT
{
    public struct Part
    {
        public string Content { get; set; }
        public string Role { get; set; }
    }

    public struct Content
    {
        public string ContentType { get; set; }
        public List<string>? Conversation { get; set; }
        public bool InternetAccess { get; set; }
        public string? Voice { get; set; }
        public List<Part> Parts { get; set; }
    }

    public struct Meta
    {
        public int Id { get; set; }
        public Content Content { get; set; }
    }

    public sealed class CreateChatCompletionRequestSimple
    {
        public string Model { get; set; }
        public string Action { get; set; }
        public List<Meta> Meta { get; set; }
        public string ConversationId { get; set; }
        public string Jailbreak { get; set; }
    }
    
    public sealed class CreateChatCompletionRequestPrompt
    {
        public string Model { get; set; }
        public List<Part> Messages { get; set; }
    }
    
    public sealed class CreateAudioCompletionRequest
    {
        public string Model { get; set; }
        public List<Part> Messages { get; set; }
        public string Voice { get; set; }
    }
}