namespace AgentToolkit.Definitions
{
    public sealed class ToolArgumentDefinition
    {
        public string Name { get; init; } = "";
        public string Description { get; init; } = "";
        public Type ArgumentType { get; init; } = typeof(string);
        public bool IsRequired { get; init; }
    }
}
