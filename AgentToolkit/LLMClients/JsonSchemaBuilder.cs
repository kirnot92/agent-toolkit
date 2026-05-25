using System.Text.Json.Nodes;

namespace AgentToolkit.LLMClients
{
    public enum JsonSchemaType
    {
        String,
        Number,
        Boolean
    }

    public sealed class JsonSchemaBuilder
    {
        private readonly JsonObject properties = new();
        private readonly List<string> required = new();

        private JsonSchemaBuilder()
        {
        }

        public static JsonSchemaBuilder Object()
        {
            return new JsonSchemaBuilder();
        }

        public JsonSchemaBuilder AddProperty(
            JsonSchemaType type,
            string name,
            string? description = null)
        {
            var property = CreateProperty(type, description);
            this.AddRequiredProperty(name, property);

            return this;
        }

        public JsonSchemaBuilder AddObjectProperty(
            string name,
            Action<JsonSchemaBuilder> configureObject,
            string? description = null)
        {
            ArgumentNullException.ThrowIfNull(configureObject);

            var property = CreateObjectProperty(configureObject);
            if (!string.IsNullOrWhiteSpace(description))
            {
                property["description"] = description;
            }

            this.AddRequiredProperty(name, property);

            return this;
        }

        public JsonSchemaBuilder AddArrayProperty(
            JsonSchemaType itemType,
            string name,
            string? description = null)
        {
            var property = CreateArrayProperty(new JsonObject
            {
                ["type"] = ToJsonSchemaType(itemType)
            });

            if (!string.IsNullOrWhiteSpace(description))
            {
                property["description"] = description;
            }

            this.AddRequiredProperty(name, property);

            return this;
        }

        public JsonSchemaBuilder AddObjectArrayProperty(
            string name,
            Action<JsonSchemaBuilder> configureItem,
            string? description = null)
        {
            ArgumentNullException.ThrowIfNull(configureItem);

            var builder = Object();
            configureItem(builder);

            var property = CreateArrayProperty(builder.Build());
            if (!string.IsNullOrWhiteSpace(description))
            {
                property["description"] = description;
            }

            this.AddRequiredProperty(name, property);

            return this;
        }

        public JsonSchemaBuilder AddEnumProperty(
            string name,
            IReadOnlyList<string> enumValues,
            string? description = null)
        {
            ArgumentNullException.ThrowIfNull(enumValues);

            if (enumValues.Count == 0)
            {
                throw new ArgumentException("Enum values cannot be empty.", nameof(enumValues));
            }

            var property = CreateProperty(JsonSchemaType.String, description);
            property["enum"] = new JsonArray(enumValues
                .Select(value => JsonValue.Create(value))
                .ToArray<JsonNode?>());

            this.AddRequiredProperty(name, property);

            return this;
        }

        public JsonObject Build()
        {
            return new JsonObject
            {
                ["type"] = "object",
                ["properties"] = this.properties.DeepClone(),
                ["required"] = new JsonArray(this.required
                    .Select(propertyName => JsonValue.Create(propertyName))
                    .ToArray<JsonNode?>()),
                ["additionalProperties"] = false
            };
        }

        private static JsonObject CreateProperty(
            JsonSchemaType type,
            string? description)
        {
            var property = type switch
            {
                JsonSchemaType.String or JsonSchemaType.Number or JsonSchemaType.Boolean => new JsonObject
                {
                    ["type"] = ToJsonSchemaType(type)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported JSON schema type.")
            };

            if (!string.IsNullOrWhiteSpace(description))
            {
                property["description"] = description;
            }

            return property;
        }

        private void AddRequiredProperty(string name, JsonObject property)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Property name cannot be empty.", nameof(name));
            }

            if (this.properties.ContainsKey(name))
            {
                throw new ArgumentException($"Property '{name}' is already defined.", nameof(name));
            }

            this.properties.Add(name, property);
            this.required.Add(name);
        }

        private static JsonObject CreateObjectProperty(Action<JsonSchemaBuilder>? configureObject)
        {
            if (configureObject is null)
            {
                throw new ArgumentException("Object properties require a nested schema configuration.", nameof(configureObject));
            }

            var builder = Object();
            configureObject(builder);
            return builder.Build();
        }

        private static JsonObject CreateArrayProperty(JsonObject items)
        {
            return new JsonObject
            {
                ["type"] = "array",
                ["items"] = items
            };
        }

        private static string ToJsonSchemaType(JsonSchemaType type)
        {
            return type switch
            {
                JsonSchemaType.String => "string",
                JsonSchemaType.Number => "number",
                JsonSchemaType.Boolean => "boolean",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported JSON schema type.")
            };
        }
    }
}
