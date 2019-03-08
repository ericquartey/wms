using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_FiniteStateMachines.Interface
{
    public interface IHomingStateMachine
    {
        #region Properties

        /// <summary>
        /// Get the <see cref="ICalibrateMessageData"/> message interface.
        /// </summary>
        ICalibrateMessageData CalibrateData { get; }

        /// <summary>
        /// Get the current state. Used for Unit Test
        /// </summary>
        IState GetState { get; }

        /// <summary>
        /// ...
        /// </summary>
        bool IsStopRequested { get; set; }

        /// <summary>
        /// ...
        /// </summary>
        int NMaxSteps { get; }

        /// <summary>
        /// ...
        /// </summary>
        int NumberOfExecutedSteps { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// ...
        /// </summary>
        /// <param name="axisToCalibrate"></param>
        void ChangeAxis(Axis axisToCalibrate);

        #endregion
    }
}
