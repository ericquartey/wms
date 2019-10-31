namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class MachineModeDataProvider : IMachineModeDataProvider
    {
        #region Properties

        public CommonUtils.Messages.MachineMode Mode { get; set; } = CommonUtils.Messages.MachineMode.Manual;

        #endregion
    }
}
