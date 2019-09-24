using System.ComponentModel.DataAnnotations.Schema;

namespace Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus
{
    public interface IControlWord
    {
        #region Properties

        [Column(Order = 4)]
        bool EnableOperation { set; }

        [Column(Order = 2)]
        bool EnableVoltage { set; }

        [Column(Order = 8)]
        bool FaultReset { get; set; }

        [Column(Order = 9)]
        bool Halt { set; }

        [Column(Order = 11)]
        bool HeartBeat { get; set; }

        [Column(Order = 16)]
        bool HorizontalAxis { get; set; }

        [Column(Order = 3)]
        bool QuickStop { set; }

        [Column(Order = 1)]
        bool SwitchOn { set; }

        ushort Value { get; set; }

        #endregion
    }
}
