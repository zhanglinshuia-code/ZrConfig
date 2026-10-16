using System.Text.Json;
using ZrConfig.Runtime;

namespace ZrConfig.App;

internal static class LanguageSettings
{
    public static string DefaultPath
    {
        get
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ZrConfig", "settings.json");
        }
    }

    public static string Load(string path)
    {
        try
        {
            using var json = JsonDocument.Parse(File.ReadAllText(path));
            if (json.RootElement.ValueKind == JsonValueKind.Object && json.RootElement.TryGetProperty("language",
                out JsonElement language) && language.ValueKind == JsonValueKind.String && language.GetString() == ConfigText.English)
            {
                return ConfigText.English;
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            // 首次启动或偏好文件不可读时，使用中文；不影响导表。
        }

        return ConfigText.Chinese;
    }

    public static void Save(string path, string language)
    {
        using IDisposable scope = ConfigText.UseLanguage(language);
        _ = Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(new
            {
                language
            }));
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }
}
