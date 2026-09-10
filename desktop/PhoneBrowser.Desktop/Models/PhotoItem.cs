namespace PhoneBrowser.Desktop.Models;

using System.Text.Json.Serialization;
using PhoneBrowser.Desktop.Converters;

public record PhotoItem
(
    string PhotoId,
    string FileName,
    [property: JsonConverter(typeof(UnixMillisDateTimeConverter))]
    DateTime DateTakenUtc,
    long SizeBytes
);

