using System.Collections.Generic;
using Server.Assembler.Domain.Dto.DataObjects;

namespace Server.Assembler.Domain.Dto.JsonResponse
{
  /// <summary>
  ///   Содержимое ответа BIM360 при получении папок
  /// </summary>
  public class ItemsResponse
  {
    /// <summary>
    ///   Данные ответа
    /// </summary>
    public List<IdentifiedDataExtended> Data { get; set; } = new List<IdentifiedDataExtended>();
  }
}