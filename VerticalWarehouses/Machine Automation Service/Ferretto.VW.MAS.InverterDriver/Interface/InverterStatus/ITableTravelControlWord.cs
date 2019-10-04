using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus
{
    public interface ITableTravelControlWord : IControlWord
    {
        #region Properties

        bool MotionBlockSelect0 { set; }

        bool MotionBlockSelect1 { set; }

        bool MotionBlockSelect2 { set; }

        bool MotionBlockSelect3 { set; }

        bool MotionBlockSelect4 { set; }

        bool Resume { set; }

        bool SequenceMode { set; }

        bool StartMotionBlock { get; set; }

        #endregion
    }
}
