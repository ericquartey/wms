using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Modules.Operator;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.Utils.Attributes;
using Prism.Commands;
using Ferretto.VW.App.Modules.Operator.Models;
using Ferretto.VW.Utils.Enumerators;
using Ferretto.VW.App.Modules.Menu.Models;
using System.Windows;

namespace Ferretto.VW.App.Menu.ViewModels
{
    public enum InstallationStatus
    {
        Incomplete,

        Complete,

        Inprogress,
    }

    [Warning(WarningsArea.Installation)]
    internal sealed class InstallationMenuViewModel : BaseInstallationMenuViewModel
    {
        #region Fields

        private readonly IMachineSetupStatusWebService machineSetupStatusWebService;

        private DelegateCommand bayCarouselCalibrationBypassCommand;

        private DelegateCommand bayFirstLoadingUnitBypassCommand;

        private DelegateCommand bayHeightCheckBypassCommand;

        private DelegateCommand bayProfileCheckBypassCommand;

        private DelegateCommand bayShutterTestBypassCommand;

        private DelegateCommand beltBurnishingTestBypassCommand;

        private DelegateCommand cellsPanelCheckBypassCommand;

        private int proceduresCompleted;

        private int proceduresCompletedPercent;

        private int proceduresCount;

        private List<ItemListSetupProcedure> source = new List<ItemListSetupProcedure>();

        #endregion

        #region Constructors

        public InstallationMenuViewModel(IMachineSetupStatusWebService machineSetupStatusWebService)
            : base()
        {
            this.machineSetupStatusWebService = machineSetupStatusWebService;
        }

        #endregion

        #region Properties

        public ICommand BayCarouselCalibrationBypassCommand =>
            this.bayCarouselCalibrationBypassCommand
            ??
            (this.bayCarouselCalibrationBypassCommand = new DelegateCommand(
                async () =>
                {
                    try
                    {
                        await this.machineSetupStatusWebService.BayCarouselCalibrationBypassAsync();

                        var r = this.source.First(f => f.Text == "Calibrazione giostra");
                        r.Bypassable = false;
                        r.Bypassed = true;
                        r.Status = InstallationStatus.Complete;
                        this.RaisePropertyChanged(nameof(this.Source));
                    }
                    catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                    {
                        this.ShowNotification(ex);
                    }
                }));

        public ICommand BayFirstLoadingUnitBypassCommand =>
            this.bayFirstLoadingUnitBypassCommand
            ??
            (this.bayFirstLoadingUnitBypassCommand = new DelegateCommand(
                async () =>
                {
                    try
                    {
                        await this.machineSetupStatusWebService.BayFirstLoadingUnitBypassAsync();

                        var r = this.source.First(f => f.Text == InstallationApp.LoadFirstDrawerPageHeader);
                        r.Bypassable = false;
                        r.Bypassed = true;
                        r.Status = InstallationStatus.Complete;
                        this.RaisePropertyChanged(nameof(this.Source));
                    }
                    catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                    {
                        this.ShowNotification(ex);
                    }
                }));

        public ICommand BayHeightCheckBypassCommand =>
            this.bayHeightCheckBypassCommand
            ??
            (this.bayHeightCheckBypassCommand = new DelegateCommand(
                async () =>
                {
                    try
                    {
                        await this.machineSetupStatusWebService.BayHeightCheckBypassAsync();

                        var r = this.source.First(f => f.Text == InstallationApp.BayHeightCheck);
                        r.Bypassable = false;
                        r.Bypassed = true;
                        r.Status = InstallationStatus.Complete;
                        this.RaisePropertyChanged(nameof(this.Source));
                    }
                    catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                    {
                        this.ShowNotification(ex);
                    }
                }));

        public ICommand BayProfileCheckBypassCommand =>
            this.bayProfileCheckBypassCommand
            ??
            (this.bayProfileCheckBypassCommand = new DelegateCommand(
                async () =>
                {
                    try
                    {
                        await this.machineSetupStatusWebService.BayProfileCheckBypassAsync();

                        var r = this.source.First(f => f.Text == InstallationApp.BarrierCalibration);
                        r.Bypassable = false;
                        r.Bypassed = true;
                        r.Status = InstallationStatus.Complete;
                        this.RaisePropertyChanged(nameof(this.Source));
                    }
                    catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                    {
                        this.ShowNotification(ex);
                    }
                }));

        public ICommand BayShutterTestBypassCommand =>
            this.bayShutterTestBypassCommand
            ??
            (this.bayShutterTestBypassCommand = new DelegateCommand(
                async () =>
                {
                    try
                    {
                        await this.machineSetupStatusWebService.BayShutterTestBypassAsync();

                        var r = this.source.First(f => f.Text == "Test serranda");
                        r.Bypassable = false;
                        r.Bypassed = true;
                        r.Status = InstallationStatus.Complete;
                        this.RaisePropertyChanged(nameof(this.Source));
                    }
                    catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                    {
                        this.ShowNotification(ex);
                    }
                }));

        public ICommand BeltBurnishingTestBypassCommand =>
                    this.beltBurnishingTestBypassCommand
            ??
            (this.beltBurnishingTestBypassCommand = new DelegateCommand(
                async () =>
                {
                    try
                    {
                        await this.machineSetupStatusWebService.BeltBurnishingTestBypassAsync();

                        var r = this.source.First(f => f.Text == InstallationApp.BeltBurnishingDone);
                        r.Bypassable = false;
                        r.Bypassed = true;
                        r.Status = InstallationStatus.Complete;
                        this.RaisePropertyChanged(nameof(this.Source));
                    }
                    catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                    {
                        this.ShowNotification(ex);
                    }
                }));

        public ICommand CellsPanelCheckBypassCommand =>
            this.cellsPanelCheckBypassCommand
            ??
            (this.cellsPanelCheckBypassCommand = new DelegateCommand(
                async () =>
                {
                    try
                    {
                        await this.machineSetupStatusWebService.CellsPanelCheckBypassAsync();

                        var r = this.source.First(f => f.Text == InstallationApp.CellsControl);
                        r.Bypassable = false;
                        r.Bypassed = true;
                        r.Status = InstallationStatus.Complete;
                        this.RaisePropertyChanged(nameof(this.Source));
                    }
                    catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                    {
                        this.ShowNotification(ex);
                    }
                }));

        public int ProceduresCompleted
        {
            get => this.proceduresCompleted;
            set => this.SetProperty(ref this.proceduresCompleted, value, this.RaiseCanExecuteChanged);
        }

        public int ProceduresCompletedPercent
        {
            get => this.proceduresCompletedPercent;
            set => this.SetProperty(ref this.proceduresCompletedPercent, value, this.RaiseCanExecuteChanged);
        }

        public int ProceduresCount
        {
            get => this.proceduresCount;
            set => this.SetProperty(ref this.proceduresCount, value, this.RaiseCanExecuteChanged);
        }

        public List<ItemListSetupProcedure> Source
        {
            get
            {
                return this.source;
            }
        }

        #endregion

        #region Methods

        public async override Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            await this.UpdateSetupStatusAsync();
        }

        protected override async Task OnHealthStatusChangedAsync(HealthStatusChangedEventArgs e)
        {
            await base.OnHealthStatusChangedAsync(e);

            this.RaiseCanExecuteChanged();
        }

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            await base.OnMachinePowerChangedAsync(e);

            this.RaiseCanExecuteChanged();
        }

        private async Task UpdateSetupStatusAsync()
        {
            try
            {
                var status = await this.machineSetupStatusWebService.GetAsync();

                BaySetupStatus bayStatus;
                switch (this.MachineService.BayNumber)
                {
                    case BayNumber.BayOne:
                        bayStatus = status.Bay1;
                        break;

                    case BayNumber.BayTwo:
                        bayStatus = status.Bay2;
                        break;

                    case BayNumber.BayThree:
                        bayStatus = status.Bay3;
                        break;

                    default:
                        throw new ArgumentException($"Bay {this.MachineService.BayNumber} not allowed", nameof(this.MachineService.BayNumber));
                }

                this.source = new List<ItemListSetupProcedure>();
                this.source.Add(new ItemListSetupProcedure() { Text = InstallationApp.VerticalAxisHomedDone, Status = status.VerticalOriginCalibration.IsCompleted ? InstallationStatus.Complete : InstallationStatus.Incomplete, Bypassable = false, Bypassed = false, Command = new DelegateCommand(() => { }), });
                this.source.Add(new ItemListSetupProcedure() { Text = InstallationApp.VerticalResolutionDone, Status = status.VerticalResolutionCalibration.IsCompleted ? InstallationStatus.Complete : InstallationStatus.Incomplete, Bypassable = false, Bypassed = false, Command = new DelegateCommand(() => { }), });
                this.source.Add(new ItemListSetupProcedure() { Text = InstallationApp.VerticalOffsetVerify, Status = status.VerticalOffsetCalibration.IsCompleted ? InstallationStatus.Complete : InstallationStatus.Incomplete, Bypassable = false, Bypassed = false, Command = new DelegateCommand(() => { }), });
                this.source.Add(new ItemListSetupProcedure()
                {
                    Text = InstallationApp.BeltBurnishingDone,
                    Status = status.BeltBurnishing.InProgress ? InstallationStatus.Inprogress : status.BeltBurnishing.IsCompleted ? InstallationStatus.Complete : InstallationStatus.Incomplete,
                    Bypassable = !status.BeltBurnishing.IsCompleted,
                    Bypassed = status.BeltBurnishing.IsBypassed,
                    Command = this.BeltBurnishingTestBypassCommand,
                });
                this.source.Add(new ItemListSetupProcedure()
                {
                    Text = InstallationApp.CellsControl,
                    Status = status.CellPanelsCheck.IsCompleted ? InstallationStatus.Complete : InstallationStatus.Incomplete,
                    Bypassable = !status.CellPanelsCheck.IsCompleted,
                    Bypassed = status.CellPanelsCheck.IsBypassed,
                    Command = this.CellsPanelCheckBypassCommand,
                });
                this.source.Add(new ItemListSetupProcedure()
                {
                    Text = InstallationApp.BayHeightCheck,
                    Status = bayStatus.Check.IsCompleted ? InstallationStatus.Complete : InstallationStatus.Incomplete,
                    Bypassable = !bayStatus.Check.IsCompleted,
                    Bypassed = bayStatus.Check.IsBypassed,
                    Command = this.BayHeightCheckBypassCommand,
                });
                this.source.Add(new ItemListSetupProcedure()
                {
                    Text = InstallationApp.BarrierCalibration,
                    Status = bayStatus.Profile.IsCompleted ? InstallationStatus.Complete : InstallationStatus.Incomplete,
                    Bypassable = !bayStatus.Profile.IsCompleted,
                    Bypassed = bayStatus.Profile.IsBypassed,
                    Command = this.BayProfileCheckBypassCommand,
                });

                if (this.MachineService.HasCarousel)
                {
                    this.source.Add(new ItemListSetupProcedure()
                    {
                        Text = "Calibrazione giostra",
                        Status = bayStatus.CarouselCalibration.IsCompleted ? InstallationStatus.Complete : InstallationStatus.Incomplete,
                        Bypassable = !bayStatus.CarouselCalibration.IsCompleted,
                        Bypassed = bayStatus.CarouselCalibration.IsBypassed,
                        Command = this.BayCarouselCalibrationBypassCommand,
                    });
                }

                if (this.MachineService.HasBayExternal)
                {
                    this.source.Add(new ItemListSetupProcedure()
                    {
                        Text = "Test baia esterna",
                        Status = false ? InstallationStatus.Complete : InstallationStatus.Incomplete,
                        Bypassable = false,
                        Bypassed = false,
                        Command = null,
                    });
                }

                if (this.MachineService.HasShutter)
                {
                    this.source.Add(new ItemListSetupProcedure()
                    {
                        Text = "Test serranda",
                        Status = bayStatus.Shutter.InProgress ? InstallationStatus.Inprogress : bayStatus.Shutter.IsCompleted ? InstallationStatus.Complete : InstallationStatus.Incomplete,
                        Bypassable = !bayStatus.Shutter.IsCompleted,
                        Bypassed = bayStatus.Shutter.IsBypassed,
                        Command = this.BayShutterTestBypassCommand,
                    });
                }

                this.source.Add(new ItemListSetupProcedure()
                {
                    Text = "Completare i test sulle altre baie",
                    Status = false ? InstallationStatus.Complete : InstallationStatus.Incomplete,
                    Bypassable = false,
                    Bypassed = false,
                    Command = new DelegateCommand(async () =>
                    {
                    }),
                });

                this.source.Add(new ItemListSetupProcedure()
                {
                    Text = InstallationApp.LoadFirstDrawerPageHeader,
                    Status = bayStatus.FirstLoadingUnit.IsCompleted ? InstallationStatus.Complete : InstallationStatus.Incomplete,
                    Bypassable = !bayStatus.FirstLoadingUnit.IsCompleted,
                    Bypassed = bayStatus.FirstLoadingUnit.IsBypassed,
                    Command = this.BayFirstLoadingUnitBypassCommand,
                });

                this.source.Add(new ItemListSetupProcedure()
                {
                    Text = "Conferma collaudo",
                    Status = status.IsComplete ? InstallationStatus.Complete : InstallationStatus.Incomplete,
                    Bypassable = false,
                    Bypassed = false,
                    Command = new DelegateCommand(() => { }),
                });

                this.ProceduresCount = this.source.Count;
                this.ProceduresCompleted = this.source.Count(c => c.Status == InstallationStatus.Complete);
                this.ProceduresCompletedPercent = (int)((double)this.ProceduresCompleted / (double)this.ProceduresCount * 100.0);

                this.RaisePropertyChanged(nameof(this.Source));
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
