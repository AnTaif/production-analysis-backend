using System.Text.Json;

namespace ProductionAnalysis.Application.Domain.Forms.Context;

public static class FormContextExtensions
{
    /// <summary>
    /// Получает контекст определенного типа по ключу из словаря контекста формы
    /// </summary>
    /// <typeparam name="T">Тип контекста для десериализации</typeparam>
    /// <param name="context">Словарь контекста формы</param>
    /// <param name="key">Ключ контекста</param>
    /// <returns>Десериализованный контекст указанного типа или null, если контекст не найден</returns>
    public static T? GetContext<T>(this Dictionary<string, object> context, string key)
        where T : class
    {
        if (!context.TryGetValue(key, out var contextValue))
        {
            return null;
        }

        try
        {
            // Если значение уже является JsonElement, сериализуем его обратно в строку для десериализации
            var jsonString = contextValue is JsonElement jsonElement
                ? jsonElement.GetRawText()
                : JsonSerializer.Serialize(contextValue);

            return JsonSerializer.Deserialize<T>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Проверяет наличие контекста с указанным ключом
    /// </summary>
    public static bool HasContext(this Dictionary<string, object> context, string key)
    {
        return context.ContainsKey(key);
    }

    /// <summary>
    /// Получает контекст определенного типа по ключу или возвращает значение по умолчанию
    /// </summary>
    public static T GetContextOrDefault<T>(this Dictionary<string, object> context, string key,
        T defaultValue = default!)
        where T : class
    {
        return GetContext<T>(context, key) ?? defaultValue;
    }
}