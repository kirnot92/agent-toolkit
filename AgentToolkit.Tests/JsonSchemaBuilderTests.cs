using System.Text.Json.Nodes;
using AgentToolkit.LLMClients;

namespace AgentToolkit.Tests;

public sealed class JsonSchemaBuilderTests
{
    [Fact]
    public void Build_creates_strict_object_schema_with_required_properties()
    {
        var schema = JsonSchemaBuilder.Object()
            .AddProperty(JsonSchemaType.String, "answer", "The answer.")
            .AddProperty(JsonSchemaType.Number, "score")
            .Build();

        Assert.Equal("object", schema["type"]!.GetValue<string>());
        Assert.False(schema["additionalProperties"]!.GetValue<bool>());

        var properties = Assert.IsType<JsonObject>(schema["properties"]);
        Assert.Equal("string", properties["answer"]!["type"]!.GetValue<string>());
        Assert.Equal("The answer.", properties["answer"]!["description"]!.GetValue<string>());
        Assert.Equal("number", properties["score"]!["type"]!.GetValue<string>());

        Assert.Equal(
            ["answer", "score"],
            Assert.IsType<JsonArray>(schema["required"])
                .Select(property => property!.GetValue<string>())
                .ToArray());
    }

    [Fact]
    public void Build_creates_nested_object_and_string_array_properties()
    {
        var schema = JsonSchemaBuilder.Object()
            .AddObjectProperty(
                "schedule",
                schedule => schedule
                    .AddProperty(JsonSchemaType.String, "day"))
            .AddArrayProperty(JsonSchemaType.String, "tags")
            .Build();

        var properties = Assert.IsType<JsonObject>(schema["properties"]);
        var schedule = Assert.IsType<JsonObject>(properties["schedule"]);
        var scheduleProperties = Assert.IsType<JsonObject>(schedule["properties"]);
        Assert.Equal("string", scheduleProperties["day"]!["type"]!.GetValue<string>());
        Assert.False(schedule["additionalProperties"]!.GetValue<bool>());

        var tags = Assert.IsType<JsonObject>(properties["tags"]);
        Assert.Equal("array", tags["type"]!.GetValue<string>());
        Assert.Equal("string", tags["items"]!["type"]!.GetValue<string>());
    }

    [Fact]
    public void Build_creates_number_array_and_object_array_properties()
    {
        var schema = JsonSchemaBuilder.Object()
            .AddArrayProperty(JsonSchemaType.Number, "scores")
            .AddObjectArrayProperty(
                "attendees",
                attendee => attendee
                    .AddProperty(JsonSchemaType.String, "name")
                    .AddProperty(JsonSchemaType.Boolean, "required"))
            .Build();

        var properties = Assert.IsType<JsonObject>(schema["properties"]);

        var scores = Assert.IsType<JsonObject>(properties["scores"]);
        Assert.Equal("array", scores["type"]!.GetValue<string>());
        Assert.Equal("number", scores["items"]!["type"]!.GetValue<string>());

        var attendees = Assert.IsType<JsonObject>(properties["attendees"]);
        Assert.Equal("array", attendees["type"]!.GetValue<string>());

        var attendeeItems = Assert.IsType<JsonObject>(attendees["items"]);
        var attendeeProperties = Assert.IsType<JsonObject>(attendeeItems["properties"]);
        Assert.Equal("string", attendeeProperties["name"]!["type"]!.GetValue<string>());
        Assert.Equal("boolean", attendeeProperties["required"]!["type"]!.GetValue<string>());
        Assert.False(attendeeItems["additionalProperties"]!.GetValue<bool>());
    }

    [Fact]
    public void Build_creates_enum_property()
    {
        var schema = JsonSchemaBuilder.Object()
            .AddEnumProperty("priority", ["low", "medium", "high"], "Priority level.")
            .Build();

        var properties = Assert.IsType<JsonObject>(schema["properties"]);
        var priority = Assert.IsType<JsonObject>(properties["priority"]);

        Assert.Equal("string", priority["type"]!.GetValue<string>());
        Assert.Equal("Priority level.", priority["description"]!.GetValue<string>());
        Assert.Equal(
            ["low", "medium", "high"],
            Assert.IsType<JsonArray>(priority["enum"])
                .Select(value => value!.GetValue<string>())
                .ToArray());
    }

    [Fact]
    public void AddProperty_rejects_invalid_schema_configuration()
    {
        var builder = JsonSchemaBuilder.Object();

        Assert.Throws<ArgumentException>(() => builder.AddProperty(
            JsonSchemaType.String,
            ""));
        Assert.Throws<ArgumentException>(() => builder.AddEnumProperty("status", []));
    }
}
