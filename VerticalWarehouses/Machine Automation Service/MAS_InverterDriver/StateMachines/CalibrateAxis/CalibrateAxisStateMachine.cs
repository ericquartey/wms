using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_InverterDriver.StateMachines.CalibrateAxis
{
    public class CalibrateAxisStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly IInverterStatusBase inverterStatus;

        private Axis currentAxis;

        private bool disposed;

        #endregion

        #region Constructors

        public CalibrateAxisStateMachine(Axis axisToCalibrate, IInverterStatusBase inverterStatus, BlockingConcurrentQueue<InverterMessage> inverterCommandQueue, IEventAggregator eventAggregator, ILogger logger)
            : base(logger)
        {
            this.Logger.LogTrace("1:Method Start");

            this.axisToCalibrate = axisToCalibrate;
            this.InverterCommandQueue = inverterCommandQueue;
            this.inverterStatus = inverterStatus;
            this.EventAggregator = eventAggregator;
        }

        #endregion

        #region Destructors

        ~CalibrateAxisStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogTrace($"1:Axis to calibrate={this.axisToCalibrate}");

            switch (this.axisToCalibrate)
            {
                case Axis.Both:
                case Axis.Horizontal:
                    this.currentAxis = Axis.Horizontal;
                    break;

                case Axis.Vertical:
                    this.currentAxis = Axis.Vertical;
                    break;
            }

            this.CurrentState = new CalibrateAxisStartState(this, this.currentAxis, this.inverterStatus, this.Logger);
            this.CurrentState?.Start();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}
