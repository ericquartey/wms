namespace Ferretto.VW.MAS_MachineManager
{
    public interface IMachineManager
    {
        #region Methods

        void DoHoming();

        void GetParam();

        void SetParam(int value);

        #endregion
    }
}
