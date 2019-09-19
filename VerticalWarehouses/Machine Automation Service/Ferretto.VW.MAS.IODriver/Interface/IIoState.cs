namespace Ferretto.VW.MAS.IODriver.Interface
{
    public interface IIoState
    {
        #region Properties

        /// <summary>
        /// Type of IO State
        /// </summary>
        string Type { get; }

        #endregion

        #region Methods

        void ProcessMessage(IoMessage message);

        void ProcessResponseMessage(IoReadMessage message);

        void Start();

        #endregion
    }
}
