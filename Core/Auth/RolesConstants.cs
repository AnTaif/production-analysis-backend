using System.Reflection;

namespace Core.Auth;

public static class RolesConstants
{
    public const string Admin = "Operator";
    public const string Tutor = "Analyst";
    public const string Student = "Admin";

    public static IReadOnlyCollection<string> GetRoles()
    {
        return typeof(RolesConstants)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f is { IsLiteral: true, IsInitOnly: false } && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToArray();
    }
}