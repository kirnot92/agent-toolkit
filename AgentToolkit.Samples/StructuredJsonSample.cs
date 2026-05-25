using AgentToolkit.Definitions;
using AgentToolkit.LLMClients;
using AgentToolkit.LLMClients.OpenAI;

namespace AgentToolkit.Samples
{
    public static class StructuredJsonSample
    {
        public static async Task Run(string apiKey, CancellationToken cancellationToken = default)
        {
            var llm = new ChatGPTClient(apiKey);
            var messages = new List<Message>
            {
                Message.Create(
                    MessageRole.User,
                    """Summarize the request "Schedule a design review for Friday afternoon".""")
            };

            var response = await llm.Call(
                messages,
                [],
                JsonSchemaBuilder.Object()
                    .AddProperty(
                        JsonSchemaType.String,
                        "title",
                        "A short title for the scheduling request.")
                    .AddProperty(
                        JsonSchemaType.String,
                        "summary",
                        "A concise summary of what needs to be scheduled.")
                    .AddEnumProperty(
                        "priority",
                        ["low", "medium", "high"],
                        "The urgency of the scheduling request.")
                    .AddObjectProperty(
                        "schedule",
                        schedule => schedule
                            .AddProperty(
                                JsonSchemaType.String,
                                "day",
                                "The requested day.")
                            .AddProperty(
                                JsonSchemaType.String,
                                "timeOfDay",
                                "The requested time of day."),
                        "The requested schedule details.")
                    .AddArrayProperty(
                        JsonSchemaType.String,
                        "tags",
                        "Short labels that categorize the request.")
                    .Build());

            Console.WriteLine(response.Content);
        }
    }
}
