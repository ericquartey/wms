using System;
using Ferretto.Common.DAL.Models;

namespace Ferretto.Common.BLL.Interfaces.Models
{
  public interface IItem : IModel<int>
  {
    string Code { get; set; }
    string Description { get; set; }
    int? Width { get; set; }
    int? Length { get; set; }
    int? Height { get; set; }
    int? FifoTimeStore { get; set; }
    int? FifoTimePick { get; set; }
    int? ReorderPoint { get; set; }
    int? ReorderQuantity { get; set; }
    int? AverageWeight { get; set; }
    int? PickTolerance { get; set; }
    int? StoreTolerance { get; set; }
    int? InventoryTolerance { get; set; }
    DateTime CreationDate { get; set; }
    DateTime? LastModificationDate { get; set; }
    DateTime? InventoryDate { get; set; }
    DateTime? LastPickDate { get; set; }
    DateTime? LastStoreDate { get; set; }
    string Image { get; set; }
    string Note { get; set; }
    AbcClass AbcClass { get; set; }
    MeasureUnit MeasureUnit { get; set; }
    ItemManagementType ItemManagementType { get; set; }
  }
}
