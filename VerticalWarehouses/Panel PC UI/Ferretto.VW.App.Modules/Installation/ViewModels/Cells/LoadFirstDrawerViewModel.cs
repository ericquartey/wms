﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class LoadFirstDrawerViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineFirstTestWebService machineFirstTestWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private bool isExecutingProcedure;

        private int loadunitId = 1;

        private SubscriptionToken moveTestToken;

        private DelegateCommand startCommand;

        private int step;

        private int stepPercent;

        private DelegateCommand stopCommand;

        private int totalStep;

        #endregion

        #region Constructors

        public LoadFirstDrawerViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineFirstTestWebService machineFirstTestWebService)
            : base(PresentationMode.Installer)
        {
            this.machineFirstTestWebService = machineFirstTestWebService ?? throw new ArgumentNullException(nameof(machineFirstTestWebService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
        }

        #endregion

        #region Properties

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            set => this.SetProperty(ref this.isExecutingProcedure, value);
        }

        public bool IsLoadunitIdValid
        {
            get
            {
                return this.MachineService.Loadunits.Any(l => l.Id == this.loadunitId);
            }
        }

        public int LoadunitId
        {
            get => this.loadunitId;
            set => this.SetProperty(ref this.loadunitId, value);
        }

        public ICommand StartCommand =>
            this.startCommand
            ??
            (this.startCommand = new DelegateCommand(
                async () =>
                {
                    try
                    {
                        this.IsWaitingForResponse = true;

                        if (!this.IsLoadunitIdValid)
                        {
                            await this.machineLoadingUnitsWebService.InsertLoadingUnitOnlyDbAsync(this.loadunitId);
                        }

                        await this.machineFirstTestWebService.StartAsync(this.LoadunitId);
                    }
                    catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                    {
                        this.ShowNotification(ex);
                    }
                    finally
                    {
                        this.IsWaitingForResponse = false;
                        this.IsExecutingProcedure = true;
                    }
                },
                () => !this.IsMoving));

        public int Step
        {
            get => this.step;
            set => this.SetProperty(ref this.step, value);
        }

        public int StepPercent
        {
            get => this.stepPercent;
            set => this.SetProperty(ref this.stepPercent, value);
        }

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () =>
                    {
                        try
                        {
                            this.IsWaitingForResponse = true;
                            await this.machineFirstTestWebService.StopAsync();
                        }
                        catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                        {
                            this.ShowNotification(ex);
                        }
                        finally
                        {
                            this.IsWaitingForResponse = false;
                            this.IsExecutingProcedure = false;
                        }
                    },
                () => this.IsMoving));

        public int TotalStep
        {
            get => this.totalStep;
            set => this.SetProperty(ref this.totalStep, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            if (this.moveTestToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<MoveTestMessageData>>().Unsubscribe(this.moveTestToken);
                this.moveTestToken?.Dispose();
                this.moveTestToken = null;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            await base.OnAppearedAsync();
        }

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            await base.OnMachinePowerChangedAsync(e);

            if (e.MachinePowerState == MachinePowerState.Unpowered)
            {
                this.IsExecutingProcedure = false;
                this.IsWaitingForResponse = false;
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.stopCommand?.RaiseCanExecuteChanged();
            this.startCommand?.RaiseCanExecuteChanged();
        }

        private void SubscribeToEvents()
        {
            this.moveTestToken = this.moveTestToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<MoveTestMessageData>>()
                    .Subscribe(
                        (m) =>
                        {
                            this.Step = m.Data.ExecutedCycles;
                            this.TotalStep = m.Data.RequiredCycles;
                            this.StepPercent = (int)(((double)this.Step / (double)this.TotalStep) * 100);

                            if (m.Status == CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd ||
                                m.Status == CommonUtils.Messages.Enumerations.MessageStatus.OperationStop)
                            {
                                this.IsExecutingProcedure = false;
                            }
                            else
                            {
                                this.IsExecutingProcedure = true;
                            }
                        },
                        ThreadOption.UIThread,
                        false);
        }

        #endregion
    }
}
