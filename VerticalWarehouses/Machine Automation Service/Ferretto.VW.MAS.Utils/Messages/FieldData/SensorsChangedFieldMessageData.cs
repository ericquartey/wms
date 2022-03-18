using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public sealed class SensorsChangedFieldMessageData : FieldMessageData, ISensorsChangedFieldMessageData
    {
        #region Constructors

        public SensorsChangedFieldMessageData(MessageVerbosity verbosity = MessageVerbosity.Info)
            : base(verbosity)
        {
        }

        #endregion

        #region Properties

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
                            "Performance",
            "CA1819:Properties should not return arrays",
            Justification = "Review the code to see if it is really necessary to return a plain array.")]
        public bool[] SensorsStates { get; set; }

        public bool SensorsStatus { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            var sensorsStates = string.Empty;
            if (this.SensorsStates != null)
            {
                var sb = new StringBuilder();
                foreach (var b in this.SensorsStates)
                {
                    sb.AppendFormat("{0:x2};", b);
                }

                sensorsStates = sb.ToString();
            }

            return $"SensorsStatus(bool):{this.SensorsStatus} SensorsStates:{sensorsStates}";
        }

        #endregion
    }
}
