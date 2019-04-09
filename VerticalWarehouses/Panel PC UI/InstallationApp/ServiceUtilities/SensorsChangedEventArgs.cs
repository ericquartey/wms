namespace Ferretto.VW.InstallationApp.ServiceUtilities
{
    public class SensorsChangedEventArgs
    {
        #region Constructors

        public SensorsChangedEventArgs(bool[] states)
        {
            this.SensorsStates = states;
        }

        #endregion

        #region Properties

        public bool[] SensorsStates { get; set; }

        #endregion
    }
}
