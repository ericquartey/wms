using Ferretto.VW.MAS.IODriver.Interface;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines.Reset
{
    public class ResetStartState : IoStateBase
    {
        #region Fields

        private readonly IoSHDStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        public ResetStartState(
            IIoStateMachine parentStateMachine,
            IoSHDStatus status,
            ILogger logger )
            : base( parentStateMachine, logger )
        {
            this.status = status;

            logger.LogTrace( "1:Method Start" );
        }

        #endregion

        #region Destructors

        ~ResetStartState()
        {
            this.Dispose( false );
        }

        #endregion

        #region Methods

        public override void ProcessMessage( IoSHDMessage message )
        {
            this.Logger.LogTrace( $"1:Valid Outputs={message.ValidOutputs}:Outputs cleared={message.OutputsCleared}" );

            if (message.ValidOutputs && message.OutputsCleared)
            {
                this.ParentStateMachine.ChangeState( new ResetEndState( this.ParentStateMachine, this.status, this.Logger ) );
            }
        }

        public override void ProcessResponseMessage( IoSHDReadMessage message )
        {
            this.Logger.LogTrace( $"1:Valid Outputs={message.ValidOutputs}:Outputs cleared={message.OutputsCleared}" );

            var checkMessage = message.FormatDataOperation == Enumerations.SHDFormatDataOperation.Data &&
                message.ValidOutputs && message.OutputsCleared;

            if (this.status.MatchOutputs( message.Outputs ))
            {
                this.ParentStateMachine.ChangeState( new ResetEndState( this.ParentStateMachine, this.status, this.Logger ) );
            }
        }

        public override void Start()
        {
            var resetIoMessage = new IoSHDWriteMessage();
            resetIoMessage.Force = true;

            this.Logger.LogTrace( $"1:Reset IO={resetIoMessage}" );

            lock (this.status)
            {
                this.status.UpdateOutputStates( resetIoMessage.Outputs );
            }
            this.ParentStateMachine.EnqueueMessage( resetIoMessage );
        }

        protected override void Dispose( bool disposing )
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;

            base.Dispose( disposing );
        }

        #endregion
    }
}
