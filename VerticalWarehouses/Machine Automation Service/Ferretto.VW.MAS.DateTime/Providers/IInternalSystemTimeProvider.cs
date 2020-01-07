namespace Ferretto.VW.MAS.TimeManagement
{
    internal interface IInternalSystemTimeProvider : ISystemTimeProvider
    {
        #region Methods

        void SetUtcTime(System.DateTime dateTime);

        #endregion
    }
}
