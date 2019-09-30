using System;
using System.ComponentModel;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Missions
{
    public class MachineMission : Mission
    {

        #region Fields

        private bool disposed;

        #endregion

        #region Constructors

        public MachineMission(IFiniteStateMachine finiteStateMachine, EventHandler<FiniteStateMachinesEventArgs> endHandler)
        {
            this.currentStateMachine = finiteStateMachine;
            this.currentStateMachine.Completed += endHandler;
            this.id = finiteStateMachine.InstanceId;
        }

        #endregion



        #region Properties

        public Guid Id => this.id;

        public IFiniteStateMachine MissionMachine => this.currentStateMachine;

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
