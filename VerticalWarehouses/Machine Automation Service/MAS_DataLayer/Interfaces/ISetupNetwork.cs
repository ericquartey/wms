namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface ISetupNetwork
    {
        #region Properties

        string AlfaNumBay1Net { get; }

        string AlfaNumBay2Net { get; }

        string AlfaNumBay3Net { get; }

        string Inverter1 { get; }

        string Inverter1Port { get; }

        string Inverter2 { get; }

        string Inverter2Port { get; }

        string IOExpansion1 { get; }

        string IOExpansion1Port { get; }

        string IOExpansion2 { get; }

        string IOExpansion2Port { get; }

        string IOExpansion3 { get; }

        string IOExpansion3Port { get; }

        string LaserBay1Net { get; }

        string LaserBay2Net { get; }

        string LaserBay3Net { get; }

        int MachineNumber { get; }

        string PPC1MasterIPAddress { get; }

        string PPC2SlaveIPAddress { get; }

        string PPC3SlaveIPAddress { get; }

        string SQLServerIPAddress { get; }

        bool WMS_ON { get; }

        #endregion
    }
}
