using System.Text.Json;
using System.Text.Json.Serialization;
using ZrConfig.Runtime;

namespace ZrConfig.Core;

[Flags]
public enum ConfigTarget
{
    Client = 1,
    Server = 2,
    Both = Client | Server
}

internal static class ConfigTargets
{
    public static ConfigTarget Parse(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "C" => ConfigTarget.Client,
        "S" => ConfigTarget.Server,
        _ => throw new ArgumentException(ConfigText.Get("目标端只允许 C（客户端）或 S（服务端）。", "The target must be C (client) or S (server)."))
    };
    public static string Marker(ConfigTarget target) => target switch
    {
        ConfigTarget.Client => "C",
        ConfigTarget.Server => "S",
        _ => throw new ArgumentOutOfRangeException(nameof(target), ConfigText.Get("请选择客户端 C 或服务端 S。", "Select client C or server S."))
    };
}

internal sealed class ConfigTargetJsonConverter : JsonConverter<ConfigTarget>
{
    public override ConfigTarget Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Target must be C or S.");
        }

        try
        {
            return ConfigTargets.Parse(reader.GetString());
        }
        catch (ArgumentException ex)
        {
            throw new JsonException(ex.Message, ex);
        }
    }

    public override void Write(Utf8JsonWriter writer,
        ConfigTarget value,
        JsonSerializerOptions options) => writer.WriteStringValue(ConfigTargets.Marker(value));
}
