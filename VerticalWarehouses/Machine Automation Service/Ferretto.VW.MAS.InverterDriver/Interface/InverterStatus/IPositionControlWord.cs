using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus
{
    public interface IPositionControlWord : IControlWord
    {
        #region Properties

        bool ChangeSetPoint { set; }

        bool ImmediateChangeSet { set; }

        bool NewSetPoint { set; }

        bool RelativeMovement { set; }

        #endregion
    }
}
