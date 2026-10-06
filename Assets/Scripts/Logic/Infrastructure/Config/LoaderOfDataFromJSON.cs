#nullable enable

using Newtonsoft.Json;
using System;

public static class LoaderOfDataFromJSON
{
    public static T LoadFromJson<T>(string json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON пуст или null", nameof(json));

        T? result;
        try
        {
            result = JsonConvert.DeserializeObject<T>(json);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Ошибка парсинга JSON в {typeof(T).Name}: {ex.Message}", ex);
        }

        if (result == null)
            throw new InvalidOperationException(
                $"JSON не содержит данных для {typeof(T).Name}");

        return result;
    }
}