using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public sealed class DiagOutChangedFieldMessageData : FieldMessageData, IDiagOutChangedFieldMessageData
    {
        #region Constructors

        public DiagOutChangedFieldMessageData(MessageVerbosity verbosity = MessageVerbosity.Info)
            : base(verbosity)
        {
        }

        #endregion

        #region Properties

        public int[] CurrentStates { get; set; }

        public bool[] FaultStates { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            var faultStates = string.Empty;
            if (this.FaultStates != null)
            {
                var sb = new StringBuilder();
                foreach (var b in this.FaultStates)
                {
                    sb.AppendFormat("{0:x2};", b);
                }

                faultStates = sb.ToString();
            }

            var currentStates = string.Empty;
            if (this.CurrentStates != null)
            {
                var sb = new StringBuilder();
                foreach (var b in this.CurrentStates)
                {
                    sb.AppendFormat("{0};", b);
                }

                currentStates = sb.ToString();
            }

            return $"OutFaultStates:{faultStates}; OutCurrentStates:{currentStates}";
        }

        #endregion
    }
}
