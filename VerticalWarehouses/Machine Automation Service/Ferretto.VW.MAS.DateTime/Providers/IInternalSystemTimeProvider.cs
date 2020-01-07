namespace Ferretto.VW.MAS.TimeManagement
{
    internal interface IInternalSystemTimeProvider : ISystemTimeProvider
    {
        #region Methods

        void SetTime(System.DateTime dateTime);

        #endregion
    }
}
