using System.Threading;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning.Interfaces
{
    public interface IShutterPositioningStateMachineData
    {


        #region Properties

        Timer DelayTimer { get; set; }

        IEventAggregator EventAggregator { get; }

        InverterIndex InverterIndex { get; }

        ILogger<FiniteStateMachines> Logger { get; }

        IMachineSensorsStatus MachineSensorsStatus { get; }

        IShutterPositioningMessageData PositioningMessageData { get; }

        BayIndex RequestingBay { get; }

        IServiceScopeFactory ServiceScopeFactory { get; }

        #endregion
    }
}
