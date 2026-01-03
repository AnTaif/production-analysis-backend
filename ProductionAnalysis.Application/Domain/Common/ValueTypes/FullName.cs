namespace ProductionAnalysis.Application.Domain.Common.ValueTypes;

public readonly record struct FullName
{
    public FullName(string lastName, string firstName, string? middleName)
    {
        LastName = Normalize(lastName);
        FirstName = Normalize(firstName);
        MiddleName = NormalizeOrNull(middleName);
    }

    public string LastName { get; }
    public string FirstName { get; }
    public string? MiddleName { get; }

    public override string ToString() =>
        MiddleName is null
            ? $"{LastName} {FirstName}"
            : $"{LastName} {FirstName} {MiddleName}";

    private static string Normalize(string value) =>
        value.Trim().Replace("  ", " ");

    private static string? NormalizeOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : Normalize(value);

    public static FullName Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Строка ФИО не может быть пустой", nameof(input));

        var parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return parts.Length switch
        {
            2 => new FullName(parts[0], parts[1], null),
            3 => new FullName(parts[0], parts[1], parts[2]),
            _ => throw new FormatException("Некорректный формат ФИО. Ожидается 'Фамилия Имя [Отчество]'"),
        };
    }

    public void Deconstruct(out string lastName, out string firstName, out string? middleName)
    {
        lastName = LastName;
        firstName = FirstName;
        middleName = MiddleName;
    }
}