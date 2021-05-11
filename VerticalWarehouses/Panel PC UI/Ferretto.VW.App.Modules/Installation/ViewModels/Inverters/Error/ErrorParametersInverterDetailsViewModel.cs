using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Interface;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public struct InverterParameters
    {
        #region Properties

        public short Code { get; set; }

        public string DataSet0 { get; set; }

        public string DataSet1 { get; set; }

        public string DataSet2 { get; set; }

        public string DataSet3 { get; set; }

        public string DataSet4 { get; set; }

        public string Description { get; set; }

        #endregion
    }

    [Warning(WarningsArea.Installation)]
    internal sealed class ErrorParametersInverterDetailsViewModel : BaseMainViewModel
    {
        #region Fields

        public static readonly IList<short> acuActualValues = new ReadOnlyCollection<short>(new List<short> { 259, 269, 273, 1247, 275, 249, 244, 245, 222, 223, 255, 256, 250, 243, 277, 251, 253, 228, 282, 283, 229, 254,
                                                                                                              257, 266, 242, 237, 231, 232, 287, 288, 289, 290, 291, 292, 293, 294, 295, 296, 297, 301, 302, 1121, 29, 0, 1, 12, 16});

        public static readonly IList<short> acuError = new ReadOnlyCollection<short>(new List<short> { 310, 311, 312, 313, 314, 315, 316, 317, 318, 319, 320, 321, 322, 323, 324, 325, 362, 363, 330, 331, 332, 335, 336,
                                                                                                       337, 338, 339, 340, 341, 342, 343, 344, 346, 348, 349, 350, 351, 352, 353, 354, 355, 356, 357, 358, 359, 360, 361, 367, 403});

        public static readonly IList<short> aglActualValues = new ReadOnlyCollection<short>(new List<short> { 259, 269, 273, 275, 1533, 249, 244, 245, 222, 223, 255, 256, 246, 250, 243, 277, 279, 251, 253, 252, 258, 228, 283, 229, 230, 254,
                                                                                                              257, 278, 1530, 210, 224, 211, 212, 238, 226, 213, 214, 239, 240, 241, 242, 231, 232, 287, 288, 289, 290, 291, 292, 298, 299, 293, 294, 295, 296 ,297, 301, 302});

        public static readonly IList<short> aglError = new ReadOnlyCollection<short>(new List<short> { 310, 311, 312, 313, 314, 315, 316, 317, 318, 319, 320, 321, 322, 323, 324, 325, 362, 363, 330, 331, 332, 335, 336,
                                                                                                       337, 338, 339, 340, 341, 342, 343, 344, 346, 348, 349, 350, 351, 352, 353, 354, 355, 356, 357, 358, 359, 360, 361, 367, 403});

        public static readonly IList<short> angActualValues = new ReadOnlyCollection<short>(new List<short> { 259, 269, 273, 1247, 275, 249, 244, 245, 222, 223, 255, 256, 250, 243, 277, 251, 253, 228, 282, 283, 229, 254,
                                                                                                              257, 266, 242, 237, 231, 232, 287, 288, 289, 290, 291, 292, 293, 294, 295, 296, 297, 301, 302, 1121, 29, 0, 1, 12, 16});

        public static readonly IList<short> angError = new ReadOnlyCollection<short>(new List<short> { 310, 311, 312, 313, 314, 315, 316, 317, 318, 319, 320, 321, 322, 323, 324, 325, 362, 363, 330, 331, 332, 333, 334, 335, 336,
                                                                                                       337, 338, 339, 340, 341, 342, 343, 344, 345, 346, 347, 348, 349, 350, 351, 352, 353, 354, 355, 356, 357, 358, 359, 360, 361, 403});

        private readonly List<InverterParameter> error = new List<InverterParameter>();

        private readonly IMachineDevicesWebService machineDevicesWebService;

        private readonly ISessionService sessionService;

        private DelegateCommand actualValueCommand;

        private ObservableCollection<InverterParameter> actualValueParameters = new ObservableCollection<InverterParameter>();

        private DelegateCommand errorCommand;

        private ObservableCollection<InverterParameters> errorParameters = new ObservableCollection<InverterParameters>();

        private SubscriptionToken inverterParameterReceivedToken;

        private Inverter inverterParameters;

        private SubscriptionToken inverterReadingMessageReceivedToken;

        private bool isActualValue;

        private bool isBusy;

        private bool isError;

        private DelegateCommand readInverterCommand;

        private DelegateCommand refreshCommand;

        private InverterType type;

        #endregion

        #region Constructors

        public ErrorParametersInverterDetailsViewModel(
            ISessionService sessionService,
            IMachineDevicesWebService machineDevicesWebService)
            : base(PresentationMode.Installer)
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineDevicesWebService = machineDevicesWebService ?? throw new ArgumentNullException(nameof(machineDevicesWebService));
        }

        #endregion

        #region Properties

        public ICommand ActualValueCommand =>
               this.actualValueCommand
               ??
               (this.actualValueCommand = new DelegateCommand(
                    () =>
                    {
                        this.IsActualValue = true;
                        this.IsError = false;
                    }, this.CanExecuteActualValue));

        public ObservableCollection<InverterParameter> ActualValueParameters
        {
            get => this.actualValueParameters;
            set => this.SetProperty(ref this.actualValueParameters, value, this.RaiseCanExecuteChanged);
        }

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand ErrorCommand =>
               this.errorCommand
               ??
               (this.errorCommand = new DelegateCommand(
                    () =>
                    {
                        this.IsActualValue = false;
                        this.IsError = true;
                    }, this.CanExecuteError));

        public ObservableCollection<InverterParameters> ErrorParameters
        {
            get => this.errorParameters;
            set => this.SetProperty(ref this.errorParameters, value, this.RaiseCanExecuteChanged);
        }

        public bool IsActualValue
        {
            get => this.isActualValue;
            set => this.SetProperty(ref this.isActualValue, value, this.RaiseCanExecuteChanged);
        }

        public bool IsAdmin => this.sessionService.UserAccessLevel == UserAccessLevel.Admin;

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value, this.RaiseCanExecuteChanged);
        }

        public bool IsError
        {
            get => this.isError;
            set => this.SetProperty(ref this.isError, value, this.RaiseCanExecuteChanged);
        }

        public ICommand ReadInverterCommand =>
                                   this.readInverterCommand
               ??
               (this.readInverterCommand = new DelegateCommand(
                   async () => await this.ReadInverterAsync(), this.CanRead));

        public ICommand RefreshCommand =>
               this.refreshCommand
               ??
               (this.refreshCommand = new DelegateCommand(
                async () => await this.RefreshAsync(), this.CanRefresh));

        public InverterType Type
        {
            get => this.type;
            set => this.SetProperty(ref this.type, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.inverterParameters = null;
            this.error.Clear();
            this.errorParameters.Clear();
            this.actualValueParameters.Clear();

            if (this.inverterReadingMessageReceivedToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<InverterReadingMessageData>>().Unsubscribe(this.inverterReadingMessageReceivedToken);
                this.inverterReadingMessageReceivedToken?.Dispose();
                this.inverterReadingMessageReceivedToken = null;
            }

            if (this.inverterParameterReceivedToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<InverterParametersMessageData>>().Unsubscribe(this.inverterParameterReceivedToken);
                this.inverterParameterReceivedToken?.Dispose();
                this.inverterParameterReceivedToken = null;
            }

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsError = true;
            this.RaisePropertyChanged(nameof(this.IsError));

            if (this.Data is Inverter mainConfiguration)
            {
                this.inverterParameters = mainConfiguration;
                this.inverterParameters.Parameters = this.inverterParameters.Parameters.OrderBy(s => s.Code).ThenBy(s => s.DataSet);
                this.Type = this.inverterParameters.Type;
            }

            this.LoadData();

            this.SubscribeEvents();

            await base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.readInverterCommand?.RaiseCanExecuteChanged();
            this.refreshCommand?.RaiseCanExecuteChanged();
            this.errorCommand?.RaiseCanExecuteChanged();
            this.actualValueCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.IsAdmin));
        }

        private bool CanExecuteActualValue()
        {
            return !this.IsBusy &&
                this.IsError &&
                this.MachineService.MachinePower <= MachinePowerState.Unpowered;
        }

        private bool CanExecuteError()
        {
            return !this.IsBusy &&
                !this.IsError &&
                this.MachineService.MachinePower <= MachinePowerState.Unpowered;
        }

        private bool CanRead()
        {
            return !this.IsBusy &&
                this.MachineService.MachinePower <= MachinePowerState.Unpowered &&
                this.inverterParameters != null &&
                this.inverterParameters.Parameters.Any();
        }

        private bool CanRefresh()
        {
            return !this.IsBusy &&
                this.MachineService.MachinePower <= MachinePowerState.Unpowered;
        }

        private void LoadData()
        {
            this.IsBusy = true;

            try
            {

                if (this.inverterParameters.Type == InverterType.Ang)
                {
                    foreach (var parameter in this.inverterParameters.Parameters)
                    {
                        if (angError.Any(s => s == parameter.Code))
                        {
                            if (parameter.DecimalCount > 0 &&
                                parameter.StringValue.Length - parameter.DecimalCount > 0)
                            {
                                parameter.StringValue = parameter.StringValue.Insert(parameter.StringValue.Length - parameter.DecimalCount, ",");
                            }

                            parameter.StringValue += " " + parameter.Um;
                            this.error.Add(parameter);

                            if (this.errorParameters.Any(s => s.Code == parameter.Code))
                            {
                                var parameterError = this.errorParameters.SingleOrDefault(s => s.Code == parameter.Code);
                                this.errorParameters.Remove(parameterError);

                                switch (parameter.DataSet)
                                {
                                    case 0:
                                        parameterError.DataSet0 = parameter.StringValue;
                                        break;

                                    case 1:
                                        parameterError.DataSet1 = parameter.StringValue;
                                        break;

                                    case 2:
                                        parameterError.DataSet2 = parameter.StringValue;
                                        break;

                                    case 3:
                                        parameterError.DataSet3 = parameter.StringValue;
                                        break;

                                    case 4:
                                        parameterError.DataSet4 = parameter.StringValue;
                                        break;
                                }

                                this.errorParameters.Add(parameterError);
                            }
                            else
                            {
                                var parameterToAdd = new InverterParameters();
                                parameterToAdd.Code = (short)parameter.Code;
                                parameterToAdd.Description = parameter.Description;

                                switch (parameter.DataSet)
                                {
                                    case 0:
                                        parameterToAdd.DataSet0 = parameter.StringValue;
                                        break;

                                    case 1:
                                        parameterToAdd.DataSet1 = parameter.StringValue;
                                        break;

                                    case 2:
                                        parameterToAdd.DataSet2 = parameter.StringValue;
                                        break;

                                    case 3:
                                        parameterToAdd.DataSet3 = parameter.StringValue;
                                        break;

                                    case 4:
                                        parameterToAdd.DataSet4 = parameter.StringValue;
                                        break;
                                }

                                this.errorParameters.Add(parameterToAdd);
                            }
                        }
                        else if (angActualValues.Any(s => s == parameter.Code))
                        {
                            if (parameter.DecimalCount > 0 &&
                                parameter.StringValue.Length - parameter.DecimalCount > 0)
                            {
                                parameter.StringValue = parameter.StringValue.Insert(parameter.StringValue.Length - parameter.DecimalCount, ",");
                            }

                            parameter.StringValue += " " + parameter.Um;
                            this.actualValueParameters.Add(parameter);
                        }
                    }
                }
                else if (this.inverterParameters.Type == InverterType.Agl)
                {
                    foreach (var parameter in this.inverterParameters.Parameters)
                    {
                        if (aglError.Any(s => s == parameter.Code))
                        {
                            if (parameter.DecimalCount > 0 &&
                                parameter.StringValue.Length - parameter.DecimalCount > 0)
                            {
                                parameter.StringValue = parameter.StringValue.Insert(parameter.StringValue.Length - parameter.DecimalCount, ",");
                            }

                            parameter.StringValue += " " + parameter.Um;
                            this.error.Add(parameter);

                            if (this.errorParameters.Any(s => s.Code == parameter.Code))
                            {
                                var parameterError = this.errorParameters.SingleOrDefault(s => s.Code == parameter.Code);
                                this.errorParameters.Remove(parameterError);

                                switch (parameter.DataSet)
                                {
                                    case 0:
                                        parameterError.DataSet0 = parameter.StringValue;
                                        break;

                                    case 1:
                                        parameterError.DataSet1 = parameter.StringValue;
                                        break;

                                    case 2:
                                        parameterError.DataSet2 = parameter.StringValue;
                                        break;

                                    case 3:
                                        parameterError.DataSet3 = parameter.StringValue;
                                        break;

                                    case 4:
                                        parameterError.DataSet4 = parameter.StringValue;
                                        break;
                                }

                                this.errorParameters.Add(parameterError);
                            }
                            else
                            {
                                var parameterToAdd = new InverterParameters();
                                parameterToAdd.Code = (short)parameter.Code;
                                parameterToAdd.Description = parameter.Description;

                                switch (parameter.DataSet)
                                {
                                    case 0:
                                        parameterToAdd.DataSet0 = parameter.StringValue;
                                        break;

                                    case 1:
                                        parameterToAdd.DataSet1 = parameter.StringValue;
                                        break;

                                    case 2:
                                        parameterToAdd.DataSet2 = parameter.StringValue;
                                        break;

                                    case 3:
                                        parameterToAdd.DataSet3 = parameter.StringValue;
                                        break;

                                    case 4:
                                        parameterToAdd.DataSet4 = parameter.StringValue;
                                        break;
                                }

                                this.errorParameters.Add(parameterToAdd);
                            }
                        }
                        else if (aglActualValues.Any(s => s == parameter.Code))
                        {
                            if (parameter.DecimalCount > 0 &&
                                parameter.StringValue.Length - parameter.DecimalCount > 0)
                            {
                                parameter.StringValue = parameter.StringValue.Insert(parameter.StringValue.Length - parameter.DecimalCount, ",");
                            }

                            parameter.StringValue += " " + parameter.Um;
                            this.actualValueParameters.Add(parameter);
                        }
                    }
                }
                else if (this.inverterParameters.Type == InverterType.Acu)
                {
                    foreach (var parameter in this.inverterParameters.Parameters)
                    {
                        if (acuError.Any(s => s == parameter.Code))
                        {
                            if (parameter.DecimalCount > 0 &&
                                parameter.StringValue.Length - parameter.DecimalCount > 0)
                            {
                                parameter.StringValue = parameter.StringValue.Insert(parameter.StringValue.Length - parameter.DecimalCount, ",");
                            }

                            parameter.StringValue += " " + parameter.Um;
                            this.error.Add(parameter);

                            if (this.errorParameters.Any(s => s.Code == parameter.Code))
                            {
                                var parameterError = this.errorParameters.SingleOrDefault(s => s.Code == parameter.Code);
                                this.errorParameters.Remove(parameterError);

                                switch (parameter.DataSet)
                                {
                                    case 0:
                                        parameterError.DataSet0 = parameter.StringValue;
                                        break;

                                    case 1:
                                        parameterError.DataSet1 = parameter.StringValue;
                                        break;

                                    case 2:
                                        parameterError.DataSet2 = parameter.StringValue;
                                        break;

                                    case 3:
                                        parameterError.DataSet3 = parameter.StringValue;
                                        break;

                                    case 4:
                                        parameterError.DataSet4 = parameter.StringValue;
                                        break;
                                }

                                this.errorParameters.Add(parameterError);
                            }
                            else
                            {
                                var parameterToAdd = new InverterParameters();
                                parameterToAdd.Code = (short)parameter.Code;
                                parameterToAdd.Description = parameter.Description;

                                switch (parameter.DataSet)
                                {
                                    case 0:
                                        parameterToAdd.DataSet0 = parameter.StringValue;
                                        break;

                                    case 1:
                                        parameterToAdd.DataSet1 = parameter.StringValue;
                                        break;

                                    case 2:
                                        parameterToAdd.DataSet2 = parameter.StringValue;
                                        break;

                                    case 3:
                                        parameterToAdd.DataSet3 = parameter.StringValue;
                                        break;

                                    case 4:
                                        parameterToAdd.DataSet4 = parameter.StringValue;
                                        break;
                                }

                                this.errorParameters.Add(parameterToAdd);
                            }
                        }
                        else if (acuActualValues.Any(s => s == parameter.Code))
                        {
                            if (parameter.DecimalCount > 0 &&
                                parameter.StringValue.Length - parameter.DecimalCount > 0)
                            {
                                parameter.StringValue = parameter.StringValue.Insert(parameter.StringValue.Length - parameter.DecimalCount, ",");
                            }

                            parameter.StringValue += " " + parameter.Um;
                            this.actualValueParameters.Add(parameter);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                this.Logger.Error(ex.ToString());
            }

            this.RaisePropertyChanged(nameof(this.ActualValueParameters));
            this.RaisePropertyChanged(nameof(this.ErrorParameters));

            this.IsBusy = false;
        }

        private void OnInverterParameterReceived(NotificationMessageUI<InverterParametersMessageData> message)
        {
            if (message.Status == CommonUtils.Messages.Enumerations.MessageStatus.OperationStepEnd)
            {
                this.ShowNotification(message.Data.ToString(), Services.Models.NotificationSeverity.Info);
            }
        }

        private void OnInverterReadingMessageReceived(NotificationMessageUI<InverterReadingMessageData> message)
        {
            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    this.IsBusy = false;
                    this.ShowNotification(Localized.Get("InstallationApp.InverterReadingSuccessfullyEnded"), Services.Models.NotificationSeverity.Success);
                    break;

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationError:
                    this.IsBusy = false;
                    this.ShowNotification(Localized.Get("InstallationApp.InverterReadingEndedErrors"), Services.Models.NotificationSeverity.Error);
                    break;

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStart:
                    this.IsBusy = true;
                    this.ShowNotification(Localized.Get("InstallationApp.InverterReadingStarted"), Services.Models.NotificationSeverity.Info);
                    break;

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    this.IsBusy = false;
                    this.ShowNotification(Localized.Get("InstallationApp.InvertersReadingStopped"), Services.Models.NotificationSeverity.Warning);
                    break;

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStepEnd:
                    this.ShowNotification(Localized.Get("InstallationApp.InverterReadingNext"), Services.Models.NotificationSeverity.Info);
                    break;

                default:
                    break;
            }
        }

        private async Task ReadInverterAsync()
        {
            try
            {
                this.ClearNotifications();

                this.IsBusy = true;

                var parameters = this.error;
                parameters.AddRange(this.actualValueParameters);

                this.inverterParameters.Parameters = parameters;

                await this.machineDevicesWebService.ReadInverterParameterAsync(this.inverterParameters);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private async Task RefreshAsync()
        {
            try
            {
                this.IsBusy = true;

                var inverters = await this.machineDevicesWebService.GetInvertersAsync();

                if (inverters.SingleOrDefault(s => s.Index == this.inverterParameters.Index) != null)
                {
                    this.inverterParameters = inverters.SingleOrDefault(s => s.Index == this.inverterParameters.Index);
                    this.inverterParameters.Parameters = this.inverterParameters.Parameters.OrderBy(s => s.Code).ThenBy(s => s.DataSet);

                    this.LoadData();
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private void SubscribeEvents()
        {
            this.inverterReadingMessageReceivedToken = this.inverterReadingMessageReceivedToken
               ?? this.EventAggregator
                   .GetEvent<NotificationEventUI<InverterReadingMessageData>>()
                   .Subscribe(
                       (m) => this.OnInverterReadingMessageReceived(m),
                       ThreadOption.UIThread,
                       false);

            this.inverterParameterReceivedToken = this.inverterParameterReceivedToken
                ?? this.EventAggregator
                    .GetEvent<NotificationEventUI<InverterParametersMessageData>>()
                    .Subscribe(
                        (m) => this.OnInverterParameterReceived(m),
                        ThreadOption.UIThread,
                        false);
        }

        #endregion
    }
}
