using AgentToolkit.Definitions;
using AgentToolkit.LLMClients.OpenAI;
using AgentToolkit.Mcp;
using AgentToolkit.Tools;

namespace AgentToolkit.Samples
{
    public static class McpFilesystemSample
    {
        public static async Task Run(
            string apiKey,
            string workspacePath,
            CancellationToken cancellationToken = default)
        {
            var llm = new ChatGPTClient(apiKey);
            await using var toolManager = ToolManager.Create()
                .AddLocalTools("_SAMPLE_")
                .AddMcpServer("filesystem", new McpServerOptions
                {
                    Command = "npx",
                    Arguments =
                    [
                        "-y",
                        "@modelcontextprotocol/server-filesystem",
                        workspacePath
                    ]
                });

            var tools = await toolManager.GetTools(cancellationToken);
            var messages = new List<Message>
            {
                Message.Create(
                    MessageRole.User,
                    "List the files in the workspace and add 5 and 10.")
            };

            for (var step = 0; step < 10; step++)
            {
                var response = await llm.Call(messages, tools);
                messages.Add(response);

                if (response.ToolCalls.Count == 0)
                {
                    Console.WriteLine(response.Content);
                    return;
                }

                foreach (var toolCall in response.ToolCalls)
                {
                    var toolResult = await toolManager.Execute(toolCall, cancellationToken);
                    messages.Add(Message.CreateToolCallResult(toolCall.Id, toolResult));
                }
            }
        }
    }
}
