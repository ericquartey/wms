using System;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Modules.BLL;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.DAL.Models;

namespace Ferretto.Common.Modules.BLL.Models
{
  public sealed class Item : IItem
  {
    #region Fields

    private int? width;
    private int? length;
    private int? height;
    private int? reorderQuantity;
    private int? averageWeight;
    private int? pickTolerance;
    private int? storeTolerance;
    private int? inventoryTolerance;

    #endregion

    #region Properties

    public int Id { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }

    public int? Width
    {
      get => this.width;
      set => this.SetIfStrictlyPositive(ref this.width, value);
    }

    public int? Length
    {
      get => this.length;
      set => this.SetIfStrictlyPositive(ref this.length, value);
    }

    public int? Height
    {
      get => this.height;
      set => this.SetIfStrictlyPositive(ref this.height, value);
    }

    public int? FifoTimeStore { get; set; }
    public int? FifoTimePick { get; set; }
    public int? ReorderPoint { get; set; }

    public int? ReorderQuantity
    {
      get => this.reorderQuantity;
      set => this.SetIfStrictlyPositive(ref this.reorderQuantity, value);
    }

    public int? AverageWeight
    {
      get => this.averageWeight;
      set => this.SetIfStrictlyPositive(ref this.averageWeight, value);
    }

    public int? PickTolerance
    {
      get => this.pickTolerance;
      set => this.SetIfStrictlyPositive(ref this.pickTolerance, value);
    }

    public int? StoreTolerance
    {
      get => this.storeTolerance;
      set => this.SetIfStrictlyPositive(ref this.storeTolerance, value);
    }

    public int? InventoryTolerance
    {
      get => this.inventoryTolerance;
      set => this.SetIfStrictlyPositive(ref this.inventoryTolerance, value);
    }

    public DateTime CreationDate { get; set; }
    public DateTime? LastModificationDate { get; set; }
    public DateTime? InventoryDate { get; set; }
    public DateTime? LastPickDate { get; set; }
    public DateTime? LastStoreDate { get; set; }
    public string Image { get; set; }
    public string Note { get; set; }
    public AbcClass AbcClass { get; set; }
    public MeasureUnit MeasureUnit { get; set; }
    public ItemManagementType ItemManagementType { get; set; }

    #endregion

  }
}
