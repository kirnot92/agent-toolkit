namespace AgentToolkit.Tools.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ToolAttribute : Attribute
    {
        public ToolAttribute(string name, string description, params string[] groups)
        {
            Name = name;
            Description = description;
            Groups = groups ?? [];
        }

        public string Name { get; }
        public string Description { get; }
        public string[] Groups { get; }
    }
}
