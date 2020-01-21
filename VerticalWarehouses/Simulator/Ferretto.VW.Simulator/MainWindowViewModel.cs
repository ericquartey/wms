using System;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.Simulator.Services.Interfaces;
using Ferretto.VW.Simulator.Services.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.Simulator
{
    public class MainWindowViewModel : BindableBase
    {
        #region Fields

        private readonly IThemeService themeService;

        private string errorMessage;

        private IMachineService inverterService;

        private bool isBusy;

        private ICommand startSimulatorCommand;

        private ICommand stopSimulatorCommand;

        private ICommand toggleThemeCommand;

        #endregion

        #region Constructors

        public MainWindowViewModel(
            IMachineService inverterService,
            IThemeService themeService)
        {
            this.inverterService = inverterService ?? throw new ArgumentNullException(nameof(inverterService));
            this.themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        }

        #endregion

        #region Properties

        public string ErrorMessage { get => this.errorMessage; set => this.SetProperty(ref this.errorMessage, value); }

        public IMachineService InverterService
        {
            get => this.inverterService;
            set => this.SetProperty(ref this.inverterService, value);
        }

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        public bool IsDarkThemeActive => this.themeService.ActiveTheme == ApplicationTheme.Dark;

        public ICommand StartSimulatorCommand =>
            this.startSimulatorCommand
            ??
            (this.startSimulatorCommand = new DelegateCommand(async () => await this.inverterService.ProcessStartSimulatorAsync()));

        public ICommand StopSimulatorCommand =>
            this.stopSimulatorCommand
            ??
            (this.stopSimulatorCommand = new DelegateCommand(async () => await this.inverterService.ProcessStopSimulatorAsync()));

        public ICommand ToggleThemeCommand =>
            this.toggleThemeCommand
            ??
            (this.toggleThemeCommand = new DelegateCommand(() => this.ToggleTheme()));

        #endregion

        #region Methods

        private void ToggleTheme()
        {
            this.themeService.ApplyTheme(
                this.themeService.ActiveTheme == ApplicationTheme.Light
                    ? ApplicationTheme.Dark
                    : ApplicationTheme.Light);

            this.RaisePropertyChanged(nameof(this.IsDarkThemeActive));
        }

        #endregion
    }
}
