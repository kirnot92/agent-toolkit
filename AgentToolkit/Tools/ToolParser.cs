using System.Reflection;
using System.Text.Json;
using AgentToolkit.Definitions;
using AgentToolkit.Tools.Attributes;

namespace AgentToolkit.Tools
{
    internal static class ToolParser
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static IReadOnlyDictionary<string, ToolDescriptor> toolDescriptors = new Dictionary<string, ToolDescriptor>();
        private static IReadOnlyList<ToolDefinition> toolDefinitions = [];

        static ToolParser()
        {
            Reload();
        }

        public static IReadOnlyList<ToolDefinition> ToolDefinitions => toolDefinitions;

        private static void Reload()
        {
            var descriptors = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.GetCustomAttribute<ToolAttribute>() is not null)
                .Select(CreateDescriptor)
                .ToList();

            toolDescriptors = descriptors.ToDictionary(
                descriptor => descriptor.Definition.Name,
                StringComparer.OrdinalIgnoreCase);

            toolDefinitions = descriptors
                .Select(descriptor => descriptor.Definition)
                .ToList();
        }

        public static ITool CreateTool(ToolCall toolCall)
        {
            if (!toolDescriptors.TryGetValue(toolCall.ToolName, out var descriptor))
            {
                throw new InvalidOperationException($"Tool '{toolCall.ToolName}' is not registered.");
            }

            var arguments = BindArguments(toolCall, descriptor);
            var tool = (ITool?)Activator.CreateInstance(descriptor.ToolType, arguments);
            if (tool is null)
            {
                throw new InvalidOperationException($"Failed to create tool '{toolCall.ToolName}'.");
            }

            return tool;
        }

        private static ToolDescriptor CreateDescriptor(Type toolType)
        {
            if (!typeof(ITool).IsAssignableFrom(toolType))
            {
                throw new InvalidOperationException($"Tool type '{toolType.FullName}' must implement {nameof(ITool)}.");
            }

            var declaration = toolType.GetCustomAttribute<ToolAttribute>()
                ?? throw new InvalidOperationException($"Tool type '{toolType.FullName}' is missing {nameof(ToolAttribute)}.");

            var constructor = SelectConstructor(toolType);
            var parameters = constructor.GetParameters()
                .Select(CreateParameterDescriptor)
                .ToList();

            var definition = new ToolDefinition
            {
                Name = declaration.Name,
                Description = declaration.Description,
                Arguments = parameters
                    .Select(parameter => parameter.Definition)
                    .ToList(),
                Groups = declaration.Groups.ToList()
            };

            return new ToolDescriptor(toolType, parameters, definition);
        }

        private static ConstructorInfo SelectConstructor(Type toolType)
        {
            var constructors = toolType.GetConstructors();
            if (constructors.Length == 0)
            {
                throw new InvalidOperationException($"Tool type '{toolType.FullName}' must have a public constructor.");
            }

            var attributedConstructors = constructors
                .Where(constructor => constructor.GetParameters()
                    .Any(parameter => parameter.GetCustomAttribute<ToolArgumentAttribute>() is not null))
                .ToList();

            if (attributedConstructors.Count > 1)
            {
                throw new InvalidOperationException($"Tool type '{toolType.FullName}' has multiple constructors with tool arguments.");
            }

            return attributedConstructors.SingleOrDefault()
                ?? constructors.SingleOrDefault()
                ?? throw new InvalidOperationException($"Tool type '{toolType.FullName}' must have exactly one public constructor.");
        }

        private static ToolParameterDescriptor CreateParameterDescriptor(ParameterInfo parameter)
        {
            var argument = parameter.GetCustomAttribute<ToolArgumentAttribute>()
                ?? throw new InvalidOperationException($"Constructor parameter '{parameter.Name}' must have {nameof(ToolArgumentAttribute)}.");

            var argumentName = string.IsNullOrWhiteSpace(argument.Name)
                ? parameter.Name ?? throw new InvalidOperationException("Tool argument parameter name could not be resolved.")
                : argument.Name;

            var definition = new ToolArgumentDefinition
            {
                Name = argumentName,
                Description = argument.Description,
                ArgumentType = parameter.ParameterType,
                IsRequired = !parameter.HasDefaultValue
            };

            return new ToolParameterDescriptor(parameter, argumentName, definition);
        }

        private static object?[] BindArguments(ToolCall toolCall, ToolDescriptor descriptor)
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(toolCall.ArgumentsJson)
                ? "{}"
                : toolCall.ArgumentsJson);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException($"Tool '{toolCall.ToolName}' arguments must be a JSON object.");
            }

            return descriptor.Parameters
                .Select(parameter => BindArgument(document.RootElement, parameter))
                .ToArray();
        }

        private static object? BindArgument(JsonElement root, ToolParameterDescriptor parameter)
        {
            if (TryGetProperty(root, parameter.ArgumentName, out var value))
            {
                return value.Deserialize(parameter.Parameter.ParameterType, JsonOptions);
            }

            if (parameter.Parameter.HasDefaultValue)
            {
                return parameter.Parameter.DefaultValue;
            }

            throw new JsonException($"Missing required tool argument '{parameter.ArgumentName}'.");
        }

        private static bool TryGetProperty(JsonElement root, string propertyName, out JsonElement value)
        {
            foreach (var property in root.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private sealed record ToolDescriptor(
            Type ToolType,
            IReadOnlyList<ToolParameterDescriptor> Parameters,
            ToolDefinition Definition);

        private sealed record ToolParameterDescriptor(
            ParameterInfo Parameter,
            string ArgumentName,
            ToolArgumentDefinition Definition);
    }
}
