using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class SensorsChangedMessageData : ISensorsChangedMessageData
    {
        #region Constructors

        public SensorsChangedMessageData()
        {
        }

        #endregion

        #region Properties

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Performance",
            "CA1819:Properties should not return arrays",
            Justification = "Review the code to see if it is really necessary to return a plain array.")]
        public bool[] SensorsStates { get; set; }

        public MessageVerbosity Verbosity => MessageVerbosity.Info;

        #endregion

        #region Methods

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var b in this.SensorsStates)
            {
                sb.AppendFormat("{0:x2};", b);
            }

            return $"SensorsStates:{sb}";
        }

        #endregion
    }
}
