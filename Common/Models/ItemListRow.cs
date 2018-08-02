using System;
using System.Collections.Generic;

namespace Ferretto.Common.Models
{
  // Riga di Lista Articoli
  public sealed class ItemListRow
  {
    public int Id { get; set; }
    public int ItemListId { get; set; }
    public string Code { get; set; }
    public int RowPriority { get; set; }
    public int ItemId { get; set; }
    public string Sub1 { get; set; }
    public string Sub2 { get; set; }
    public int MaterialStatusId { get; set; }
    public int PackageTypeId { get; set; }
    public string Lot { get; set; }
    public string RegistrationNumber { get; set; }
    public int RequiredQuantity { get; set; }
    public int EvadedQuantity { get; set; }
    public int ItemListRowStatusId { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime? LastModificationDate { get; set; }
    public DateTime? LastExecutionDate { get; set; }
    public DateTime? CompletionDate { get; set; }

    public ItemList ItemList { get; set; }
    public Item Item { get; set; }
    public MaterialStatus MaterialStatus { get; set; }
    public PackageType PackageType { get; set; }
    public ItemListRowStatus ItemListRowStatus { get; set; }

    public List<Mission> Missions { get; set; }
  }
}
