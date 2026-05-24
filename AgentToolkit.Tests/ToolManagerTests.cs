using System.Text.Json;
using AgentToolkit.Definitions;
using AgentToolkit.Tools;
using AgentToolkit.Tools.Attributes;

namespace AgentToolkit.Tests;

public sealed class ToolManagerTests
{
    [Fact]
    public async Task GetTools_creates_definitions_from_tool_attributes_and_constructor_arguments()
    {
        var tools = await ToolManager.Create()
            .AddLocalTools()
            .GetTools();

        var tool = tools
            .Single(tool => tool.Name == "default_named");

        Assert.Equal("Default named tool.", tool.Description);
        Assert.Contains("metadata", tool.Groups);

        Assert.Collection(
            tool.Arguments,
            text =>
            {
                Assert.Equal("text", text.Name);
                Assert.Equal("Required text.", text.Description);
                Assert.Equal(typeof(string), text.ArgumentType);
                Assert.True(text.IsRequired);
            },
            count =>
            {
                Assert.Equal("count", count.Name);
                Assert.Equal("Optional count.", count.Description);
                Assert.Equal(typeof(int), count.ArgumentType);
                Assert.False(count.IsRequired);
            });
    }

    [Fact]
    public async Task GetTools_without_groups_returns_all_registered_tools()
    {
        var tools = await ToolManager.Create()
            .AddLocalTools()
            .GetTools();

        Assert.Contains(tools, tool => tool.Name == "default_named");
        Assert.Contains(tools, tool => tool.Name == "visible_group_tool");
    }

    [Fact]
    public async Task GetTools_with_group_returns_matching_tools_case_insensitively()
    {
        var tools = await ToolManager.Create()
            .AddLocalTools("VISIBLE")
            .GetTools();

        var tool = Assert.Single(tools, tool => tool.Name == "visible_group_tool");
        Assert.Contains("visible", tool.Groups);
    }

    [Fact]
    public async Task Execute_binds_arguments_case_insensitively()
    {
        var result = await ToolManager.Create()
            .AddLocalTools()
            .Execute(new ToolCall
            {
                ToolName = "sum_required",
                ArgumentsJson = """{"LEFT":2,"Right":5}"""
            });

        Assert.Equal("7", result.Result);
    }

    [Fact]
    public async Task Execute_uses_default_value_for_optional_argument()
    {
        var result = await ToolManager.Create()
            .AddLocalTools()
            .Execute(new ToolCall
            {
                ToolName = "default_named",
                ArgumentsJson = """{"text":"repeat"}"""
            });

        Assert.Equal("repeat:3", result.Result);
    }

    [Fact]
    public async Task Execute_throws_when_required_argument_is_missing()
    {
        var manager = ToolManager.Create()
            .AddLocalTools();

        var exception = await Assert.ThrowsAsync<JsonException>(() => manager.Execute(new ToolCall
        {
            ToolName = "sum_required",
            ArgumentsJson = """{"left":2}"""
        }));

        Assert.Contains("Missing required tool argument 'right'", exception.Message);
    }

    [Fact]
    public async Task Execute_throws_when_arguments_json_is_not_an_object()
    {
        var manager = ToolManager.Create()
            .AddLocalTools();

        var exception = await Assert.ThrowsAsync<JsonException>(() => manager.Execute(new ToolCall
        {
            ToolName = "sum_required",
            ArgumentsJson = "[]"
        }));

        Assert.Contains("arguments must be a JSON object", exception.Message);
    }

    [Fact]
    public async Task Execute_throws_when_tool_is_not_registered()
    {
        var manager = ToolManager.Create()
            .AddLocalTools();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => manager.Execute(new ToolCall
        {
            ToolName = "missing_tool",
            ArgumentsJson = "{}"
        }));

        Assert.Contains("Tool 'missing_tool' is not registered.", exception.Message);
    }

    [Fact]
    public async Task Execute_routes_to_registered_provider()
    {
        var manager = ToolManager.Create()
            .AddProvider(new StubToolProvider("external_tool", "external"));

        var result = await manager.Execute(new ToolCall
        {
            ToolName = "external_tool",
            ArgumentsJson = "{}"
        });

        Assert.Equal("external", result.Result);
    }

    [Fact]
    public async Task GetTools_throws_when_tool_names_are_duplicated()
    {
        var manager = ToolManager.Create()
            .AddProvider(new StubToolProvider("duplicate_tool", "first"))
            .AddProvider(new StubToolProvider("DUPLICATE_TOOL", "second"));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await manager.GetTools());

        Assert.Contains("Tool 'DUPLICATE_TOOL' is already registered.", exception.Message);
    }

    private sealed class StubToolProvider(string toolName, string result) : IToolProvider
    {
        public Task<IReadOnlyList<ToolDefinition>> GetTools(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ToolDefinition> tools =
            [
                new ToolDefinition
                {
                    Name = toolName,
                    Description = "Stub tool."
                }
            ];

            return Task.FromResult(tools);
        }

        public Task<ToolResult> Execute(ToolCall toolCall, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ToolResult.FromText(result));
        }
    }
}

[Tool(
    name: "default_named",
    description: "Default named tool.",
    groups: "metadata")]
public sealed class DefaultNamedTool(
    [ToolArgument("Required text.")] string text,
    [ToolArgument("Optional count.")] int count = 3) : ITool
{
    public Task<ToolResult> Execute(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ToolResult.FromText($"{text}:{count}"));
    }
}

[Tool(
    name: "sum_required",
    description: "Adds two required numbers.")]
public sealed class SumRequiredTool(
    [ToolArgument("Left number.")] int left,
    [ToolArgument("Right number.")] int right) : ITool
{
    public Task<ToolResult> Execute(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ToolResult.FromText((left + right).ToString()));
    }
}

[Tool(
    name: "visible_group_tool",
    description: "Visible grouped tool.",
    groups: "visible")]
public sealed class VisibleGroupTool : ITool
{
    public Task<ToolResult> Execute(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ToolResult.FromText("visible"));
    }
}
