using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bridge.Api.Contracts;

public sealed class FlexibleSummaryJsonConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var parts = new List<string>();
        CollectText(document.RootElement, parts);
        return string.Join(" ", parts.Where(part => !string.IsNullOrWhiteSpace(part)).Select(part => part.Trim()));
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);

    private static void CollectText(JsonElement element, ICollection<string> parts)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                parts.Add(element.GetString() ?? string.Empty);
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    CollectText(item, parts);
                break;
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                    CollectText(property.Value, parts);
                break;
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                parts.Add(element.GetRawText());
                break;
        }
    }
}
