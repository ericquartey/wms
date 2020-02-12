using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public sealed class CurrentPositionFieldMessageData : FieldMessageData, IFieldMessageData
    {
        #region Constructors

        public CurrentPositionFieldMessageData(double currentPosition, MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.CurrentPosition = currentPosition;
        }

        #endregion

        #region Properties

        public double CurrentPosition { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"CurrentPosition:{this.CurrentPosition}";
        }

        #endregion
    }
}
