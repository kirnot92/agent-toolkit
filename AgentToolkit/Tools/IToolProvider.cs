using AgentToolkit.Definitions;

namespace AgentToolkit.Tools
{
    public interface IToolProvider
    {
        Task<IReadOnlyList<ToolDefinition>> GetTools(CancellationToken cancellationToken = default);

        Task<ToolResult> Execute(ToolCall toolCall, CancellationToken cancellationToken = default);
    }
}
