namespace Ferretto.VW.RemoteIODriver
{
    public interface IRemoteIODriver
    {
        #region Properties

        byte[] InputStateLines { get; }

        string IPAddressToConnect { get; set; }

        int PortAddressToConnect { get; set; }

        #endregion Properties

        #region Methods

        bool Initialize();

        bool ReadAllInputLines();

        void Terminate();

        bool WriteAllOutputLines(byte[] value);

        #endregion Methods
    }
}
