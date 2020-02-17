using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public sealed class LaserFieldMessageData : FieldMessageData, IFieldMessageData
    {
        #region Constructors

        public LaserFieldMessageData(MessageVerbosity verbosity = MessageVerbosity.Debug) : base(verbosity)
        {
        }

        #endregion

        #region Properties

        public int Speed { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }

        #endregion
    }
}
