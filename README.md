# AgentToolkit

AgentToolkit is a lightweight .NET toolkit for building tool-calling workflows around chat-based LLM clients.

It provides message and tool definitions, attribute-based local tool discovery, a provider-based tool manager, OpenAI-compatible chat client adapters, and an optional MCP integration package.

## Projects

- `AgentToolkit`: core library for local tools, messages, tool routing, and LLM client adapters.
- `AgentToolkit.Mcp`: optional MCP tool provider integration.
- `AgentToolkit.Samples`: local samples for core and MCP tool flows.
- `AgentToolkit.Tests`: unit tests for message handling, tool routing, and MCP naming.

## Status

This project is early-stage and not published as a NuGet package yet. Use project references when trying it locally.

## Core Usage

Local tools are regular `ITool` implementations annotated with `ToolAttribute` and constructor `ToolArgumentAttribute` metadata.

```csharp
var toolManager = ToolManager.Create()
    .AddLocalTools();

var tools = await toolManager.GetTools(cancellationToken);
var response = await llm.Call(messages, tools);

foreach (var toolCall in response.ToolCalls)
{
    var toolResult = await toolManager.Execute(toolCall, cancellationToken);
    messages.Add(Message.CreateToolCallResult(toolCall.Id, toolResult));
}
```

To ask the model for structured JSON response content, pass a JSON schema. The assistant response remains a regular
`Message`; its raw JSON is available in `Content`.

```csharp
var response = await llm.Call(
    messages,
    tools,
    JsonSchemaBuilder.Object()
        .AddProperty(JsonSchemaType.String, "answer", "The answer to the user request.")
        .Build());

var json = response.Content;
```

## MCP Usage

MCP support lives in `AgentToolkit.Mcp` so the core package does not depend on MCP transports or lifecycle management.

```csharp
using AgentToolkit.Mcp;

await using var toolManager = ToolManager.Create()
    .AddLocalTools()
    .AddMcpServer("filesystem", new McpServerOptions
    {
        Command = "npx",
        Arguments =
        [
            "-y",
            "@modelcontextprotocol/server-filesystem",
            workspacePath
        ]
    });
```

MCP tools are exposed with server-prefixed names by default, such as `mcp_filesystem__read_file`, while calls are routed back to the original MCP tool names internally.

## Development

```powershell
dotnet build .\AgentToolkit.slnx --configuration Release
dotnet test .\AgentToolkit.slnx --configuration Release --no-build
dotnet pack .\AgentToolkit\AgentToolkit.csproj --configuration Release --no-build
dotnet pack .\AgentToolkit.Mcp\AgentToolkit.Mcp.csproj --configuration Release --no-build
```

The solution currently targets `net10.0`.
