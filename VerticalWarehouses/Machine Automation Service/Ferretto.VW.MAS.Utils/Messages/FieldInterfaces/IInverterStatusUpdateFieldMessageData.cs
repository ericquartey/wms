using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterStatusUpdateFieldMessageData : IFieldMessageData
    {
        #region Properties

        Axis CurrentAxis { get; }

        decimal? CurrentPosition { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Performance",
            "CA1819:Properties should not return arrays",
            Justification = "Review the code to see if it is really necessary to return a plain array.")]
        bool[] CurrentSensorStatus { get; }

        DataSample TorqueCurrent { get; }

        #endregion
    }
}
