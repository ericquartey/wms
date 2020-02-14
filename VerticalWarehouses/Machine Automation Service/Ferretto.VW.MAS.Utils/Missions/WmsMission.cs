using System;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;


namespace Ferretto.VW.MAS.Utils.Missions
{
    public class WmsMission<TMachine> : Mission<TMachine>
        where TMachine : class, IFiniteStateMachine
    {
        #region Fields

        private bool disposed;

        #endregion

        #region Constructors

        public WmsMission(IServiceScopeFactory serviceScopeFactory, EventHandler<FiniteStateMachinesEventArgs> endHandler)
            : base(serviceScopeFactory)
        {
            this.CurrentStateMachine.Completed += endHandler;
        }

        #endregion

        #region Properties

        public TMachine MissionMachine => this.CurrentStateMachine;

        #endregion

        #region Methods

        public override bool AllowMultipleInstances(CommandMessage command)
        {
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                // Managed Resources
            }

            // Unmanaged Resources
            this.disposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}
