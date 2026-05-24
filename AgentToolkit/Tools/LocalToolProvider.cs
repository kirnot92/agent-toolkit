using AgentToolkit.Definitions;

namespace AgentToolkit.Tools
{
    public sealed class LocalToolProvider : IToolProvider
    {
        private readonly string[] groups;

        public LocalToolProvider(params string[] groups)
        {
            this.groups = groups ?? [];
        }

        public Task<IReadOnlyList<ToolDefinition>> GetTools(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (this.groups.Length == 0)
            {
                return Task.FromResult(ToolParser.ToolDefinitions);
            }

            var groupSet = new HashSet<string>(this.groups, StringComparer.OrdinalIgnoreCase);
            IReadOnlyList<ToolDefinition> tools = ToolParser.ToolDefinitions
                .Where(tool => tool.Groups.Any(groupSet.Contains))
                .ToList();

            return Task.FromResult(tools);
        }

        public async Task<ToolResult> Execute(
            ToolCall toolCall,
            CancellationToken cancellationToken = default)
        {
            var tool = ToolParser.CreateTool(toolCall);
            return await tool.Execute(cancellationToken);
        }
    }
}
