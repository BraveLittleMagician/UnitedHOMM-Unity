#nullable enable

using Newtonsoft.Json.Linq;

public sealed record ModifierDto(string Name, int? Duration = null, int? Charges = null)
{
    public JObject? Args { get; init; }
}