using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    internal sealed class InverterDiagnosticsViewModel : BaseAboutMenuViewModel
    {
        #region Fields

        private double invEnergy;

        private double invInsideTemp;

        private string invSerialNumber;

        private string invSoftwareVersion;

        private double invSyncTemp;

        private int testValue;

        #endregion

        #region Constructors

        public InverterDiagnosticsViewModel(ISessionService sessionService)
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        }

        #endregion

        #region Properties

        public double InvEnergy
        {
            get => this.invEnergy;
            set => this.SetProperty(ref this.invEnergy, value);
        }

        public double InvInsideTemp
        {
            get => this.invInsideTemp;
            set => this.SetProperty(ref this.invInsideTemp, value);
        }

        public string InvSerialNumber
        {
            get => this.invSerialNumber;
            set => this.SetProperty(ref this.invSerialNumber, value, this.RaiseCanExecuteChanged);
        }

        public string InvSoftwareVersion
        {
            get => this.invSoftwareVersion;
            set => this.SetProperty(ref this.invSoftwareVersion, value);
        }

        public double InvSyncTemp
        {
            get => this.invSyncTemp;
            set => this.SetProperty(ref this.invSyncTemp, value);
        }

        public int TestValue
        {
            get => this.testValue;
            set => this.SetProperty(ref this.testValue, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            try
            {
                await base.OnAppearedAsync();

                this.IsBackNavigationAllowed = true;

                var r = new Random();

                this.InvSerialNumber = "ANG-410193FASBRSW0";
                this.InvSoftwareVersion = "8.1.6.0";
                this.InvEnergy = r.Next(0, 101);
                this.InvInsideTemp = r.Next(0, 101);
                this.InvSyncTemp = r.Next(0, 101);

                this.TestValue = r.Next(0, 101);
            }
            catch
            {
                // do nothing
            }
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                await base.OnDataRefreshAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
