using System.Collections.Generic;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace KoalaGPT.Tests
{
    public class TestApiSendPormpt
    {
        private KoalaGPTApi _koalaGptApi = new KoalaGPTApi();
        private List<Meta> _metas = new List<Meta>();
        
        [Test]
        public async Task Create_Text_Completion()
        {
            var part = new Part();
            part.Role = "user";
            part.Content = "Hello!";

            var parts = new List<Part>();
            parts.Add(part);

            var message = new Meta();
            message.Content = new Content()
            {
                ContentType = "text",   
                InternetAccess = false,
                Parts = parts,
            };

            _metas.Add(message);
            
            var req = new CreateChatCompletionRequestSimple
            {
                Model = "gpt4",
                Jailbreak = "default",
                Action = "_ask",
                ConversationId = "8123812",
                Meta = _metas
            };
            var res = await _koalaGptApi.CreateChatCompletionSimple(req);
            Assert.NotNull(res);
        }
        
        [Test]
        public async Task Create_Speech()
        {
            var messages = new List<Part>();
            var message = new Part();
            message.Role = "user";
            message.Content = "Hello";

            messages.Add(message);

            var request = new CreateChatCompletionRequestPrompt();
            request.Messages = messages;
            request.Model = "gpt4";
            
            var res = await _koalaGptApi.CreateSpeechPrompt(request);
            Assert.NotNull(res);
        }
    }
}