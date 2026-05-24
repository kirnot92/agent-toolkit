namespace AgentToolkit.Definitions
{
    public interface ITool
    {
        Task<ToolResult> Execute(CancellationToken cancellationToken = default);
    }
}
