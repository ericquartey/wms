using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.Utilities
{
    public static class StopRequestReasonConverter
    {
        #region Methods

        public static MessageStatus GetMessageStatusFromReason(StopRequestReason reason)
        {
            MessageStatus returnValue = MessageStatus.NotSpecified;
            switch (reason)
            {
                case StopRequestReason.NoReason:
                    returnValue = MessageStatus.OperationEnd;
                    break;

                case StopRequestReason.Stop:
                    returnValue = MessageStatus.OperationStop;
                    break;

                case StopRequestReason.Error:
                    returnValue = MessageStatus.OperationError;
                    break;

                case StopRequestReason.FaultStateChanged:
                    returnValue = MessageStatus.OperationFaultStop;
                    break;

                case StopRequestReason.RunningStateChanged:
                    returnValue = MessageStatus.OperationRunningStop;
                    break;
            }

            return returnValue;
        }

        #endregion
    }
}
