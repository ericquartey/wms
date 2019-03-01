namespace Ferretto.VW.MAS_IODriver.StateMachines
{
    public interface IIoState
    {
        #region Properties

        string Type { get; }

        #endregion

        #region Methods

        void ProcessMessage(IoMessage message);

        #endregion
    }
}
