using AgentToolkit.Definitions;

namespace AgentToolkit.Tools
{
    public sealed class ToolManager
    {
        private readonly List<IToolProvider> providers = new();
        private Dictionary<string, IToolProvider>? toolRoutes;
        private bool debugMode;

        private ToolManager()
        {
        }

        public static ToolManager Create()
        {
            return new ToolManager();
        }

        public ToolManager AddProvider(IToolProvider provider)
        {
            ArgumentNullException.ThrowIfNull(provider);

            this.providers.Add(provider);
            this.toolRoutes = null;
            return this;
        }

        public ToolManager AddLocalTools(params string[] groups)
        {
            return this.AddProvider(new LocalToolProvider(groups));
        }

        public ToolManager EnableDebugLog(bool enabled = true)
        {
            this.debugMode = enabled;
            return this;
        }

        public async Task<IReadOnlyList<ToolDefinition>> GetTools(
            CancellationToken cancellationToken = default)
        {
            var tools = new List<ToolDefinition>();
            var routes = new Dictionary<string, IToolProvider>(StringComparer.OrdinalIgnoreCase);

            foreach (var provider in this.providers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var providerTools = await provider.GetTools(cancellationToken);
                foreach (var tool in providerTools)
                {
                    if (string.IsNullOrWhiteSpace(tool.Name))
                    {
                        throw new InvalidOperationException("Tool name cannot be empty.");
                    }

                    if (!routes.TryAdd(tool.Name, provider))
                    {
                        throw new InvalidOperationException($"Tool '{tool.Name}' is already registered.");
                    }

                    tools.Add(tool);
                }
            }

            this.toolRoutes = routes;
            return tools;
        }

        public async Task<ToolResult> Execute(
            ToolCall toolCall,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(toolCall);

            var routes = this.toolRoutes;
            if (routes is null || !routes.ContainsKey(toolCall.ToolName))
            {
                await this.GetTools(cancellationToken);
                routes = this.toolRoutes;
            }

            if (routes is null || !routes.TryGetValue(toolCall.ToolName, out var provider))
            {
                throw new InvalidOperationException($"Tool '{toolCall.ToolName}' is not registered.");
            }

            if (this.debugMode)
            {
                Console.WriteLine($"{toolCall.ToolName} {toolCall.ArgumentsJson}");
            }

            return await provider.Execute(toolCall, cancellationToken);
        }
    }
}
