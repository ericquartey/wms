using System.ComponentModel.DataAnnotations.Schema;

namespace Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus
{
    public interface IControlWord
    {
        #region Properties

        [Column(Order = 3)]
        bool EnableOperation { set; }

        [Column(Order = 1)]
        bool EnableVoltage { set; }

        [Column(Order = 7)]
        bool FaultReset { get; set; }

        [Column(Order = 8)]
        bool Halt { set; }

        [Column(Order = 10)]
        bool HeartBeat { get; set; }

        [Column(Order = 15)]
        bool HorizontalAxis { get; set; }

        [Column(Order = 2)]
        bool QuickStop { set; }

        [Column(Order = 0)]
        bool SwitchOn { set; }

        ushort Value { get; set; }

        #endregion
    }
}
