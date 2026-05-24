using AgentToolkit.Definitions;

namespace AgentToolkit.Tools
{
    public static class ToolManager
    {
        private static bool debugMode;

        public static void EnableDebugLog(bool enabled = true)
        {
            debugMode = enabled;
        }

        public static IReadOnlyList<ToolDefinition> GetTools()
        {
            return ToolParser.ToolDefinitions.ToList();
        }

        public static IReadOnlyList<ToolDefinition> GetTools(params string[] groups)
        {
            if (groups.Length == 0)
            {
                return GetTools();
            }

            var groupSet = new HashSet<string>(groups, StringComparer.OrdinalIgnoreCase);
            return ToolParser.ToolDefinitions
                .Where(tool => tool.Groups.Any(groupSet.Contains))
                .ToList();
        }

        public static async Task<ToolResult> Execute(
            ToolCall toolCall,
            CancellationToken cancellationToken = default)
        {
            var tool = ToolParser.CreateTool(toolCall);
            if (debugMode)
            {
                Console.WriteLine(ToolParser.CreateDebugText(toolCall));
            }

            return await tool.Execute(cancellationToken);
        }
    }
}
