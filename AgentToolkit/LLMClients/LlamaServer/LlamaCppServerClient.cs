using System.ClientModel;
using AgentToolkit.Definitions;
using AgentToolkit.LLMClients.OpenAI;
using OpenAI;
using OpenAI.Chat;

namespace AgentToolkit.LLMClients.LlamaServer
{
    public sealed class LlamaCppServerClient : ILLMClient
    {
        private readonly OpenAIChatCompletionClient client;

        public LlamaCppServerClient(string endpoint, string model, string apiKey = "unused")
        {
            var chatClient = new ChatClient(
                model: model,
                credential: new ApiKeyCredential(apiKey),
                options: new OpenAIClientOptions
                {
                    Endpoint = new Uri(endpoint)
                });

            this.client = new OpenAIChatCompletionClient(chatClient);
        }

        public Task<Message> Call(List<Message> messages, IReadOnlyList<ToolDefinition> tools)
        {
            return this.client.Call(messages, tools);
        }
    }
}
