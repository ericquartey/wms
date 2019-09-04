using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Homing.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines.Homing.Models
{
    public class HomingMachineData : IHomingMachineData
    {

        #region Fields

        private Axis axisToCalibrate;

        private Axis axisToCalibrated;

        private int maximumSteps;

        private int numberOfExecutedSteps;

        #endregion

        #region Constructors

        public HomingMachineData(
            bool isOneKMachine,
            BayIndex requestingBay,
            IEventAggregator eventAggregator,
            ILogger<FiniteStateMachines> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.IsOneKMachine = isOneKMachine;
            this.RequestingBay = requestingBay;
            this.EventAggregator = eventAggregator;
            this.Logger = logger;
            this.ServiceScopeFactory = serviceScopeFactory;
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

        public IEventAggregator EventAggregator { get; }

        public bool IsOneKMachine { get; }

        public ILogger<FiniteStateMachines> Logger { get; }

        public int MaximumSteps { get => this.maximumSteps; set => this.maximumSteps = value; }

        public int NumberOfExecutedSteps { get => this.numberOfExecutedSteps; set => this.numberOfExecutedSteps = value; }

        public BayIndex RequestingBay { get; set; }

        public IServiceScopeFactory ServiceScopeFactory { get; }

        #endregion
    }
}
