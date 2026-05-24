using AgentToolkit.Definitions;
using AgentToolkit.LLMClients;
using AgentToolkit.LLMClients.OpenAI;
using AgentToolkit.Tools;

namespace AgentToolkit.Samples
{
    public static class AddNumberSample
    {
        public static async Task Run(string apiKey, CancellationToken cancellationToken = default)
        {
            var llm = new ChatGPTClient(apiKey);
            var tools = ToolManager.GetTools("_SAMPLE_");

            var step = 0;
            var maxStep = 10;
            var messages = new List<Message>();

            var rawMessage = "Add 5 and 10 and 5";
            Console.WriteLine(rawMessage);
            messages.Add(Message.Create(MessageRole.User, rawMessage));

            ToolManager.EnableDebugLog();

            while (true)
            {
                if (step++ >= maxStep)
                {
                    break;
                }

                var response = await llm.Call(messages, tools);
                messages.Add(response);

                if (response.ToolCalls.Count == 0)
                {
                    Console.WriteLine(response.Content);
                    break;
                }

                foreach (var toolCall in response.ToolCalls)
                {
                    var toolResult = await ToolManager.Execute(toolCall, cancellationToken);

                    messages.Add(Message.CreateToolCallResult(toolCall.Id, toolResult));
                }
            }
        }
    }
}
