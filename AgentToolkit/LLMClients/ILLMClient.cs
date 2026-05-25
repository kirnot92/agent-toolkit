using System.Text.Json.Nodes;
using AgentToolkit.Definitions;

namespace AgentToolkit.LLMClients
{
    public interface ILLMClient
    {
        Task<Message> Call(
            List<Message> messages,
            IReadOnlyList<ToolDefinition> tools,
            JsonObject? jsonSchema = null);
    }
}
