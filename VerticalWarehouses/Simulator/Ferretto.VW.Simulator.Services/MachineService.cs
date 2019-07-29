using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Simulator.Services.Interfaces;
using Microsoft.Extensions.Logging;
using NLog;
using Prism.Mvvm;

namespace Ferretto.VW.Simulator.Services
{
    internal class MachineService : BindableBase, IMachineService
    {
        #region Fields

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Properties

        public bool IsStartedSimulator { get; private set; }

        public bool NotIsStartedSimulator { get => !this.IsStartedSimulator; }

        #endregion

        #region Methods

        /// <summary>
        /// Start simulator, inizialize socket (???)
        /// </summary>
        /// <returns></returns>
        public async Task ProcessStartSimulatorAsync()
        {
            this.Logger.Trace("1:ProcessStartSimulator");

            await Task.Delay(100);

            this.IsStartedSimulator = true;

            this.RaisePropertyChanged(nameof(this.IsStartedSimulator));
            this.RaisePropertyChanged(nameof(this.NotIsStartedSimulator));
        }

        #endregion
    }
}
