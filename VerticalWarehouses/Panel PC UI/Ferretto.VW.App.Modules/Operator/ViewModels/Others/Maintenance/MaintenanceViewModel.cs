using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Maintenance)]
    public class MaintenanceViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IMachineServicingWebService machineServicingWebService;

        private readonly ISessionService sessionService;

        //private ICommand servicingInfoUp;

        //private ICommand servicingInfoDown;

        private DelegateCommand confirmServiceCommand;

        private bool isConfirmServiceVisible;

        private string machineModel;

        private string machineSerial;

        private string mainteinanceRequest;

        private DelegateCommand maintenanceDetailButtonCommand;

        private ServicingInfo selectedServicingInfo;

        private List<ServicingInfo> servicingInfo;

        private MachineStatistics statistics;

        #endregion

        #region Constructors

        public MaintenanceViewModel(
            ISessionService sessionService,
            IMachineServicingWebService machineServicingWebService)
            : base(PresentationMode.Operator)
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineServicingWebService = machineServicingWebService ?? throw new ArgumentNullException(nameof(machineServicingWebService));
        }

        #endregion

        #region Properties

        public ICommand ConfirmServiceCommand =>
           this.confirmServiceCommand
           ??
           (this.confirmServiceCommand = new DelegateCommand(
              async () => await this.ConfirmServiceAsync(), this.CanConfirmService));

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsConfirmServiceVisible
        {
            get => this.isConfirmServiceVisible;
            set => this.SetProperty(ref this.isConfirmServiceVisible, value);
        }

        public string MachineModel
        {
            get => this.machineModel;
            set => this.SetProperty(ref this.machineModel, value);
        }

        public string MachineSerial
        {
            get => this.machineSerial;
            set => this.SetProperty(ref this.machineSerial, value);
        }

        public string MainteinanceRequest
        {
            get => this.mainteinanceRequest;
            set => this.SetProperty(ref this.mainteinanceRequest, value);
        }

        public ICommand MaintenanceDetailButtonCommand =>
                    this.maintenanceDetailButtonCommand
            ??
            (this.maintenanceDetailButtonCommand = new DelegateCommand(() => this.Detail(), this.CanDetailCommand));

        public ServicingInfo SelectedServicingInfo
        {
            get => this.selectedServicingInfo;
            set
            {
                if (this.SetProperty(ref this.selectedServicingInfo, value, this.RaiseCanExecuteChanged))
                {
                    try
                    {
                        if (this.selectedServicingInfo.ServiceStatus == MachineServiceStatus.Valid)
                        {
                            this.mainteinanceRequest = "Red";
                        }

                        if (this.selectedServicingInfo.ServiceStatus == MachineServiceStatus.Expired)
                        {
                            this.mainteinanceRequest = "Green";
                        }

                        if (this.selectedServicingInfo.ServiceStatus == MachineServiceStatus.Expiring)
                        {
                            this.mainteinanceRequest = "Orange";
                        }

                        if (this.selectedServicingInfo.ServiceStatus == MachineServiceStatus.Completed)
                        {
                            this.mainteinanceRequest = "White";
                        }

                        this.RaisePropertyChanged(nameof(this.MainteinanceRequest));

                        this.GetStatistics();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public List<ServicingInfo> ServicingInfo
        {
            get => this.servicingInfo;
            set => this.SetProperty(ref this.servicingInfo, value);
        }

        public MachineStatistics Statistics
        {
            get => this.statistics;
            set => this.SetProperty(ref this.statistics, value);
        }

        #endregion

        //public ICommand ServicingInfoUp =>
        //    this.servicingInfoUp
        //    ??
        //    (this.servicingInfoUp = new DelegateCommand(
        //        () => this.ServicingInfoUpAction(), this.CanServicingInfoUp));

        #region Methods

        public override void Disappear()
        {
            this.Statistics = null;

            base.Disappear();
        }

        //public ICommand ServicingInfoDown =>
        //    this.servicingInfoDown
        //    ??
        //    (this.servicingInfoDown = new DelegateCommand(
        //        () => this.ServicingInfoDownAction(), this.CanServicingInfoDown));
        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            //this.IsConfirmServiceVisible = this.sessionService.UserAccessLevel == UserAccessLevel.Support;

            //this.RaisePropertyChanged(nameof(this.IsConfirmServiceVisible));

            this.MachineSerial = this.sessionService.MachineIdentity.SerialNumber;

            this.MachineModel = this.sessionService.MachineIdentity.ModelName;

            this.RaisePropertyChanged(nameof(this.MachineSerial));

            this.RaisePropertyChanged(nameof(this.MachineModel));

            var lst = await this.machineServicingWebService.GetAllAsync();

            //this.ServicingInfo = lst.OrderByDescending(o => o.LastServiceDate).ToList();

            this.ServicingInfo = lst.ToList();

            this.RaisePropertyChanged(nameof(this.ServicingInfo));
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.maintenanceDetailButtonCommand?.RaiseCanExecuteChanged();

            base.RaiseCanExecuteChanged();
        }

        private bool CanConfirmService()
        {
            if (true) { return true; }
            else { return false; }
        }

        private bool CanDetailCommand()
        {
            try
            {
                return this.selectedServicingInfo != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //}
        private async Task ConfirmServiceAsync()
        {
            try
            {
                await this.machineServicingWebService.ConfirmServiceAsync();
            }
            catch (Exception)
            {
            }
        }

        private void Detail()
        {
            this.IsWaitingForResponse = true;

            try
            {
                this.NavigationService.Appear(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.Others.Maintenance.DETAIL,
                    this.selectedServicingInfo.Id,
                    trackCurrentView: true);

                //this.NavigationService.Appear(
                //    nameof(Utils.Modules.Operator),
                //    Utils.Modules.Operator.Others.Maintenance.DETAIL,
                //    null,
                //    trackCurrentView: true);
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void GetStatistics()
        {
            try
            {
                this.Statistics = this.selectedServicingInfo.MachineStatistics;
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        //private void ServicingInfoUpAction()
        //{
        //    try
        //    {
        //        var index = this.ServicingInfo.IndexOf(this.SelectedServicingInfo);
        //        this.SelectedServicingInfo = this.ServicingInfo.ElementAt(index++);

        //        this.RaisePropertyChanged(nameof(this.SelectedServicingInfo));
        //    }
        //    catch (Exception)
        //    {
        //    }

        //}

        //private void ServicingInfoDownAction()
        //{
        //    try
        //    {
        //        var index = this.ServicingInfo.IndexOf(this.SelectedServicingInfo);
        //        this.SelectedServicingInfo = this.ServicingInfo.ElementAt(index--);

        //        this.RaisePropertyChanged(nameof(this.SelectedServicingInfo));
        //    }
        //    catch (Exception)
        //    {
        //    }

        //}

        //private bool CanServicingInfoUp()
        //{
        //    try
        //    {
        //        var index = this.ServicingInfo.IndexOf(this.SelectedServicingInfo);
        //        if (index == (this.ServicingInfo.Count() - 1))
        //        {
        //            return false;
        //        }
        //        else
        //        {
        //            return true;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }

        //}

        //private bool CanServicingInfoDown()
        //{
        //    try
        //    {
        //        var index = this.ServicingInfo.IndexOf(this.SelectedServicingInfo);
        //        if (index < 1 && this.SelectedServicingInfo != null)
        //        {
        //            return false;
        //        }
        //        else
        //        {
        //            return true;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
    }
}
