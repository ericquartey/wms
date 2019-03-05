using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Common_Utils.DTOs
{
    public class MovementMessageDataDTO
    {
        #region Constructors

        public MovementMessageDataDTO(decimal mm, int axis, int movementType, uint speedPercentage)
        {
            this.Mm = mm;
            this.Axis = axis;
            this.MovementType = movementType;
            this.SpeedPercentage = speedPercentage;
        }

        #endregion

        #region Properties

        public int Axis { get; set; }

        public decimal Mm { get; set; }

        public int MovementType { get; set; }

        public uint SpeedPercentage { get; set; }

        #endregion
    }
}
