using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.FiniteStateMachines.Homing.Interfaces
{
    public interface IHomingOperation
    {


        #region Properties

        Axis AxisToCalibrate { get; }

        Axis AxisToCalibrated { get; }

        IEventAggregator EventAggregator { get; }

        bool IsOneKMachine { get; }

        ILogger<FiniteStateMachines> Logger { get; }

        int MaximumSteps { get; }

        int NumberOfExecutedSteps { get; }

        BayIndex RequestingBay { get; }

        IServiceScopeFactory ServiceScopeFactory { get; }

        #endregion
    }
}
