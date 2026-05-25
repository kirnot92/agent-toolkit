using AgentToolkit.Definitions;
using System.Text.Json.Nodes;
using OpenAI;

namespace AgentToolkit.LLMClients.OpenAI
{
    public class ChatGPTClient : ILLMClient
    {
        private readonly OpenAIChatCompletionClient client;

        public ChatGPTClient(string apiKey)
        {
            var openAiClient = new OpenAIClient(apiKey);
            this.client = new OpenAIChatCompletionClient(openAiClient.GetChatClient("gpt-5.5")); // todo model config
        }

        public Task<Message> Call(
            List<Message> messages,
            IReadOnlyList<ToolDefinition> tools,
            JsonObject? jsonSchema = null)
        {
            return this.client.Call(messages, tools, jsonSchema);
        }
    }
}
