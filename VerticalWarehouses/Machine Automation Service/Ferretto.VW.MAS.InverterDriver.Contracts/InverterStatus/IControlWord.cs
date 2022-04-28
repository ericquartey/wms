using System.ComponentModel.DataAnnotations.Schema;

namespace Ferretto.VW.MAS.InverterDriver.Contracts
{
    public interface IControlWord
    {
        #region Properties

        [Column(Order = 3)]
        bool EnableOperation { get; set; }

        [Column(Order = 1)]
        bool EnableVoltage { get; set; }

        [Column(Order = 7)]
        bool FaultReset { get; set; }

        [Column(Order = 8)]
        bool Halt { get; set; }

        [Column(Order = 10)]
        bool HeartBeat { get; set; }

        [Column(Order = 15)]
        bool HorizontalAxis { get; set; }

        [Column(Order = 2)]
        bool QuickStop { get; set; }

        [Column(Order = 0)]
        bool SwitchOn { get; set; }

        ushort Value { get; set; }

        #endregion
    }
}
