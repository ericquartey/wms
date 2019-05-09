namespace Ferretto.VW.MAS_InverterDriver.Interface.InverterStatus
{
    public interface IPositionControlWord : IControlWord
    {
        #region Properties

        bool AbsoluteMovement { set; }

        bool ChangeSetPoint { set; }

        bool ImmediateChangeSet { set; }

        bool NewSetPoint { set; }

        #endregion
    }
}
