namespace Ferretto.VW.MAS.TimeManagement
{
    public interface ISystemTimeProvider
    {
        #region Properties

        /// <summary>
        /// Returns True if it is possible to enable the time synchronisation with WMS, False otherwise.
        /// </summary>
        bool CanEnableWmsAutoSyncMode { get; }

        /// <summary>
        /// Gets or sets the indication of whether the time synchronisation with WMS is enabled.
        /// </summary>
        bool IsWmsAutoSyncEnabled { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the local system time to the specified time.
        /// </summary>
        /// <param name="dateTime">The new system time.</param>
        /// <exception cref="System.InvalidOperationException">Exception is thrown if it is not possible to set the system time.</exception>
        void SetUtcSystemTime(System.DateTimeOffset dateTime);

        #endregion
    }
}
