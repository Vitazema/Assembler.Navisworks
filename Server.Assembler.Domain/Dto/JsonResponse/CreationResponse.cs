using Server.Assembler.Domain.Dto.DataObjects;

namespace Server.Assembler.Domain.Dto.JsonResponse
{
  /// <summary>
  ///   Содержимое ответа BIM360 при создании объектов
  /// </summary>
  public class CreationResponse
  {
    /// <summary>
    ///   Данные ответа
    /// </summary>
    public IdentifiedData Data { get; set; } = new IdentifiedData();
  }
}