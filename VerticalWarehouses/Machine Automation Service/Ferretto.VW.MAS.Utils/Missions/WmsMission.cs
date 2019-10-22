using System;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ArrangeThisQualifier
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
