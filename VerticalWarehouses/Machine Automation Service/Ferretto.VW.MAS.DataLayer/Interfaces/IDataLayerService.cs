namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IDataLayerService
    {
        #region Properties

        bool IsReady { get; }

        #endregion

        #region Methods

        void CopyMachineDatabaseToServer(string host);

        byte[] GetRawDatabaseContent();

        #endregion
    }
}
