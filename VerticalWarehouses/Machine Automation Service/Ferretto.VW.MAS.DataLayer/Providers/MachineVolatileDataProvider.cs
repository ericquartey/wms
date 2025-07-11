﻿using System;
using System.Collections.Generic;
using System.Timers;
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

        private const int TIMERINTERVAL = 30000;

        private readonly IEventAggregator eventAggregator;

        private readonly Dictionary<BayNumber, double> positions = new Dictionary<BayNumber, double>();

        private readonly Timer refreshTimer;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private bool isHomingExecuted;

        private MachinePowerState lastMachinePowerState;

        private MachineMode lastMode;

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

            this.IsShutterHomingActive = new Dictionary<BayNumber, bool>();
            this.IsShutterHomingActive.Add(BayNumber.BayOne, false);
            this.IsShutterHomingActive.Add(BayNumber.BayTwo, false);
            this.IsShutterHomingActive.Add(BayNumber.BayThree, false);
            this.IsShutterHomingActive.Add(BayNumber.ElevatorBay, false);

            this.positions.Add(BayNumber.BayOne, 0);
            this.positions.Add(BayNumber.BayTwo, 0);
            this.positions.Add(BayNumber.BayThree, 0);

            this.LoadUnitsToTest = null;
            this.LoadUnitsExecutedCycles = null;
            this.BayTestNumber = BayNumber.None;

            this.ElevatorVerticalPositionOld = -10000;
            this.RandomCells = false;

            this.SocketLinkOperation = new Dictionary<BayNumber, SocketLinkOperation>();
            this.SocketLinkOperation.Add(BayNumber.BayOne, null);
            this.SocketLinkOperation.Add(BayNumber.BayTwo, null);
            this.SocketLinkOperation.Add(BayNumber.BayThree, null);

            this.BayNumbers = new List<BayNumber>();
            this.IsExternal = new Dictionary<BayNumber, bool>();
            this.IsTelescopic = new Dictionary<BayNumber, bool>();

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

            this.refreshTimer = new Timer(TIMERINTERVAL);
            this.refreshTimer.Elapsed += new ElapsedEventHandler(this.timer_Elapsed);
            this.refreshTimer.Enabled = true;
            this.refreshTimer.AutoReset = true;
        }

        #endregion

        #region Properties

        public List<BayNumber> BayNumbers { get; set; }

        public BayNumber BayTestNumber { get; set; }

        public double ElevatorHorizontalPosition { get; set; }

        public double ElevatorVerticalPosition { get; set; }

        public double ElevatorVerticalPositionOld { get; set; }

        public int ExecutedCycles { get; set; }

        public bool IsAutomationServiceReady { get; set; }

        public Dictionary<BayNumber, bool> IsBayHomingExecuted { get; set; }

        public Dictionary<BayNumber, bool> IsBayLightOn { get; set; }

        public bool IsDeviceManagerBusy { get; set; }

        public Dictionary<BayNumber, bool> IsExternal { get; set; }

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

        public bool? IsLoadUnitFixed { get; set; }

        public bool IsMachineRunning => (this.MachinePowerState == MachinePowerState.Powered);

        public bool? IsOneTonMachine { get; set; }

        public bool IsOptimizeRotationClass { get; set; }

        public Dictionary<BayNumber, bool> IsShutterHomingActive { get; set; }

        public bool IsStandbyDbOk { get; set; }

        public Dictionary<BayNumber, bool> IsTelescopic { get; set; }

        public Dictionary<int, int> LoadUnitsExecutedCycles { get; set; }

        public List<int> LoadUnitsToTest { get; set; }

        public int? MachineId { get; set; }

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

        public bool RandomCells { get; set; }

        public int? RequiredCycles { get; set; }

        public Uri ServiceUrl { get; set; }

        public bool? SocketLinkIsEnabled { get; set; }

        public Dictionary<BayNumber, SocketLinkOperation> SocketLinkOperation { get; set; }

        public bool StopTest { get; set; }

        public MachineMode UiFilteredMode
        {
            get
            {
                switch (this.Mode)
                {
                    case MachineMode.Automatic:
                    case MachineMode.Manual:
                    case MachineMode.Manual2:
                    case MachineMode.Manual3:
                    case MachineMode.LoadUnitOperations:
                    case MachineMode.LoadUnitOperations2:
                    case MachineMode.LoadUnitOperations3:
                    case MachineMode.Test:
                    case MachineMode.Test2:
                    case MachineMode.Test3:
                    case MachineMode.Compact:
                    case MachineMode.Compact2:
                    case MachineMode.Compact3:
                    case MachineMode.FastCompact:
                    case MachineMode.FastCompact2:
                    case MachineMode.FastCompact3:
                    case MachineMode.Shutdown:
                        return this.Mode;

                    case MachineMode.FullTest:
                    case MachineMode.FirstTest:
                        return MachineMode.Test;

                    case MachineMode.FullTest2:
                    case MachineMode.FirstTest2:
                        return MachineMode.Test2;

                    case MachineMode.FullTest3:
                    case MachineMode.FirstTest3:
                        return MachineMode.Test3;

                    case MachineMode.SwitchingToAutomatic:
                    case MachineMode.SwitchingToManual:
                    case MachineMode.SwitchingToManual2:
                    case MachineMode.SwitchingToManual3:
                    case MachineMode.SwitchingToShutdown:
                        return this.Mode;

                    case MachineMode.SwitchingToLoadUnitOperations:
                    case MachineMode.SwitchingToCompact:
                    case MachineMode.SwitchingToFastCompact:
                    case MachineMode.SwitchingToFullTest:
                    case MachineMode.SwitchingToFirstTest:
                        return MachineMode.SwitchingToManual;

                    case MachineMode.SwitchingToLoadUnitOperations2:
                    case MachineMode.SwitchingToFullTest2:
                    case MachineMode.SwitchingToFirstTest2:
                        return MachineMode.SwitchingToManual2;

                    case MachineMode.SwitchingToLoadUnitOperations3:
                    case MachineMode.SwitchingToFullTest3:
                    case MachineMode.SwitchingToFirstTest3:
                        return MachineMode.SwitchingToManual3;

                    default:
                        return MachineMode.NotSpecified;
                }
            }
        }

        public int? WMSConnectionTimeout { get; set; }

        public bool? WMSIsConnected { get; set; }

        public bool? WmsIsEnabled { get; set; }

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

        public MachineMode GetMachineModeManualByBayNumber(BayNumber bayNumber)
        {
            switch (bayNumber)
            {
                case BayNumber.BayOne:
                    return MachineMode.Manual;

                case BayNumber.BayTwo:
                    return MachineMode.Manual2;

                case BayNumber.BayThree:
                    return MachineMode.Manual3;

                default:
                    return MachineMode.Manual;
            }
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
            this.IsStandbyDbOk = true;
            //this.IsOneTonMachine = this.IsOneTonMachine ?? this.serviceScopeFactory.CreateScope().ServiceProvider.GetService<IMachineProvider>().IsOneTonMachine();
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.mode == MachineMode.Automatic &&
                this.mode == this.lastMode)
            {
                this.serviceScopeFactory.CreateScope().ServiceProvider.GetService<IMachineProvider>().UpdateTotalAutomaticTime(TimeSpan.FromMilliseconds(TIMERINTERVAL));
            }

            if (this.MachinePowerState == MachinePowerState.Powered &&
                this.lastMachinePowerState == this.MachinePowerState)
            {
                this.serviceScopeFactory.CreateScope().ServiceProvider.GetService<IMachineProvider>().UpdateTotalPowerOnTime(TimeSpan.FromMilliseconds(TIMERINTERVAL));
            }

            this.lastMachinePowerState = this.MachinePowerState;
            this.lastMode = this.mode;

            this.serviceScopeFactory.CreateScope().ServiceProvider.GetService<ILogoutSettingsProvider>().UpdateStatus(TimeSpan.FromMilliseconds(TIMERINTERVAL).TotalMinutes);
            this.serviceScopeFactory.CreateScope().ServiceProvider.GetService<IAutoCompactingSettingsProvider>().UpdateStatus();
        }

        #endregion
    }
}
