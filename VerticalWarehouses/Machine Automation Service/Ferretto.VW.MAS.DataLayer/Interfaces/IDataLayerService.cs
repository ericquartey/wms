namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IDataLayerService
    {
        #region Properties

        bool IsReady { get; }

        #endregion

        #region Methods

        void CopyMachineDatabaseToServer(string server, string username, string password, string database, string serialNumber);

        #endregion

        //byte[] GetRawDatabaseContent();
    }
}
