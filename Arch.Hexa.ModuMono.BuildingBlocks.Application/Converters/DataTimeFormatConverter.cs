using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arch.Hexa.ModuMono.BuildingBlocks.Application.Converters;

public class DateTimeFormatConverter(string dateFormat) : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var dateString = reader.GetString();
            if (DateTime.TryParseExact(dateString, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }
        }

        throw new JsonException($"Unable to convert \"{reader.GetString()}\" to a DateTime object.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(dateFormat, CultureInfo.InvariantCulture));
    }
}