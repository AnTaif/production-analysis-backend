using System.Text.Json.Serialization;

namespace ProductionAnalysis.Data.Models.Forms;

/// <summary>
/// Базовый класс для DTO контекста формы при сериализации в БД
/// Используется только в Data слое
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ProductFormContextDbo), typeDiscriminator: "product")]
[JsonDerivedType(typeof(MultiProductFormContextDbo), typeDiscriminator: "multiProduct")]
public abstract class FormContextBaseDbo
{
}