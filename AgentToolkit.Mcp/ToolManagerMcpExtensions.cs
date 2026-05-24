using AgentToolkit.Tools;

namespace AgentToolkit.Mcp
{
    public static class ToolManagerMcpExtensions
    {
        public static ToolManager AddMcpServer(
            this ToolManager toolManager,
            string serverName,
            McpServerOptions options)
        {
            ArgumentNullException.ThrowIfNull(toolManager);
            ArgumentNullException.ThrowIfNull(options);

            return toolManager.AddProvider(new McpToolProvider(serverName, options));
        }
    }
}
