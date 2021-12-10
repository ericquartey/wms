using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    public class CellPlus : Cell
    {
        #region Constructors

        public CellPlus()
        {
        }

        public CellPlus(Cell from, LoadingUnit loadUnit)
        {
            if (from != null)
            {
                this.BlockLevel = from.BlockLevel;
                this.Id = from.Id;
                this.IsFree = from.IsFree;
                this.PanelId = from.PanelId;
                this.Position = from.Position;
                this.Priority = from.Priority;
                this.Side = from.Side;
                this.SupportType = from.SupportType;
                this.LoadingUnit = loadUnit;
                this.LoadUnitId = null;
                this.Description = from.Description;
            }
        }

        #endregion

        #region Properties

        public LoadingUnit LoadingUnit { get; set; }

        public int? LoadUnitId { get; set; }

        #endregion
    }
}
