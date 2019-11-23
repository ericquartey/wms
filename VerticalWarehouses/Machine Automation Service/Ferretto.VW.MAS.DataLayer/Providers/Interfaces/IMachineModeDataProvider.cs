namespace Ferretto.VW.MAS.DataLayer
{
    public interface IMachineModeVolatileDataProvider
    {
        #region Properties

        CommonUtils.Messages.MachineMode Mode { get; set; }

        #endregion
    }
}
