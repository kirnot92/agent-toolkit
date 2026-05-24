namespace AgentToolkit.Mcp
{
    public sealed class McpServerOptions
    {
        public string Command { get; init; } = "";

        public IReadOnlyList<string> Arguments { get; init; } = [];

        public string? WorkingDirectory { get; init; }

        public IReadOnlyDictionary<string, string?> EnvironmentVariables { get; init; } =
            new Dictionary<string, string?>();

        public bool PrefixToolNames { get; init; } = true;

        public TimeSpan? ShutdownTimeout { get; init; }
    }
}
