using System.Text.Json;
using AgentToolkit.Definitions;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace AgentToolkit.Tools
{
    public sealed class McpToolProvider : IToolProvider, IAsyncDisposable
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string serverName;
        private readonly McpServerOptions options;
        private readonly SemaphoreSlim clientLock = new(1, 1);
        private McpClient? client;
        private Dictionary<string, McpToolRoute>? toolRoutes;

        public McpToolProvider(string serverName, McpServerOptions options)
        {
            if (string.IsNullOrWhiteSpace(serverName))
            {
                throw new ArgumentException("MCP server name cannot be empty.", nameof(serverName));
            }

            ArgumentNullException.ThrowIfNull(options);

            this.serverName = serverName;
            this.options = options;
        }

        public async Task<IReadOnlyList<ToolDefinition>> GetTools(CancellationToken cancellationToken = default)
        {
            var mcpClient = await this.GetClient(cancellationToken);
            var mcpTools = await mcpClient.ListToolsAsync(cancellationToken: cancellationToken);
            var toolDefinitions = new List<ToolDefinition>(mcpTools.Count);
            var routes = new Dictionary<string, McpToolRoute>(StringComparer.OrdinalIgnoreCase);

            foreach (var tool in mcpTools)
            {
                var exposedName = this.CreateExposedName(tool.Name);
                if (!routes.TryAdd(exposedName, new McpToolRoute(tool.Name)))
                {
                    throw new InvalidOperationException(
                        $"MCP server '{this.serverName}' exposes duplicate tool name '{exposedName}'.");
                }

                toolDefinitions.Add(new ToolDefinition
                {
                    Name = exposedName,
                    Description = tool.Description ?? "",
                    InputJsonSchema = tool.JsonSchema.GetRawText()
                });
            }

            this.toolRoutes = routes;
            return toolDefinitions;
        }

        public async Task<ToolResult> Execute(
            ToolCall toolCall,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(toolCall);

            var routes = this.toolRoutes;
            if (routes is null || !routes.TryGetValue(toolCall.ToolName, out var route))
            {
                await this.GetTools(cancellationToken);
                routes = this.toolRoutes;
            }

            if (routes is null || !routes.TryGetValue(toolCall.ToolName, out route))
            {
                throw new InvalidOperationException($"Tool '{toolCall.ToolName}' is not registered.");
            }

            var mcpClient = await this.GetClient(cancellationToken);
            var result = await mcpClient.CallToolAsync(
                route.OriginalName,
                ParseArguments(toolCall),
                cancellationToken: cancellationToken);

            return ToolResult.FromText(FormatResult(result));
        }

        public async ValueTask DisposeAsync()
        {
            var existingClient = this.client;
            if (existingClient is not null)
            {
                await existingClient.DisposeAsync();
            }

            this.clientLock.Dispose();
        }

        private async Task<McpClient> GetClient(CancellationToken cancellationToken)
        {
            if (this.client is not null)
            {
                return this.client;
            }

            await this.clientLock.WaitAsync(cancellationToken);
            try
            {
                if (this.client is not null)
                {
                    return this.client;
                }

                var transportOptions = new StdioClientTransportOptions
                {
                    Name = this.serverName,
                    Command = this.options.Command,
                    Arguments = this.options.Arguments.ToArray(),
                    WorkingDirectory = this.options.WorkingDirectory,
                    EnvironmentVariables = this.options.EnvironmentVariables.ToDictionary()
                };

                if (this.options.ShutdownTimeout is { } shutdownTimeout)
                {
                    transportOptions.ShutdownTimeout = shutdownTimeout;
                }

                var transport = new StdioClientTransport(transportOptions);

                this.client = await McpClient.CreateAsync(transport, cancellationToken: cancellationToken);
                return this.client;
            }
            finally
            {
                this.clientLock.Release();
            }
        }

        private string CreateExposedName(string toolName)
        {
            return this.options.PrefixToolNames
                ? McpToolNames.CreateExposedName(this.serverName, toolName)
                : McpToolNames.SanitizeSegment(toolName);
        }

        private static Dictionary<string, object?> ParseArguments(ToolCall toolCall)
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(toolCall.ArgumentsJson)
                ? "{}"
                : toolCall.ArgumentsJson);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException($"Tool '{toolCall.ToolName}' arguments must be a JSON object.");
            }

            return document.RootElement
                .EnumerateObject()
                .ToDictionary(
                    property => property.Name,
                    property => (object?)property.Value.Clone(),
                    StringComparer.OrdinalIgnoreCase);
        }

        private static string FormatResult(CallToolResult result)
        {
            var text = string.Join(
                Environment.NewLine,
                result.Content
                    .OfType<TextContentBlock>()
                    .Select(content => content.Text)
                    .Where(content => !string.IsNullOrWhiteSpace(content)));

            if (!string.IsNullOrWhiteSpace(text))
            {
                return result.IsError == true ? $"Error: {text}" : text;
            }

            return JsonSerializer.Serialize(result, JsonOptions);
        }

        private sealed record McpToolRoute(string OriginalName);
    }
}
