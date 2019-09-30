namespace Ferretto.VW.MAS.InverterDriver.Contracts
{
    public enum InverterIndex : byte
    {
        MainInverter = 0x00,

        Slave1 = 0x01,

        Slave2 = 0x02,

        Slave3 = 0x03,

        Slave4 = 0x04,

        Slave5 = 0x05,

        Slave6 = 0x06,

        Slave7 = 0x07,

        All = 0x10,

        None = 0xFF,
    }
}
