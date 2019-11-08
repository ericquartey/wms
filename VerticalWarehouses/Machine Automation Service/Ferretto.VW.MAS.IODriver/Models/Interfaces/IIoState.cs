namespace Ferretto.VW.MAS.IODriver
{
    internal interface IIoState
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
