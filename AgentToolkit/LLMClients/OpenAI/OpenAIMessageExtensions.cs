using AgentToolkit.Definitions;
using OpenAI.Chat;

namespace AgentToolkit.LLMClients.OpenAI
{
    public static class OpenAIMessageExtensions
    {
        public static ChatMessage ToOpenAiMessage(this Message message)
        {
            return message.Role switch
            {
                MessageRole.User => new UserChatMessage(message.Content),
                MessageRole.Assistant => CreateAssistantMessage(message),
                MessageRole.Tool => CreateToolMessage(message),
                _ => throw new NotSupportedException($"Unsupported message role: {message.Role}")
            };
        }

        private static AssistantChatMessage CreateAssistantMessage(Message message)
        {
            if (message.ToolCalls.Count == 0)
            {
                return new AssistantChatMessage(message.Content);
            }

            return new AssistantChatMessage(message.ToolCalls.Select(ToOpenAiToolCall));
        }

        private static ChatToolCall ToOpenAiToolCall(ToolCall toolCall)
        {
            return ChatToolCall.CreateFunctionToolCall(
                id: toolCall.Id,
                functionName: toolCall.ToolName,
                functionArguments: BinaryData.FromString(toolCall.ArgumentsJson));
        }

        private static ToolChatMessage CreateToolMessage(Message message)
        {
            if (string.IsNullOrWhiteSpace(message.ToolCallId))
            {
                throw new ArgumentException("Tool messages require a tool call ID.", nameof(message));
            }

            return new ToolChatMessage(message.ToolCallId, message.Content);
        }
    }
}
