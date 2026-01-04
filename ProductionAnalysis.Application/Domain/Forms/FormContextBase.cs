using System.Text.Json.Serialization;

namespace ProductionAnalysis.Application.Domain.Forms;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ProductFormContext), typeDiscriminator: "product")]
[JsonDerivedType(typeof(MultiProductFormContext), typeDiscriminator: "multiProduct")]
public abstract class FormContextBase
{
}