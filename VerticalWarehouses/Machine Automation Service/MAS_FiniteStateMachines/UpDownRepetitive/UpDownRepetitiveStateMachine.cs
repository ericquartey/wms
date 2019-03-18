using System;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.UpDownRepetitive
{
    public class UpDownRepetitiveStateMachine : StateMachineBase, IUpDownRepetitiveStateMachine
    {
        #region Fields

        private readonly IUpDownRepetitiveMessageData upDownMessageData;

        #endregion

        #region Constructors

        public UpDownRepetitiveStateMachine(IEventAggregator eventAggregator, IUpDownRepetitiveMessageData upDownMessageData)
            : base(eventAggregator)
        {
            this.upDownMessageData = upDownMessageData;
            this.NumberOfRequestedCycles = upDownMessageData.NumberOfRequiredCycles;
        }

        #endregion

        #region Properties

        public IState GetState => this.CurrentState;

        public bool IsStopRequested { get; set; }

        public int NumberOfCompletedCycles { get; set; }

        public int NumberOfRequestedCycles { get; private set; }

        public IUpDownRepetitiveMessageData UpDownRepetitiveData => this.upDownMessageData;

        #endregion

        #region Methods

        public override void Start()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
