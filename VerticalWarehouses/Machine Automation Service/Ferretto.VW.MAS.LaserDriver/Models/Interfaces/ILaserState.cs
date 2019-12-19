namespace Ferretto.VW.MAS.LaserDriver
{
    internal interface ILaserState
    {
        #region Properties

        /// <summary>
        /// Type of IO State
        /// </summary>
        string Type { get; }

        #endregion

        #region Methods

        void ProcessResponseMessage(string message);

        void Start();

        #endregion
    }
}
