using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Homing.Interfaces;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines.Homing.Models
{
    public class HomingOperation : IHomingOperation
    {

        #region Fields

        private Axis axisToCalibrate;

        private Axis axisToCalibrated;

        private int maximumSteps;

        private int numberOfExecutedSteps;

        #endregion

        #region Constructors

        public HomingOperation(Axis axisToCalibrate, int numberOfExecutedSteps, int maximumSteps, BayIndex requestingBay)
        {
            this.axisToCalibrate = axisToCalibrate;
            this.numberOfExecutedSteps = numberOfExecutedSteps;
            this.maximumSteps = maximumSteps;
            this.RequestingBay = requestingBay;
        }

        #endregion



        #region Properties

        public Axis AxisToCalibrate
        {
            get => this.axisToCalibrate;
            set
            {
                // set old axis
                if (this.axisToCalibrate != value)
                {
                    this.axisToCalibrated = this.axisToCalibrate;
                }
                this.axisToCalibrate = value;
            }
        }

        public Axis AxisToCalibrated { get => this.axisToCalibrated; set => this.axisToCalibrated = value; }

        public int MaximumSteps { get => this.maximumSteps; set => this.maximumSteps = value; }

        public int NumberOfExecutedSteps { get => this.numberOfExecutedSteps; set => this.numberOfExecutedSteps = value; }

        public BayIndex RequestingBay { get; set; }

        #endregion
    }
}
