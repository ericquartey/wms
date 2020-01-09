namespace Ferretto.VW.MAS.TimeManagement
{
    internal interface IInternalSystemTimeProvider : ISystemTimeProvider
    {
        #region Methods

        void SetUtcTime(System.DateTimeOffset dateTime);

        #endregion
    }
}
