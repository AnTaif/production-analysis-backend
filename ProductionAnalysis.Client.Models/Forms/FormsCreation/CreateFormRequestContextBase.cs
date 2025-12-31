namespace ProductionAnalysis.Client.Models.Forms.FormsCreation;

/// <summary>
/// Базовый класс для контекста создания формы.
/// Контекст содержит дополнительную информацию, необходимую для расчета формул в таблице формы.
/// </summary>
/// <remarks>
/// Доступные типы контекста:
/// <list type="bullet">
/// <item><see cref="CreateFormRequestProductContext"/> - контекст для одного продукта с параметрами производства</item>
/// <item><see cref="CreateFormRequestProductListContext"/> - контекст для списка продуктов</item>
/// <item><see cref="CreateFormRequestOperationContext"/> - контекст для операции</item>
/// </list>
/// </remarks>
public abstract class CreateFormRequestContextBase
{
}