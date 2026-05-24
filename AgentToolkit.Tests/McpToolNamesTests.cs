using AgentToolkit.Mcp;

namespace AgentToolkit.Tests;

public sealed class McpToolNamesTests
{
    [Theory]
    [InlineData("github", "search_issues", "mcp_github__search_issues")]
    [InlineData("GitHub Server", "Search Issues", "mcp_github_server__search_issues")]
    [InlineData("file-system", "read.file", "mcp_file_system__read_file")]
    public void CreateExposedName_prefixes_and_sanitizes_server_and_tool_names(
        string serverName,
        string toolName,
        string expected)
    {
        Assert.Equal(expected, McpToolNames.CreateExposedName(serverName, toolName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("---")]
    public void SanitizeSegment_rejects_empty_segments(string segment)
    {
        Assert.Throws<ArgumentException>(() => McpToolNames.SanitizeSegment(segment));
    }
}
