using System.Linq;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator.Models
{
    public enum ListExecutionMode
    {
        None,

        AllRows,

        SomeRows,
    }

    public class ItemListExecution : ItemList
    {
        #region Constructors

        public ItemListExecution(ItemList itemList, int machineId)
        {
            if (itemList is null)
            {
                throw new System.ArgumentNullException(nameof(itemList));
            }

            this.Id = itemList.Id;
            this.ItemListType = itemList.ItemListType;
            this.Code = itemList.Code;
            this.Description = itemList.Description;
            this.ShipmentUnitCode = itemList.ShipmentUnitCode;
            this.ShipmentUnitDescription = itemList.ShipmentUnitDescription;

            if (itemList.Machines?.Any(m => m.Id == machineId) == true)
            {
                this.ExecutionMode = (itemList.Machines.Count() == 1) ? ListExecutionMode.AllRows : ListExecutionMode.SomeRows;
            }

            if (itemList.Machines?.Any() == true)
            {
                this.MachinesInfo = string.Join(", ", itemList.Machines.Select(m => m.Nickname).ToArray());
            }
        }

        public ItemListExecution(ItemListDetails itemList, int machineId)
            : this(
                new ItemList
                {
                    Code = itemList.Code,
                    CreationDate = itemList.CreationDate,
                    Description = itemList.Description,
                    Id = itemList.Id,
                    ItemListRowsCount = itemList.ItemListRowsCount,
                    ItemListType = itemList.ItemListType,
                    Policies = itemList.Policies,
                    Priority = itemList.Priority,
                    ShipmentUnitCode = itemList.ShipmentUnitCode,
                    ShipmentUnitDescription = itemList.ShipmentUnitDescription,
                    Status = itemList.Status,
                }, machineId)
        {
        }

        #endregion

        #region Properties

        public ListExecutionMode ExecutionMode { get; }

        public string MachinesInfo { get; }

        #endregion
    }
}
