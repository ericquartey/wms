using Ferretto.VW.RemoteIODriver;

namespace Ferretto.VW.ActionBlocks
{
    public interface ICalibrateAxes
    {
        #region Properties
        InverterDriver.InverterDriver SetInverterDriverInterface { set; }

        IRemoteIO SetRemoteIOInterface { set; }

        #endregion Properties

        #region Methods
        void Initialize();

        void SetAxesOrigin(int m, short ofs, short vFast, short vCreep);

        void StopInverter();

        void Terminate();

        #endregion Methods
    }
}
