using System.Text.Json;
using System.Text.Json.Nodes;
using AgentToolkit.Definitions;
using OpenAI.Chat;

namespace AgentToolkit.LLMClients.OpenAI
{
    internal sealed class OpenAIChatCompletionClient : ILLMClient
    {
        private readonly ChatClient chatClient;

        public OpenAIChatCompletionClient(ChatClient chatClient)
        {
            this.chatClient = chatClient;
        }

        public async Task<Message> Call(
            List<Message> messages,
            IReadOnlyList<ToolDefinition> tools,
            JsonObject? jsonSchema = null)
        {
            var openAiMessages = messages
                .Select(message => message.ToOpenAiMessage())
                .ToList();

            var chatOptions = CreateOptions(tools, jsonSchema);

            var completion = await this.chatClient.CompleteChatAsync(openAiMessages, chatOptions);
            var toolCalls = completion.Value.ToolCalls
                .Select(ToToolCall)
                .ToList();

            if (toolCalls.Count > 0)
            {
                return Message.CreateAssistantToolCalls(toolCalls);
            }

            return Message.Create(
                MessageRole.Assistant,
                string.Concat(completion.Value.Content.Select(content => content.Text)));
        }

        private static ChatCompletionOptions CreateOptions(
            IReadOnlyList<ToolDefinition> tools,
            JsonObject? jsonSchema)
        {
            var chatOptions = new ChatCompletionOptions
            {
                ResponseFormat = ToChatResponseFormat(jsonSchema)
            };

            foreach (var tool in tools)
            {
                chatOptions.Tools.Add(ToChatTool(tool));
            }

            return chatOptions;
        }

        private static ChatTool ToChatTool(ToolDefinition tool)
        {
            var functionParameters = !string.IsNullOrWhiteSpace(tool.InputJsonSchema)
                ? BinaryData.FromString(tool.InputJsonSchema)
                : BinaryData.FromString(JsonSerializer.Serialize(CreateSchema(tool)));

            return ChatTool.CreateFunctionTool(
                functionName: tool.Name,
                functionDescription: tool.Description,
                functionParameters: functionParameters);
        }

        private static object CreateSchema(ToolDefinition tool)
        {
            var schema = new
            {
                type = "object",
                properties = tool.Arguments.ToDictionary(
                    argument => argument.Name,
                    argument => new
                    {
                        type = ToJsonSchemaType(argument.ArgumentType),
                        description = argument.Description
                    }),
                required = tool.Arguments
                    .Where(argument => argument.IsRequired)
                    .Select(argument => argument.Name)
                    .ToArray(),
                additionalProperties = false
            };

            return schema;
        }

        private static ToolCall ToToolCall(ChatToolCall toolCall)
        {
            return new ToolCall
            {
                Id = toolCall.Id,
                ToolName = toolCall.FunctionName,
                ArgumentsJson = toolCall.FunctionArguments.ToString()
            };
        }

        private static ChatResponseFormat? ToChatResponseFormat(JsonObject? jsonSchema)
        {
            if (jsonSchema is null)
            {
                return null;
            }

            return ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "response",
                jsonSchema: BinaryData.FromString(jsonSchema.ToJsonString()),
                jsonSchemaIsStrict: true);
        }

        private static string ToJsonSchemaType(Type type)
        {
            if (type == typeof(string))
            {
                return "string";
            }

            if (type == typeof(int) || type == typeof(long))
            {
                return "integer";
            }

            if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
            {
                return "number";
            }

            if (type == typeof(bool))
            {
                return "boolean";
            }

            throw new NotSupportedException($"Unsupported tool argument type: {type.Name}");
        }
    }
}
