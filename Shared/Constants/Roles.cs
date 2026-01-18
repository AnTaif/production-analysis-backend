using System.Reflection;

namespace Shared.Constants;

public static class Roles
{
    public const string Operator = "Operator";
    public const string DepartmentHead = "DepartmentHead";
    public const string Analyst = "Analyst";
    public const string Admin = "Admin";
    public const string JustEmployee = "JustEmployee";

    public static IReadOnlyCollection<string> GetRoles()
    {
        return typeof(Roles)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f is { IsLiteral: true, IsInitOnly: false } && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToArray();
    }
}