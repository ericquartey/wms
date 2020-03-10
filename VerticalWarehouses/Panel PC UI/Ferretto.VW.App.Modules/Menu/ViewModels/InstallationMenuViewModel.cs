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

        private readonly Services.IDialogService dialogService;

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly IMachineService machineService;

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

        private bool setupListCompleted;

        private List<ItemListSetupProcedure> source = new List<ItemListSetupProcedure>();

        #endregion

        #region Constructors

        public InstallationMenuViewModel(
            IMachineSetupStatusWebService machineSetupStatusWebService,
            IMachineService machineService,
            IDialogService dialogService,
            IMachineIdentityWebService machineIdentityWebService)
           : base()
        {
            this.machineSetupStatusWebService = machineSetupStatusWebService ?? throw new ArgumentNullException(nameof(machineSetupStatusWebService));
            this.machineService = machineService ?? throw new ArgumentNullException(nameof(machineService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));
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
                    this.IsExecutingProcedure = true;

                    var messageBoxResult = this.dialogService.ShowMessage(InstallationApp.BypassTest, InstallationApp.CarouselCalibration, DialogType.Question, DialogButtons.YesNo);
                    if (messageBoxResult == DialogResult.Yes)
                    {
                        await this.machineSetupStatusWebService.BayCarouselCalibrationBypassAsync();

                        await this.UpdateSetupStatusAsync();
                    }
                }
                catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                {
                    this.ShowNotification(ex);
                }
                finally
                {
                    this.IsExecutingProcedure = false;
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
                        this.IsExecutingProcedure = true;
                        var messageBoxResult = this.dialogService.ShowMessage(InstallationApp.BypassTest, InstallationApp.LoadFirstDrawerPageHeader, DialogType.Question, DialogButtons.YesNo);
                        if (messageBoxResult == DialogResult.Yes)
                        {
                            await this.machineSetupStatusWebService.LoadFirstDrawerTestBypassAsync();

                            await this.UpdateSetupStatusAsync();
                        }
                    }
                    catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                    {
                        this.ShowNotification(ex);
                    }
                    finally
                    {
                        this.IsExecutingProcedure = false;
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
                        this.IsExecutingProcedure = true;

                        var messageBoxResult = this.dialogService.ShowMessage(InstallationApp.BypassTest, InstallationApp.BayHeightCheck, DialogType.Question, DialogButtons.YesNo);
                        if (messageBoxResult == DialogResult.Yes)
                        {
                            await this.machineSetupStatusWebService.BayHeightCheckBypassAsync();

                            await this.UpdateSetupStatusAsync();
                        }
                    }
                    catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                    {
                        this.ShowNotification(ex);
                    }
                    finally
                    {
                        this.IsExecutingProcedure = false;
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
                        this.IsExecutingProcedure = true;
                        var messageBoxResult = this.dialogService.ShowMessage(InstallationApp.BypassTest, InstallationApp.BarrierCalibration, DialogType.Question, DialogButtons.YesNo);
                        if (messageBoxResult == DialogResult.Yes)
                        {
                            await this.machineSetupStatusWebService.BayProfileCheckBypassAsync();

                            await this.UpdateSetupStatusAsync();
                        }
                    }
                    catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                    {
                        this.ShowNotification(ex);
                    }
                    finally
                    {
                        this.IsExecutingProcedure = false;
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
                        this.IsExecutingProcedure = true;
                        var messageBoxResult = this.dialogService.ShowMessage(InstallationApp.BypassTest, InstallationApp.GateControl, DialogType.Question, DialogButtons.YesNo);
                        if (messageBoxResult == DialogResult.Yes)
                        {
                            await this.machineSetupStatusWebService.BayShutterTestBypassAsync();

                            await this.UpdateSetupStatusAsync();
                        }
                    }
                    catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                    {
                        this.ShowNotification(ex);
                    }
                    finally
                    {
                        this.IsExecutingProcedure = false;
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
                        this.IsExecutingProcedure = true;
                        var messageBoxResult = this.dialogService.ShowMessage(InstallationApp.BypassTest, InstallationApp.BeltBurnishing, DialogType.Question, DialogButtons.YesNo);
                        if (messageBoxResult == DialogResult.Yes)
                        {
                            await this.machineSetupStatusWebService.BeltBurnishingTestBypassAsync();

                            await this.UpdateSetupStatusAsync();
                        }
                    }
                    catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                    {
                        this.ShowNotification(ex);
                    }
                    finally
                    {
                        this.IsExecutingProcedure = false;
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
                        this.IsExecutingProcedure = true;
                        var messageBoxResult = this.dialogService.ShowMessage(InstallationApp.BypassTest, InstallationApp.CellsControl, DialogType.Question, DialogButtons.YesNo);
                        if (messageBoxResult == DialogResult.Yes)
                        {
                            await this.machineSetupStatusWebService.CellsPanelCheckBypassAsync();

                            await this.UpdateSetupStatusAsync();
                        }
                    }
                    catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                    {
                        this.ShowNotification(ex);
                    }
                    finally
                    {
                        this.IsExecutingProcedure = false;
                    }
                }));

        public override bool ConfirmSetupVisible => (this.SetupListCompleted && !this.machineService.IsTuningCompleted && !this.IsExecutingProcedure && this.IsGeneralActive);

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

        public bool SetupListCompleted
        {
            get => this.setupListCompleted;
            set => this.SetProperty(ref this.setupListCompleted, value, this.RaiseCanExecuteChanged);
        }

        public List<ItemListSetupProcedure> Source
        {
            get
            {
                return this.source;
            }
        }

        public string SubTitleLabel =>
            this.ProceduresCompletedPercent == 100 ?
            "Stato installazione completato." :
            $"Stato installazione incompleto. Eseguite {this.ProceduresCompleted}/{this.ProceduresCount} procedure.";

        #endregion

        #region Methods

        public override async Task ConfirmSetupAsync()
        {
            try
            {
                this.IsExecutingProcedure = true;

                var messageBoxResult = this.dialogService.ShowMessage(InstallationApp.ConfirmCompleteTest, InstallationApp.ConfirmSetup, DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    await this.machineSetupStatusWebService.ConfirmSetupAsync();

                    await this.UpdateSetupStatusAsync();
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsExecutingProcedure = false;
            }
        }

        public override bool ConfirmSetupEnabled()
        {
            return this.SetupListCompleted && !this.machineService.IsTuningCompleted && !this.IsExecutingProcedure;
        }

        public override void Disappear()
        {
            base.Disappear();

            this.source = new List<ItemListSetupProcedure>();

            this.RaisePropertyChanged(nameof(this.Source));
        }

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

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.Source));
            this.RaisePropertyChanged(nameof(this.SubTitleLabel));
        }

        private async Task UpdateSetupStatusAsync()
        {
            try
            {
                // TODO: Optimize this and next call (make one query to MAS for setup status) - Move everything inside machine service
                await this.MachineService.GetTuningStatus();

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

                var activeOtherBaysNumber = this.MachineService.Bays.Where(b => b.Number != this.MachineService.Bay.Number && b.Number != BayNumber.ElevatorBay).Select(b => b.Number);

                bool otherBaysSetupCompleted = true;
                foreach (BayNumber bayNumber in activeOtherBaysNumber)
                {
                    switch (bayNumber)
                    {
                        case BayNumber.BayOne:
                            otherBaysSetupCompleted &= status.Bay1.IsAllTestCompleted;
                            break;

                        case BayNumber.BayTwo:
                            otherBaysSetupCompleted &= status.Bay2.IsAllTestCompleted;
                            break;

                        case BayNumber.BayThree:
                            otherBaysSetupCompleted &= status.Bay3.IsAllTestCompleted;
                            break;

                        default:
                            throw new ArgumentException($"Bay {this.MachineService.BayNumber} not allowed", nameof(this.MachineService.BayNumber));
                    }
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
                    Status = status.CellPanelsCheck.InProgress ? InstallationStatus.Inprogress : status.CellPanelsCheck.IsCompleted ? InstallationStatus.Complete : InstallationStatus.Incomplete,
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
                        Text = InstallationApp.CarouselCalibration,
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
                        Text = InstallationApp.TestExternalBay,
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
                        Text = InstallationApp.GateControl,
                        Status = bayStatus.Shutter.InProgress ? InstallationStatus.Inprogress : bayStatus.Shutter.IsCompleted ? InstallationStatus.Complete : InstallationStatus.Incomplete,
                        Bypassable = !bayStatus.Shutter.IsCompleted,
                        Bypassed = bayStatus.Shutter.IsBypassed,
                        Command = this.BayShutterTestBypassCommand,
                    });
                }

                this.source.Add(new ItemListSetupProcedure()
                {
                    Text = InstallationApp.LoadFirstDrawerPageHeader,
                    Status = status.LoadFirstDrawerTest.InProgress ? InstallationStatus.Inprogress : status.LoadFirstDrawerTest.IsCompleted ? InstallationStatus.Complete : InstallationStatus.Incomplete,
                    Bypassable = !status.LoadFirstDrawerTest.IsCompleted,
                    Bypassed = status.LoadFirstDrawerTest.IsBypassed,
                    Command = this.BayFirstLoadingUnitBypassCommand,
                });

                if (this.MachineService.Bays.Count(x => x.Number != BayNumber.ElevatorBay) > 1)
                {
                    this.source.Add(new ItemListSetupProcedure()
                    {
                        Text = InstallationApp.CompleteOtherBayTest,
                        Status = otherBaysSetupCompleted ? InstallationStatus.Complete : InstallationStatus.Incomplete,
                        Bypassable = false,
                        Bypassed = false,
                        Command = new DelegateCommand(async () =>
                        {
                        }),
                    });
                }

                this.source.Add(new ItemListSetupProcedure()
                {
                    Text = InstallationApp.ConfirmSetup,
                    Status = this.MachineService.IsTuningCompleted ? InstallationStatus.Complete : InstallationStatus.Incomplete,
                    Bypassable = false,
                    Bypassed = false,
                    Command = new DelegateCommand(() => { }),
                });

                this.ProceduresCount = this.source.Count;

                this.ProceduresCompleted = this.source.Count(c => c.Status == InstallationStatus.Complete);

                this.SetupListCompleted = !this.source.Where(c => c.Text != "Conferma collaudo").Any(c => c.Status != InstallationStatus.Complete);

                this.ProceduresCompletedPercent = (int)((double)this.ProceduresCompleted / (double)this.ProceduresCount * 100.0);

                this.RaiseCanExecuteChanged();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
