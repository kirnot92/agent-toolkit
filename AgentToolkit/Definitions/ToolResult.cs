namespace AgentToolkit.Definitions
{
    public class ToolResult
    {
        public string Result { get; internal set; } = "";

        public static ToolResult FromText(string text)
        {
            return new ToolResult
            {
                Result = text
            };
        }
    }
}
