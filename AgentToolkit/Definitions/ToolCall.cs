namespace AgentToolkit.Definitions
{
    public class ToolCall
    {
        public string Id { get; init; } = "";
        public string ToolName { get; init; } = "";
        public string ArgumentsJson { get; init; } = "{}";
    }
}
