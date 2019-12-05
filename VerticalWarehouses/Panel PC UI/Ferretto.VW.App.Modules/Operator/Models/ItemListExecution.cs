using System.Linq;
using Ferretto.WMS.Data.WebAPI.Contracts;

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
            this.Id = itemList.Id;
            this.ItemListType = itemList.ItemListType;
            this.Code = itemList.Code;
            this.Description = itemList.Description;

            if (itemList.Machines.Any(m => m.Id == machineId))
            {
                this.ExecutionMode = (itemList.Machines.Count == 1) ? ListExecutionMode.AllRows : ListExecutionMode.SomeRows;
            }

            if (itemList.Machines.Any())
            {
                this.MachinesInfo = string.Join(", ", itemList.Machines.Select(m => m.Nickname).ToArray());
            }
        }

        #endregion

        #region Properties

        public ListExecutionMode ExecutionMode { get; }

        public string MachinesInfo { get; }

        #endregion
    }
}
