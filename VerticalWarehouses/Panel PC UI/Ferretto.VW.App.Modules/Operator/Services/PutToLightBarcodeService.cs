﻿using System;
using System.Configuration;
using System.Threading.Tasks;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.MAS.AutomationService.Contracts;
using NLog;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator
{
    internal sealed class PutToLightBarcodeService : IPutToLightBarcodeService
    {
        #region Fields

        private readonly BayNumber bayNumber;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineIdentityWebService identityService;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly IMachinePutToLightWebService putToLightWebService;

        private readonly ISessionService sessionService;

        private string selectedBasketCode;

        private string selectedCarCode;

        private string selectedMachineCode;

        private string selectedShelfCode;

        private UserAction selectedUserAction;

        #endregion

        #region Constructors

        public PutToLightBarcodeService(
            IMachinePutToLightWebService putToLightWebService,
            IMachineIdentityWebService identityService,
            ISessionService sessionService,
            IEventAggregator eventAggregator)
        {
            this.putToLightWebService = putToLightWebService;
            this.identityService = identityService;
            this.eventAggregator = eventAggregator;
            this.sessionService = sessionService;

            this.bayNumber = ConfigurationManager.AppSettings.GetBayNumber();
        }

        #endregion

        #region Methods

        public async Task<bool> ProcessUserActionAsync(UserActionEventArgs e)
        {
            if (e.IsReset)
            {
                this.ResetUserSelection();

                return false;
            }

            switch (e.UserAction)
            {
                case UserAction.AssociateBasketToShelf:
                case UserAction.CompleteBasket:
                case UserAction.RemoveFullBasket:
                case UserAction.CarToMachine:
                case UserAction.CarComplete:

                    this.InitiateUserAction(e);
                    break;

                case UserAction.SelectBasket:

                    await this.SelectBasketAsync(e);
                    break;

                case UserAction.SelectShelf:

                    await this.SelectShelfAsync(e);
                    break;

                case UserAction.SelectCar:

                    await this.SelectCarAsync(e);
                    break;

                case UserAction.SelectMachine:
                    await this.SelectMachineAsync(e);
                    break;

                default:
                    return false;
            }

            return true;
        }

        private async Task AssociateBasketToShelfAsync()
        {
            try
            {
                var machineIdentity = this.sessionService.MachineIdentity;
                if (machineIdentity is null)
                {
                    this.NotifyError(new Exception(Localized.Get("OperatorApp.IdMachineNotDefined")));
                    return;
                }

                var machineId = machineIdentity.Id;

                await this.putToLightWebService.AssociateBasketToShelfAsync(this.selectedBasketCode, this.selectedShelfCode, machineId, (int)this.bayNumber);
                this.NotifySuccess(string.Format(Localized.Get("OperatorApp.BoxAssociateShelf"), this.selectedBasketCode, this.selectedShelfCode));

                this.ResetUserSelection();
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
                this.ResetUserSelection();
            }
        }

        private async Task CarCompleteAsync()
        {
            try
            {
                var machineIdentity = this.sessionService.MachineIdentity;
                if (machineIdentity is null)
                {
                    this.NotifyError(new Exception(Localized.Get("OperatorApp.IdMachineNotDefined")));
                    return;
                }

                var machineId = machineIdentity.Id;
                this.logger.Trace(string.Format(Localized.Get("OperatorApp.CarClosed"), this.selectedCarCode));

                await this.putToLightWebService.CarCompleteAsync(this.selectedCarCode, this.selectedMachineCode, machineId, (int)this.bayNumber);
                this.NotifySuccess(string.Format(Localized.Get("OperatorApp.CarClosed"), this.selectedCarCode));

                this.ResetUserSelection();
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
                this.ResetUserSelection();
            }
        }

        private async Task CarToMachineAsync()
        {
            try
            {
                var machineIdentity = this.sessionService.MachineIdentity;
                if (machineIdentity is null)
                {
                    this.NotifyError(new Exception(Localized.Get("OperatorApp.IdMachineNotDefined")));
                    return;
                }

                var machineId = machineIdentity.Id;
                this.logger.Trace(string.Format(Localized.Get("OperatorApp.CarToMachine"), this.selectedCarCode, this.selectedMachineCode));

                await this.putToLightWebService.CarToMachineAsync(this.selectedCarCode, this.selectedMachineCode, machineId, (int)this.bayNumber);
                this.NotifySuccess(string.Format(Localized.Get("OperatorApp.CarToMachine"), this.selectedCarCode, this.selectedMachineCode));

                this.ResetUserSelection();
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
                this.ResetUserSelection();
            }
        }

        private async Task CompleteBasketAsync()
        {
            try
            {
                var machineIdentity = this.sessionService.MachineIdentity;
                if (machineIdentity is null)
                {
                    this.NotifyError(new Exception(Localized.Get("OperatorApp.IdMachineNotDefined")));
                    return;
                }

                var machineId = machineIdentity.Id;

                await this.putToLightWebService.CompleteBasketAsync(this.selectedBasketCode, this.selectedShelfCode, machineId, (int)this.bayNumber);
                this.NotifySuccess(string.Format(Localized.Get("OperatorApp.BoxClosed"), this.selectedBasketCode));

                this.ResetUserSelection();
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
                this.ResetUserSelection();
            }
        }

        private void InitiateUserAction(UserActionEventArgs e)
        {
            this.selectedUserAction = e.UserAction;
            this.selectedBasketCode = null;
            this.selectedShelfCode = null;
            this.selectedCarCode = null;
            this.selectedMachineCode = null;

            switch (e.UserAction)
            {
                case UserAction.AssociateBasketToShelf:
                    this.NotifyInfo(Localized.Get("OperatorApp.StartAssociateBoxToShelf"), NotificationSeverity.PtlInfoStart);
                    break;

                case UserAction.RemoveFullBasket:
                    this.NotifyInfo(Localized.Get("OperatorApp.StartMarkingFullBox"), NotificationSeverity.PtlInfoStart);
                    break;

                case UserAction.CompleteBasket:
                    this.NotifyInfo(Localized.Get("OperatorApp.StartClosingBox"), NotificationSeverity.PtlInfoStart);
                    break;

                case UserAction.CarToMachine:
                    this.NotifyInfo(Localized.Get("OperatorApp.StartCarToMachine"), NotificationSeverity.PtlInfoStart);
                    break;

                case UserAction.CarComplete:
                    this.NotifyInfo(Localized.Get("OperatorApp.StartClosingCar"), NotificationSeverity.PtlInfoStart);
                    break;
            }
        }

        private void NotifyError(Exception ex)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(ex.Message, Services.Models.NotificationSeverity.PtlError));
        }

        private void NotifyInfo(string message, NotificationSeverity severity = NotificationSeverity.NotSpecified)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(message, severity));
        }

        private void NotifySuccess(string message)
        {
            this.eventAggregator
              .GetEvent<PresentationNotificationPubSubEvent>()
              .Publish(new PresentationNotificationMessage(message, Services.Models.NotificationSeverity.PtlSuccess));
        }

        private void NotifyWarning(string message)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(message, Services.Models.NotificationSeverity.PtlWarning));
        }

        private async Task RemoveFullBasketAsync()
        {
            try
            {
                var machineIdentity = this.sessionService.MachineIdentity;
                if (machineIdentity is null)
                {
                    this.NotifyError(new Exception(Localized.Get("OperatorApp.IdMachineNotDefined")));
                    return;
                }

                var machineId = machineIdentity.Id;

                await this.putToLightWebService.RemoveFullBasketAsync(this.selectedBasketCode, this.selectedShelfCode, machineId, (int)this.bayNumber);
                this.NotifySuccess(string.Format(Localized.Get("OperatorApp.BoxIsFull"), this.selectedBasketCode));

                this.ResetUserSelection();
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
                this.ResetUserSelection();
            }
        }

        private void ResetUserSelection()
        {
            this.selectedUserAction = default(UserAction);
            this.selectedBasketCode = null;
            this.selectedShelfCode = null;
            this.selectedCarCode = null;
            this.selectedMachineCode = null;
        }

        private async Task RunActionAsync()
        {
            switch (this.selectedUserAction)
            {
                case UserAction.AssociateBasketToShelf:

                    if (string.IsNullOrWhiteSpace(this.selectedBasketCode)
                        ||
                        string.IsNullOrWhiteSpace(this.selectedShelfCode))
                    {
                        return;
                    }

                    await this.AssociateBasketToShelfAsync();
                    break;

                case UserAction.CompleteBasket:

                    if (string.IsNullOrWhiteSpace(this.selectedBasketCode)
                        ||
                        string.IsNullOrWhiteSpace(this.selectedShelfCode))
                    {
                        return;
                    }

                    await this.CompleteBasketAsync();
                    break;

                case UserAction.RemoveFullBasket:

                    if (string.IsNullOrWhiteSpace(this.selectedBasketCode)
                        ||
                        string.IsNullOrWhiteSpace(this.selectedShelfCode))
                    {
                        return;
                    }

                    await this.RemoveFullBasketAsync();
                    break;

                case UserAction.CarToMachine:
                    if (string.IsNullOrWhiteSpace(this.selectedMachineCode)
                        ||
                        string.IsNullOrWhiteSpace(this.selectedCarCode))
                    {
                        return;
                    }

                    await this.CarToMachineAsync();
                    break;

                case UserAction.CarComplete:
                    if (string.IsNullOrWhiteSpace(this.selectedMachineCode)
                        ||
                        string.IsNullOrWhiteSpace(this.selectedCarCode))
                    {
                        return;
                    }

                    await this.CarCompleteAsync();
                    break;
            }
        }

        private async Task SelectBasketAsync(UserActionEventArgs e)
        {
            if (this.selectedUserAction is UserAction.NotSpecified
                || this.selectedUserAction is UserAction.CarToMachine
                || this.selectedUserAction is UserAction.CarComplete)
            {
                this.NotifyWarning(Localized.Get("OperatorApp.ScanActionCodeFirst"));

                return;
            }

            if (this.selectedShelfCode == null)
            {
                this.NotifyWarning(Localized.Get("OperatorApp.ScanShelfFirst"));

                return;
            }

            this.selectedBasketCode = e.GetBasketCode();

            if (this.selectedBasketCode is null)
            {
                this.NotifyWarning(Localized.Get("OperatorApp.NoBoxCode"));
            }
            else
            {
                this.NotifyInfo(string.Format(Localized.Get("OperatorApp.SelectedBox"), this.selectedBasketCode), NotificationSeverity.PtlInfo2);
                await this.RunActionAsync();
            }
        }

        private async Task SelectCarAsync(UserActionEventArgs e)
        {
            if (this.selectedUserAction is UserAction.NotSpecified ||
                (this.selectedUserAction != UserAction.CarToMachine && this.selectedUserAction != UserAction.CarComplete))
            {
                this.NotifyWarning(Localized.Get("OperatorApp.ScanActionCodeFirst"));

                return;
            }

            this.selectedMachineCode = null;
            this.selectedCarCode = e.GetCarCode();

            if (this.selectedCarCode is null)
            {
                this.NotifyWarning(Localized.Get("OperatorApp.NoCarCode"));
            }
            else
            {
                this.NotifyInfo(string.Format(Localized.Get("OperatorApp.SelectedCar"), this.selectedCarCode), NotificationSeverity.PtlInfo1);
                await this.RunActionAsync();
            }
        }

        private async Task SelectMachineAsync(UserActionEventArgs e)
        {
            if (this.selectedUserAction is UserAction.NotSpecified ||
                (this.selectedUserAction != UserAction.CarToMachine && this.selectedUserAction != UserAction.CarComplete))
            {
                this.NotifyWarning(Localized.Get("OperatorApp.ScanActionCodeFirst"));

                return;
            }

            if (this.selectedCarCode == null)
            {
                this.NotifyWarning(Localized.Get("OperatorApp.ScanCarFirst"));

                return;
            }

            this.selectedMachineCode = e.GetMachineCode();
            if (this.selectedMachineCode is null)
            {
                this.NotifyWarning(Localized.Get("OperatorApp.NoMachineCode"));
            }
            else
            {
                this.NotifyInfo(string.Format(Localized.Get("OperatorApp.SelectedMachine"), this.selectedMachineCode), NotificationSeverity.PtlInfo2);
                await this.RunActionAsync();
            }
        }

        private async Task SelectShelfAsync(UserActionEventArgs e)
        {
            if (this.selectedUserAction is UserAction.NotSpecified
                || this.selectedUserAction is UserAction.CarToMachine
                || this.selectedUserAction is UserAction.CarComplete)
            {
                this.NotifyWarning(Localized.Get("OperatorApp.ScanActionCodeFirst"));

                return;
            }

            this.selectedBasketCode = null;
            this.selectedShelfCode = e.GetShelfCode();

            if (this.selectedShelfCode is null)
            {
                this.NotifyWarning(Localized.Get("OperatorApp.NoShelfCode"));
            }
            else
            {
                this.NotifyInfo(string.Format(Localized.Get("OperatorApp.SelectedShelf"), this.selectedShelfCode), NotificationSeverity.PtlInfo1);
                await this.RunActionAsync();
            }
        }

        #endregion
    }
}
