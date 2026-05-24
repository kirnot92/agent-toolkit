using AgentToolkit.Definitions;
using AgentToolkit.LLMClients.OpenAI;

namespace AgentToolkit.Tests;

public sealed class MessageTests
{
    [Fact]
    public void Create_rejects_tool_role_without_tool_call_id()
    {
        var exception = Assert.Throws<ArgumentException>(() => Message.Create(MessageRole.Tool, "result"));

        Assert.Contains("Tool messages require a tool call ID.", exception.Message);
    }

    [Fact]
    public void CreateAssistantToolCalls_creates_assistant_message_with_tool_calls()
    {
        var toolCalls = new List<ToolCall>
        {
            new()
            {
                Id = "call-1",
                ToolName = "sum_required",
                ArgumentsJson = """{"left":1,"right":2}"""
            }
        };

        var message = Message.CreateAssistantToolCalls(toolCalls);

        Assert.Equal(MessageRole.Assistant, message.Role);
        Assert.Same(toolCalls, message.ToolCalls);
        Assert.Equal("", message.Content);
    }

    [Fact]
    public void CreateToolCallResult_creates_tool_message_from_result()
    {
        var message = Message.CreateToolCallResult(
            "call-1",
            ToolResult.FromText("done"));

        Assert.Equal(MessageRole.Tool, message.Role);
        Assert.Equal("call-1", message.ToolCallId);
        Assert.Equal("done", message.Content);
        Assert.Empty(message.ToolCalls);
    }

    [Fact]
    public void ToOpenAiMessage_rejects_tool_message_without_tool_call_id()
    {
        var message = new Message
        {
            Role = MessageRole.Tool,
            Content = "done"
        };

        var exception = Assert.Throws<ArgumentException>(() => message.ToOpenAiMessage());

        Assert.Contains("Tool messages require a tool call ID.", exception.Message);
    }
}
