﻿using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class MachineVolatileDataProvider : IMachineVolatileDataProvider
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly Dictionary<BayNumber, double> positions = new Dictionary<BayNumber, double>();

        private readonly IServiceScopeFactory serviceScopeFactory;

        private bool isHomingExecuted;

        private MachineMode mode;

        #endregion

        #region Constructors

        public MachineVolatileDataProvider(
            IServiceScopeFactory serviceScopeFactory,
            IEventAggregator eventAggregator,
            IDataLayerService dataLayerService)
        {
            this.MachinePowerState = MachinePowerState.NotSpecified;
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.eventAggregator = eventAggregator ?? throw new System.ArgumentNullException(nameof(eventAggregator));

            this.IsBayLightOn = new Dictionary<BayNumber, bool>();

            this.IsBayHomingExecuted = new Dictionary<BayNumber, bool>();
            this.IsBayHomingExecuted.Add(BayNumber.BayOne, false);
            this.IsBayHomingExecuted.Add(BayNumber.BayTwo, false);
            this.IsBayHomingExecuted.Add(BayNumber.BayThree, false);
            this.IsBayHomingExecuted.Add(BayNumber.ElevatorBay, false);

            this.positions.Add(BayNumber.BayOne, 0);
            this.positions.Add(BayNumber.BayTwo, 0);
            this.positions.Add(BayNumber.BayThree, 0);

            this.LoadUnitsToTest = null;
            this.LoadUnitsExecutedCycles = null;
            this.BayTestNumber = BayNumber.None;

            this.ElevatorVerticalPositionOld = -10000;

            if (dataLayerService.IsReady)
            {
                this.OnDataLayerReady();
            }
            else
            {
                eventAggregator.GetEvent<NotificationEvent>().Subscribe((x) =>
                    this.OnDataLayerReady(),
                    ThreadOption.PublisherThread,
                    false,
                    m => m.Type is MessageType.DataLayerReady);
            }
        }

        #endregion

        #region Properties

        public BayNumber BayTestNumber { get; set; }

        public double ElevatorHorizontalPosition { get; set; }

        public double ElevatorVerticalPosition { get; set; }

        public double ElevatorVerticalPositionOld { get; set; }

        public int ExecutedCycles { get; set; }

        public bool IsAutomationServiceReady { get; set; }

        public Dictionary<BayNumber, bool> IsBayHomingExecuted { get; set; }

        public Dictionary<BayNumber, bool> IsBayLightOn { get; set; }

        public bool IsDeviceManagerBusy { get; set; }

        public bool IsHomingActive { get; set; }

        // this is a duplicate for IsBayHomingExecuted[BayNumber.ElevatorBay].
        // I keep it only to send the message to the UI
        public bool IsHomingExecuted
        {
            get => this.isHomingExecuted;
            set
            {
                if (this.isHomingExecuted != value)
                {
                    this.isHomingExecuted = value;
                    this.IsBayHomingExecuted[BayNumber.ElevatorBay] = value;

                    // send a message to the UI
                    this.eventAggregator
                        .GetEvent<NotificationEvent>()
                        .Publish(
                            new NotificationMessage
                            {
                                Data = new HomingMessageData(),
                                Destination = MessageActor.AutomationService,
                                Source = MessageActor.DataLayer,
                                Type = MessageType.Homing,
                            });
                }
            }
        }

        public bool IsMachineRunning => (this.MachinePowerState == MachinePowerState.Powered);

        public bool? IsOneTonMachine { get; set; }

        public Dictionary<int, int> LoadUnitsExecutedCycles { get; set; }

        public List<int> LoadUnitsToTest { get; set; }

        public MachinePowerState MachinePowerState { get; set; }

        public MachineMode Mode
        {
            get => this.mode;
            set
            {
                if (this.mode != value)
                {
                    this.mode = value;

                    this.eventAggregator
                        .GetEvent<NotificationEvent>()
                        .Publish(
                            new NotificationMessage
                            {
                                Data = new MachineModeMessageData(this.UiFilteredMode),
                                Destination = MessageActor.Any,
                                Source = MessageActor.DataLayer,
                                Type = MessageType.MachineMode,
                            });
                }
            }
        }

        public int? RequiredCycles { get; set; }

        public bool StopTest { get; set; }

        public MachineMode UiFilteredMode
        {
            get
            {
                switch (this.Mode)
                {
                    case MachineMode.Automatic:
                    case MachineMode.Manual:
                    case MachineMode.LoadUnitOperations:
                    case MachineMode.Test:
                    case MachineMode.Compact:
                        return this.Mode;

                    case MachineMode.FullTest:
                    case MachineMode.FirstTest:
                        return MachineMode.Test;

                    case MachineMode.SwitchingToAutomatic:
                    case MachineMode.SwitchingToManual:
                        return this.Mode;

                    case MachineMode.SwitchingToLoadUnitOperations:
                    case MachineMode.SwitchingToCompact:
                    case MachineMode.SwitchingToFullTest:
                    case MachineMode.SwitchingToFirstTest:
                        return MachineMode.SwitchingToManual;

                    default:
                        return MachineMode.NotSpecified;
                }
            }
        }

        #endregion

        #region Methods

        public double GetBayEncoderPosition(BayNumber bayNumber)
        {
            if (!this.positions.ContainsKey(bayNumber))
            {
                throw new ArgumentOutOfRangeException(nameof(bayNumber));
            }

            return this.positions[bayNumber];
        }

        public void SetBayEncoderPosition(BayNumber bayNumber, double position)
        {
            if (!this.positions.ContainsKey(bayNumber))
            {
                throw new ArgumentOutOfRangeException(nameof(bayNumber));
            }

            if (this.positions[bayNumber] != position)
            {
                this.positions[bayNumber] = position;

                this.eventAggregator
                    .GetEvent<NotificationEvent>()
                    .Publish(
                        new NotificationMessage
                        {
                            Data = new BayChainPositionMessageData(bayNumber, position),
                            Destination = MessageActor.Any,
                            Source = MessageActor.DataLayer,
                            Type = MessageType.BayChainPosition,
                        });
            }
        }

        private void OnDataLayerReady()
        {
            this.IsOneTonMachine = this.IsOneTonMachine ?? this.serviceScopeFactory.CreateScope().ServiceProvider.GetService<IMachineProvider>().IsOneTonMachine();
        }

        #endregion
    }
}
