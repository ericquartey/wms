using System.Linq;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Modules.Operator.Models
{
    public class ItemInfo : Item
    {
        #region Constructors

        public ItemInfo(Item item, int machineId)
        {
            this.Id = item.Id;
            this.Code = item.Code;
            this.Description = item.Description;
            this.Machines = item.Machines;

            if (item.Machines.Any(m => m.Id == machineId))
            {
                this.IsQtyOnMachine = true;
            }

            if (item.Machines.Any())
            {
                this.MachinesInfo = string.Join(", ", item.Machines.Select(m => m.Nickname).ToArray());
            }
        }

        #endregion

        #region Properties

        public bool IsQtyOnMachine { get; }

        public string MachinesInfo { get; }

        #endregion
    }
}
