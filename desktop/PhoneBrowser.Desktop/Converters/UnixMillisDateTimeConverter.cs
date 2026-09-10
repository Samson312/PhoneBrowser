namespace PhoneBrowser.Desktop.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;

public class UnixMillisDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64()).UtcDateTime;

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteNumberValue(new DateTimeOffset(value, TimeSpan.Zero).ToUnixTimeMilliseconds());
}

