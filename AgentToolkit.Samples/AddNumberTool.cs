using AgentToolkit.Definitions;
using AgentToolkit.Tools.Attributes;

namespace AgentToolkit.Samples
{
    [Tool(
        name: "add_number",
        description: "Adds two numbers and returns the sum.",
        groups: "_SAMPLE_")]
    public sealed class AddNumberTool(
        [ToolArgument("First number to add.")] int left,
        [ToolArgument("Second number to add.")] int right) : ITool
    {
        public Task<ToolResult> Execute(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ToolResult.FromText(Add(left, right).ToString()));
        }

        public static int Add(int left, int right)
        {
            return left + right;
        }
    }
}
