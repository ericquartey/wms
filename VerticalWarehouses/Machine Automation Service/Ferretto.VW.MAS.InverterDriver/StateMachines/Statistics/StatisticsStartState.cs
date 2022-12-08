using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Statistics
{
    internal class StatisticsStartState : InverterStateBase
    {
        #region Fields

        private readonly IErrorsProvider errorProvider;

        private ushort averageActivePower;

        private ushort averageRMSCurrent;

        private ushort operationHours;

        private ushort peakHeatSinkTemperature;

        private ushort peakInsideTemperature;

        private DateTime startTime = DateTime.UtcNow;

        private ushort workingHours;

        #endregion

        #region Constructors

        public StatisticsStartState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.errorProvider = this.ParentStateMachine.GetRequiredService<IErrorsProvider>();
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug($"Statistics Start Inverter {this.InverterStatus.SystemIndex}");
            this.startTime = DateTime.UtcNow;

            var message = new InverterMessage(this.InverterStatus.SystemIndex, InverterParameterId.WorkingHours);
            this.Logger.LogTrace($"5:inverterMessage={message}");
            this.ParentStateMachine.EnqueueCommandMessage(message);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Statistics Stop requested");

            this.ParentStateMachine.ChangeState(
                new StatisticsEndState(
                    this.ParentStateMachine,
                    this.InverterStatus,
                    this.Logger));
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            if (message.IsError)
            {
                this.Logger.LogError($"1:StatisticsStartState message={message}");
                this.errorProvider.RecordNew(MachineErrorCode.InverterErrorBaseCode);
                this.ParentStateMachine.ChangeState(new StatisticsErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");
                if (message.ParameterId == InverterParameterId.ResetAverageMemory)
                {
                    if (message.UShortPayload != 0)
                    {
                        short reset = 0;
                        var next = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ResetAverageMemory, reset);
                        this.Logger.LogTrace($"5:inverterMessage={next}");
                        this.ParentStateMachine.EnqueueCommandMessage(next);
                    }
                    else
                    {
                        this.ParentStateMachine.ChangeState(
                            new StatisticsEndState(
                                this.ParentStateMachine,
                                this.InverterStatus,
                                this.Logger));
                    }
                }
            }

            return true;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            var returnValue = false;

            if (message.IsError)
            {
                this.Logger.LogError($"1:StatisticsStartState message={message}");
                this.errorProvider.RecordNew(MachineErrorCode.InverterErrorBaseCode);
                this.ParentStateMachine.ChangeState(new StatisticsErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                InverterMessage next = null;

                switch (message.ParameterId)
                {
                    case InverterParameterId.WorkingHours:
                        this.workingHours = message.UShortPayload;
                        next = new InverterMessage(this.InverterStatus.SystemIndex, InverterParameterId.OperationHours);
                        break;

                    case InverterParameterId.OperationHours:
                        this.operationHours = message.UShortPayload;
                        next = new InverterMessage(this.InverterStatus.SystemIndex, InverterParameterId.PeakHeatSinkTemperature);
                        break;

                    case InverterParameterId.PeakHeatSinkTemperature:
                        this.peakHeatSinkTemperature = message.UShortPayload;
                        next = new InverterMessage(this.InverterStatus.SystemIndex, InverterParameterId.PeakInsideTemperature);
                        break;

                    case InverterParameterId.PeakInsideTemperature:
                        this.peakInsideTemperature = message.UShortPayload;
                        next = new InverterMessage(this.InverterStatus.SystemIndex, InverterParameterId.AverageRMSCurrent);
                        break;

                    case InverterParameterId.AverageRMSCurrent:
                        this.averageRMSCurrent = message.UShortPayload;
                        next = new InverterMessage(this.InverterStatus.SystemIndex, InverterParameterId.AverageActivePower);
                        break;

                    case InverterParameterId.AverageActivePower:
                        this.averageActivePower = message.UShortPayload;
                        var addNew = this.ParentStateMachine.GetRequiredService<IStatisticsDataProvider>().AddInverterStatistics(
                            this.workingHours / 10.0,
                            this.operationHours / 10.0,
                            this.peakHeatSinkTemperature / 10.0,
                            this.peakInsideTemperature / 10.0,
                            this.averageRMSCurrent / 10.0,
                            this.averageActivePower / 10.0);

                        // TODO - remove this always false to send a reset command to the average inverter values
                        if (addNew && false)
                        {
                            short reset = 102;
                            next = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ResetAverageMemory, reset);
                        }
                        else
                        {
                            this.ParentStateMachine.ChangeState(
                                new StatisticsEndState(
                                    this.ParentStateMachine,
                                    this.InverterStatus,
                                    this.Logger));
                        }
                        break;
                }
                if (next != null)
                {
                    this.Logger.LogTrace($"5:inverterMessage={next}");
                    this.ParentStateMachine.EnqueueCommandMessage(next);
                }
            }
            return returnValue;
        }

        #endregion
    }
}
