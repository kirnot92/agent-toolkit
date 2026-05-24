namespace AgentToolkit.Definitions
{
    public class Message
    {
        public MessageRole Role { get; init; }
        public string Content { get; init; } = "";
        public string? ToolCallId { get; init; }
        public List<ToolCall> ToolCalls { get; init; } = new();

        public static Message Create(MessageRole role, string content)
        {
            if (role == MessageRole.Tool)
            {
                throw new ArgumentException("Tool messages require a tool call ID.", nameof(role));
            }

            return new Message
            {
                Role = role,
                Content = content
            };
        }

        public static Message CreateAssistantToolCalls(List<ToolCall> toolCalls)
        {
            return new Message
            {
                Role = MessageRole.Assistant,
                ToolCalls = toolCalls
            };
        }

        public static Message CreateToolCallResult(string toolCallId, ToolResult toolResult)
        {
            return new Message
            {
                Role = MessageRole.Tool,
                Content = toolResult.Result,
                ToolCallId = toolCallId
            };
        }
    }
}
