namespace ProductionAnalysis.Client.Models.Forms;

public record FormFieldDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string InputType { get; init; } = string.Empty;
    public string? InputSelector { get; init; }
    public string? ValueType { get; init; }
    public bool IsCumulative { get; init; }

    /// <summary>
    /// Указывает, должна ли колонка объединяться для строк с одинаковым GroupKey.
    /// Используется для форм, где несколько строк относятся к одной группе (например, цикл операций).
    /// </summary>
    public bool ShouldMergeInGroup { get; init; }
}