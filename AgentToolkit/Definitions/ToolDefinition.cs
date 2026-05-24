namespace AgentToolkit.Definitions
{
    public sealed class ToolDefinition
    {
        public string Name { get; init; } = "";
        public string Description { get; init; } = "";
        public List<ToolArgumentDefinition> Arguments { get; init; } = new();
        public List<string> Groups { get; init; } = new();
    }
}
