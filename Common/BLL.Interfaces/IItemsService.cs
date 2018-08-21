using System.Collections.Generic;

namespace Ferretto.Common.BLL.Interfaces
{
  public interface IItemsService
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Minor Code Smell",
      "S4049:Properties should be preferred",
      Justification = "The method name implements the repository pattern naming convention.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Minor Code Smell",
      "CA0124:Properties should be preferred",
      Justification = "The method name implements the repository pattern naming convention.")]
    IEnumerable<Models.IItem> GetAll();

    Models.IItem GetById(int id);

    Models.IItem Create(Models.IItem item);
  }
}
