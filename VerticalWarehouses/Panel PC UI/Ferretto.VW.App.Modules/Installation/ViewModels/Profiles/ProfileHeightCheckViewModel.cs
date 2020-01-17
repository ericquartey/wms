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
        /// <summary>
        /// Step 1
        /// </summary>
        Initialize,

        /// <summary>
        /// Step 2
        /// </summary>
        ElevatorPosition,

        /// <summary>
        /// Step 3
        /// </summary>
        DrawerPosition,

        /// <summary>
        /// Step 4
        /// </summary>
        ShapePosition,

        /// <summary>
        /// Step 5
        /// </summary>
        TaraturaCatena,

        /// <summary>
        /// Step 6
        /// </summary>
        ResultCheck,
    }

    [Warning(WarningsArea.Installation)]
    internal sealed class ProfileHeightCheckViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private string currentError;

        private ProfileCheckStep currentStep;

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
            //this[nameof(this.StartPosition)],
            Environment.NewLine,
            Environment.NewLine);

        public bool HasStepDrawerPosition => this.currentStep is ProfileCheckStep.DrawerPosition;

        public bool HasStepElevatorPosition => this.currentStep is ProfileCheckStep.ElevatorPosition;

        public bool HasStepInitialize => this.currentStep is ProfileCheckStep.Initialize;

        public bool HasStepResultCheck => this.currentStep is ProfileCheckStep.ResultCheck;

        public bool HasStepShapePosition => this.currentStep is ProfileCheckStep.ShapePosition;

        public bool HasStepTaraturaCatena => this.currentStep is ProfileCheckStep.TaraturaCatena;

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
                        this.CurrentStep = ProfileCheckStep.DrawerPosition;
                    }
                    else
                    {
                        this.CurrentStep = ProfileCheckStep.Initialize;
                    }

                    break;

                case ProfileCheckStep.DrawerPosition:
                    if (e.Next)
                    {
                        this.CurrentStep = ProfileCheckStep.ShapePosition;
                    }
                    else
                    {
                        this.CurrentStep = ProfileCheckStep.ElevatorPosition;
                    }

                    break;

                case ProfileCheckStep.ShapePosition:
                    if (e.Next)
                    {
                        this.CurrentStep = ProfileCheckStep.TaraturaCatena;
                    }
                    else
                    {
                        this.CurrentStep = ProfileCheckStep.DrawerPosition;
                    }

                    break;

                case ProfileCheckStep.TaraturaCatena:
                    if (e.Next)
                    {
                        this.CurrentStep = ProfileCheckStep.ResultCheck;
                    }
                    else
                    {
                        this.CurrentStep = ProfileCheckStep.ShapePosition;
                    }

                    break;

                case ProfileCheckStep.ResultCheck:
                    if (!e.Next)
                    {
                        this.CurrentStep = ProfileCheckStep.TaraturaCatena;
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
        }

        private bool CanBaseExecute()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsMoving;
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

                case ProfileCheckStep.ElevatorPosition:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, true);
                    break;

                case ProfileCheckStep.DrawerPosition:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, true);
                    break;

                case ProfileCheckStep.ShapePosition:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, true);
                    break;

                case ProfileCheckStep.TaraturaCatena:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, true);
                    break;

                case ProfileCheckStep.ResultCheck:
                    this.ShowPrevStepSinglePage(true, true);
                    this.ShowNextStepSinglePage(true, false);
                    break;
            }

            this.ShowAbortStep(true, true);

            this.RaisePropertyChanged(nameof(this.HasStepInitialize));
            this.RaisePropertyChanged(nameof(this.HasStepElevatorPosition));
            this.RaisePropertyChanged(nameof(this.HasStepDrawerPosition));
            this.RaisePropertyChanged(nameof(this.HasStepShapePosition));
            this.RaisePropertyChanged(nameof(this.HasStepTaraturaCatena));
            this.RaisePropertyChanged(nameof(this.HasStepResultCheck));
        }

        #endregion
    }
}
