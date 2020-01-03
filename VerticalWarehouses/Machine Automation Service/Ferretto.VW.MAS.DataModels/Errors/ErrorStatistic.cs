using Ferretto.VW.MAS.DataModels.Extensions;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class ErrorStatistic
    {
        #region Properties

        public int Code { get; set; }

        public string Description => ((MachineErrorCode)this.Code).GetDescription();

        public string Reason => ((MachineErrorCode)this.Code).GetReason();

        public int TotalErrors { get; set; }

        #endregion
    }
}
