namespace AgentToolkit.Tools.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ToolArgumentAttribute : Attribute
    {
        public ToolArgumentAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; }
        public string? Name { get; init; }
    }
}
