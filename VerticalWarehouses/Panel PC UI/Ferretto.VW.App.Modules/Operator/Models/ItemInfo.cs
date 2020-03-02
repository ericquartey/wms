using System.Linq;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator.Models
{
    public class ItemInfo : Item
    {
        #region Constructors

        public ItemInfo(Item item, int machineId)
        {
            if (item is null)
            {
                throw new System.ArgumentNullException(nameof(item));
            }

            this.Id = item.Id;
            this.Code = item.Code;
            this.Description = item.Description;
            this.MeasureUnitDescription = item.MeasureUnitDescription;
            this.Machines = item.Machines;
            this.PutTolerance = item.PutTolerance;
            this.PickTolerance = item.PickTolerance;
            this.AverageWeight = item.AverageWeight;
            this.IsQtyOnMachine = item.Machines.Any(m => m.Id == machineId);

            if (item.Machines.Any())
            {
                this.MachinesInfo = string.Join(", ", item.Machines.Select(m => m.Nickname).ToArray());
            }
        }

        #endregion

        #region Properties

        public bool IsQtyOnMachine { get; }

        public string MachinesInfo { get; }

        public string MeasureUnit => this.MeasureUnitDescription.ToLowerInvariant() ?? Resources.OperatorApp.Pieces;

        public double PickIncrement => this.PickTolerance.HasValue ? System.Math.Pow(10, -this.PickTolerance.Value) : 1;

        #endregion
    }
}
