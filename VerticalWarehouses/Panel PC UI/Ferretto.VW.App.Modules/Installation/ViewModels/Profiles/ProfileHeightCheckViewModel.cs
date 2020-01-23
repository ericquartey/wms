using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using LoadingUnitLocation = Ferretto.VW.MAS.AutomationService.Contracts.LoadingUnitLocation;
using ShutterMovementDirection = Ferretto.VW.MAS.AutomationService.Contracts.ShutterMovementDirection;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public enum ProfileCheckStep
    {
        Initialize,

        ElevatorPosition,

        ShapePositionDx,

        TuningChainDx,

        ShapePositionSx,

        TuningChainSx,

        ResultCheck,
    }

    [Warning(WarningsArea.Installation)]
    internal sealed class ProfileHeightCheckViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private string currentError;

        private ProfileCheckStep currentStep;

        private DelegateCommand moveToElevatorPositionCommand;

        private SubscriptionToken stepChangedToken;

        private DelegateCommand stopCommand;

        #endregion

        #region Constructors

        public ProfileHeightCheckViewModel()
            : base(PresentationMode.Installer)
        {
            this.CurrentStep = ProfileCheckStep.Initialize;
        }

        #endregion

        #region Properties

        public ProfileCheckStep CurrentStep
        {
            get => this.currentStep;
            protected set => this.SetProperty(ref this.currentStep, value, this.UpdateStatusButtonFooter);
        }

        public override EnableMask EnableMask => EnableMask.MachineManualMode | EnableMask.MachinePoweredOn;

        public string Error => string.Join(
            Environment.NewLine,
            this.GetType().GetProperties()
                .Select(p => this[p.Name])
                .Distinct()
                .Where(s => !string.IsNullOrEmpty(s)));

        public bool HasStepElevatorPosition => this.currentStep is ProfileCheckStep.ElevatorPosition;

        public bool HasStepInitialize => this.currentStep is ProfileCheckStep.Initialize;

        public bool HasStepResultCheck => this.currentStep is ProfileCheckStep.ResultCheck;

        public bool HasStepShapePositionDx => this.currentStep is ProfileCheckStep.ShapePositionDx;

        public bool HasStepShapePositionSx => this.currentStep is ProfileCheckStep.ShapePositionSx;

        public bool HasStepTuningChainDx => this.currentStep is ProfileCheckStep.TuningChainDx;

        public bool HasStepTuningChainSx => this.currentStep is ProfileCheckStep.TuningChainSx;

        public ICommand MoveToElevatorPositionCommand =>
            this.moveToElevatorPositionCommand
            ??
            (this.moveToElevatorPositionCommand = new DelegateCommand(
                () => this.CurrentStep = ProfileCheckStep.ElevatorPosition,
                this.CanMoveToElevatorPosition));

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanStop));

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                this.currentError = null;

                if (this.IsWaitingForResponse)
                {
                    return null;
                }

                switch (columnName)
                {
                }

                if (this.IsVisible && string.IsNullOrEmpty(this.currentError))
                {
                    //this.ClearNotifications();
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            if (this.stepChangedToken != null)
            {
                this.EventAggregator.GetEvent<StepChangedPubSubEvent>().Unsubscribe(this.stepChangedToken);
                this.stepChangedToken?.Dispose();
                this.stepChangedToken = null;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            this.UpdateStatusButtonFooter();

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
            }
            catch (HttpRequestException ex)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected void OnStepChanged(StepChangedMessage e)
        {
            switch (this.CurrentStep)
            {
                case ProfileCheckStep.Initialize:
                    if (e.Next)
                    {
                        this.CurrentStep = ProfileCheckStep.ElevatorPosition;
                    }

                    break;

                case ProfileCheckStep.ElevatorPosition:
                    if (e.Next)
                    {
                        this.CurrentStep = ProfileCheckStep.ShapePositionDx;
                    }
                    else
                    {
                        this.CurrentStep = ProfileCheckStep.Initialize;
                    }

                    break;

                case ProfileCheckStep.ShapePositionDx:
                    if (e.Next)
                    {
                        this.CurrentStep = ProfileCheckStep.TuningChainDx;
                    }
                    else
                    {
                        this.CurrentStep = ProfileCheckStep.ElevatorPosition;
                    }

                    break;

                case ProfileCheckStep.TuningChainDx:
                    if (e.Next)
                    {
                        this.CurrentStep = ProfileCheckStep.ShapePositionSx;
                    }
                    else
                    {
                        this.CurrentStep = ProfileCheckStep.ShapePositionDx;
                    }

                    break;

                case ProfileCheckStep.ShapePositionSx:
                    if (e.Next)
                    {
                        this.CurrentStep = ProfileCheckStep.TuningChainSx;
                    }
                    else
                    {
                        this.CurrentStep = ProfileCheckStep.TuningChainDx;
                    }

                    break;

                case ProfileCheckStep.TuningChainSx:
                    if (e.Next)
                    {
                        this.CurrentStep = ProfileCheckStep.ResultCheck;
                    }
                    else
                    {
                        this.CurrentStep = ProfileCheckStep.ShapePositionSx;
                    }

                    break;

                case ProfileCheckStep.ResultCheck:
                    if (!e.Next)
                    {
                        this.CurrentStep = ProfileCheckStep.TuningChainSx;
                    }

                    break;

                default:
                    break;
            }

            this.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.moveToElevatorPositionCommand?.RaiseCanExecuteChanged();
        }

        private bool CanBaseExecute()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsMoving;
        }

        private bool CanMoveToElevatorPosition()
        {
            return this.CanBaseExecute() &&
                   this.SensorsService.IsLoadingUnitInBay;
        }

        private bool CanStop()
        {
            return
                this.IsMoving
                &&
                !this.IsWaitingForResponse;
        }

        private async Task StopAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.MachineService.StopMovingByAllAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void SubscribeToEvents()
        {
            this.stepChangedToken = this.stepChangedToken
                ?? this.EventAggregator
                    .GetEvent<StepChangedPubSubEvent>()
                    .Subscribe(
                        (m) => this.OnStepChanged(m),
                        ThreadOption.UIThread,
                        false);
        }

        private void UpdateStatusButtonFooter()
        {
            switch (this.CurrentStep)
            {
                case ProfileCheckStep.Initialize:
                    this.ShowPrevStepSinglePage(true, false);
                    this.ShowNextStepSinglePage(true, true);
                    break;

                case ProfileCheckStep.ResultCheck:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, false);
                    break;

                default:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, true);
                    break;
            }

            this.ShowAbortStep(true, true);

            this.RaisePropertyChanged(nameof(this.HasStepInitialize));
            this.RaisePropertyChanged(nameof(this.HasStepElevatorPosition));
            this.RaisePropertyChanged(nameof(this.HasStepShapePositionDx));
            this.RaisePropertyChanged(nameof(this.HasStepTuningChainDx));
            this.RaisePropertyChanged(nameof(this.HasStepShapePositionSx));
            this.RaisePropertyChanged(nameof(this.HasStepTuningChainSx));
            this.RaisePropertyChanged(nameof(this.HasStepResultCheck));
        }

        #endregion
    }
}
