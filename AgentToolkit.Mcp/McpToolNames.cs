using System.Text;

namespace AgentToolkit.Mcp
{
    public static class McpToolNames
    {
        public static string CreateExposedName(string serverName, string toolName)
        {
            return $"mcp_{SanitizeSegment(serverName)}__{SanitizeSegment(toolName)}";
        }

        public static string SanitizeSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Tool name segment cannot be empty.", nameof(value));
            }

            var builder = new StringBuilder(value.Length);
            var lastWasSeparator = false;

            foreach (var character in value.Trim())
            {
                if (IsAsciiLetterOrDigit(character))
                {
                    builder.Append(char.ToLowerInvariant(character));
                    lastWasSeparator = false;
                }
                else if (!lastWasSeparator)
                {
                    builder.Append('_');
                    lastWasSeparator = true;
                }
            }

            var sanitized = builder.ToString().Trim('_');
            if (sanitized.Length == 0)
            {
                throw new ArgumentException("Tool name segment must contain letters or digits.", nameof(value));
            }

            return sanitized;
        }

        private static bool IsAsciiLetterOrDigit(char character)
        {
            return character is >= 'a' and <= 'z'
                or >= 'A' and <= 'Z'
                or >= '0' and <= '9';
        }
    }
}
